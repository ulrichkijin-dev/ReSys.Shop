import os
import sys
import logging
import time
import argparse
import pandas as pd
from pathlib import Path
from datetime import datetime
from sqlalchemy import func
from sqlalchemy.orm import Session
from sqlalchemy.exc import SQLAlchemyError
import traceback
import numpy as np

# Add project root to path
sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.config import settings
from app.database import SessionLocal, ProductImage, Product, ProductClassification, Taxon
from app.dataset_loader import FashionDatasetLoader
from app.model_factory import model_manager
from app.model_mapping import get_embedding_prefix, get_embedding_column
from app.services.search_service import SearchService
from evaluation.metrics import calculate_ap
from evaluation.visualize_metrics import ThesisVisualizer, benchmark_models

def setup_logging(output_dir: Path):
    """Setup comprehensive logging with UTF-8 encoding."""
    log_file = output_dir / "experiment.log"
    
    # Clear existing handlers
    for handler in logging.root.handlers[:]:
        logging.root.removeHandler(handler)
    
    logging.basicConfig(
        level=logging.INFO,
        format='[%(asctime)s] %(levelname)s - %(message)s',
        handlers=[
            logging.FileHandler(log_file, encoding='utf-8'),
            logging.StreamHandler(sys.stdout)
        ]
    )
    return logging.getLogger(__name__)

class ThesisExperimentRunner:
    def __init__(self, sizes: list, models: list, output_root: str):
        self.dataset_sizes = sorted(sizes)
        self.models_to_test = models
        self.timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.base_output_dir = Path(output_root) / f"run_{self.timestamp}"
        self.base_output_dir.mkdir(parents=True, exist_ok=True)
        self.logger = setup_logging(self.base_output_dir)
        
        # Validate models
        self.valid_models = [m for m in models if m in settings.AVAILABLE_MODELS]
        if len(self.valid_models) != len(models):
            invalid = set(models) - set(self.valid_models)
            self.logger.warning(f"Invalid models skipped: {invalid}")

    def run_all(self):
        """Run all experiment iterations."""
        self.logger.info("="*80)
        self.logger.info("FASHION IMAGE SEARCH - THESIS EXPERIMENT RUNNER")
        self.logger.info(f"Models: {self.valid_models}")
        self.logger.info(f"Dataset Sizes: {self.dataset_sizes}")
        self.logger.info("="*80)
        
        try:
            # Pre-warm models
            self.logger.info("🔥 Pre-warming champion models...")
            model_manager.warmup(self.valid_models)
            
            # Run experiments
            for size in self.dataset_sizes:
                try:
                    self.run_single_experiment(size)
                except Exception as e:
                    self.logger.error(f"❌ FAILED experiment for size {size}: {e}")
                    self.logger.error(traceback.format_exc())
            
            self.logger.info(f"\n✅ All experiments completed. Results in: {self.base_output_dir}")
            
        except Exception as e:
            self.logger.critical(f"💥 GLOBAL FAILURE: {e}")
            raise

    def run_single_experiment(self, size: int):
        """Run single experiment for a specific dataset size."""
        self.logger.info(f"\n>>> Starting Experiment: Size={size:,} <<<")
        
        run_dir = self.base_output_dir / f"size_{size}"
        run_dir.mkdir(exist_ok=True)
        
        # Step 1: Load/Sample data
        self.logger.info(f"1. Loading/Clearing database with {size} images...")
        loader = FashionDatasetLoader(
            json_path="data/styles.csv",
            images_dir="data/images",
            total_images=size
        )
        # Using clear_existing=True to isolate this experiment's results
        loader.process_and_load(clear_existing=True)
        
        db = SessionLocal()
        try:
            # Step 2: Accuracy Evaluation
            self.logger.info("2. Evaluating retrieval accuracy (P@K, R@K, mAP)...")
            results = self._evaluate_accuracy(db)
            results_df = pd.DataFrame(results)
            results_df.to_csv(run_dir / "accuracy_metrics.csv", index=False)
            
            # Step 3: Performance Benchmarking
            self.logger.info("3. Benchmarking inference performance...")
            bench_df = benchmark_models(db, sample_size=min(size, 100))
            
            # Merge accuracy (mAP@10) into benchmarks for trade-off analysis
            if not results_df.empty:
                acc_k10 = results_df[results_df['K'] == 10][['Model', 'mAP']]
                bench_df = bench_df.merge(acc_k10, on='Model', how='left')
            bench_df.to_csv(run_dir / "performance_benchmarks.csv", index=False)
            
            # Step 4: Visualization
            self.logger.info("4. Generating thesis-ready visualizations...")
            visualizer = ThesisVisualizer(run_dir)
            visualizer.plot_accuracy_comparison(results_df)
            visualizer.plot_inference_performance(bench_df)
            
            # Generate t-SNE for each model
            for model in self.valid_models:
                try:
                    visualizer.generate_tsne(db, model, n_samples=min(size, 500))
                except Exception as e:
                    self.logger.warning(f"   ⚠ t-SNE skipped for {model}: {e}")
            
            # Summary report
            self._generate_summary(run_dir, size, results_df, bench_df)
            
        finally:
            db.close()

    def _evaluate_accuracy(self, db: Session) -> list:
        """Calculate retrieval metrics across test split."""
        # Sample test queries
        queries = db.query(ProductImage).join(Product).filter(
            Product.public_metadata['split'].astext == 'test',
            ProductImage.type == 'Search'
        ).order_by(func.random()).limit(50).all()

        if not queries:
            self.logger.warning("   ⚠ No test split found, sampling from available search images...")
            queries = db.query(ProductImage).filter(
                ProductImage.type == 'Search'
            ).order_by(func.random()).limit(50).all()

        if not queries:
            return []

        all_results = []
        for model in self.valid_models:
            self.logger.info(f"   🧠 Evaluating accuracy: {model}")
            emb_col = get_embedding_column(model)
            metrics = {k: {"p": [], "r": [], "map": []} for k in [5, 10, 20]}
            
            for q in queries:
                vector = getattr(q, emb_col)
                if vector is None: continue
                
                cat = SearchService.get_categorization(db, q.product_id)
                gt_art = cat["article_type"]
                
                hits = SearchService.search_by_vector(db, vector, model, limit=20, exclude_image_id=q.id)
                rel_mask = [h.article_type == gt_art for h in hits]
                
                total_rel = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(
                    Taxon.name == gt_art, Product.id != q.product_id
                ).scalar() or 0
                
                if total_rel == 0: continue
                
                for k in [5, 10, 20]:
                    k_mask = rel_mask[:k]
                    metrics[k]["p"].append(sum(k_mask) / k)
                    metrics[k]["r"].append(sum(k_mask) / total_rel)
                    metrics[k]["map"].append(calculate_ap(k_mask))
            
            for k in [5, 10, 20]:
                all_results.append({
                    "Model": model, "K": k,
                    "mP": np.mean(metrics[k]["p"]) if metrics[k]["p"] else 0,
                    "mR": np.mean(metrics[k]["r"]) if metrics[k]["r"] else 0,
                    "mAP": np.mean(metrics[k]["map"]) if metrics[k]["map"] else 0
                })
        return all_results

    def _generate_summary(self, run_dir: Path, size: int, results_df: pd.DataFrame, bench_df: pd.DataFrame):
        summary_path = run_dir / "SUMMARY.md"
        with open(summary_path, "w", encoding='utf-8') as f:
            f.write(f"# 📊 Thesis Experiment: {size:,} Items\n\n")
            f.write(f"Generated on {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n\n")
            
            if not results_df.empty:
                f.write("## 🎯 Retrieval Accuracy (@K=10)\n\n")
                k10 = results_df[results_df['K'] == 10].sort_values('mAP', ascending=False)
                f.write(k10.to_markdown(index=False) + "\n\n")
            
            if not bench_df.empty:
                f.write("## ⚡ Inference Performance\n\n")
                f.write(bench_df.to_markdown(index=False) + "\n\n")
        
        self.logger.info(f"Summary written to {summary_path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Full Thesis Experiment Suite")
    parser.add_argument('--sizes', type=int, nargs="+", default=[100, 500])
    parser.add_argument('--output', type=str, default="docs/thesis/results")
    args = parser.parse_args()
    
    runner = ThesisExperimentRunner(
        sizes=args.sizes, 
        models=settings.AVAILABLE_MODELS, 
        output_root=args.output
    )
    runner.run_all()