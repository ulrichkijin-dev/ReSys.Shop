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
from app.model_mapping import get_embedding_prefix
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
        self.dataset_sizes = sorted(sizes)  # Ensure sorted for consistent runs
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
        """Run all experiments with comprehensive error handling."""
        self.logger.info("="*80)
        self.logger.info("THESIS EXPERIMENT RUNNER")
        self.logger.info(f"Experiment ID: {self.timestamp}")
        self.logger.info(f"Valid Models: {self.valid_models}")
        self.logger.info(f"Dataset Sizes: {self.dataset_sizes}")
        self.logger.info(f"Output: {self.base_output_dir}")
        self.logger.info("="*80)
        
        try:
            # Pre-warm models
            self.logger.info("🔥 Pre-warming models...")
            model_manager.warmup(self.valid_models)
            loaded_count = len(model_manager.get_loaded_models())
            self.logger.info(f"✅ Loaded {loaded_count} models")
            
            # Run experiments
            success_count = 0
            for size in self.dataset_sizes:
                try:
                    self.run_single_experiment(size)
                    success_count += 1
                except Exception as e:
                    self.logger.error(f"❌ FAILED size {size}: {e}")
                    self.logger.error(traceback.format_exc())
            
            self.logger.info(f"\n✅ Completed {success_count}/{len(self.dataset_sizes)} experiments")
            self.logger.info(f"📁 Results: {self.base_output_dir.absolute()}")
            
        except Exception as e:
            self.logger.critical(f"💥 GLOBAL FAILURE: {e}")
            self.logger.error(traceback.format_exc())
            raise

    def run_single_experiment(self, size: int):
        """Run single experiment with robust error handling."""
        self.logger.info(f"\n{'='*80}")
        self.logger.info(f"🚀 EXPERIMENT: Dataset Size = {size:,}")
        self.logger.info(f"{'='*80}")
        
        run_dir = self.base_output_dir / f"size_{size}"
        run_dir.mkdir(exist_ok=True)
        
        db = None
        try:
            # Step 1: Load data
            self.logger.info("📥 Step 1/4: Loading dataset...")
            loader = FashionDatasetLoader(
                json_path="data/styles.csv",
                images_dir="data/images",
                total_images=size
            )
            loader.process_and_load(clear_existing=True)
            
            # Step 2: Database session
            db = SessionLocal()
            
            # Step 3: Evaluate accuracy
            self.logger.info("📊 Step 2/4: Evaluating accuracy...")
            results_df = pd.DataFrame(self._evaluate_accuracy(db))
            results_df.to_csv(run_dir / "accuracy_metrics.csv", index=False)
            
            # Step 4: Benchmark performance
            self.logger.info("⚡ Step 3/4: Benchmarking performance...")
            bench_df = benchmark_models(db, sample_size=min(size, 50))
            
            # Merge results
            if not results_df.empty:
                acc_k10 = results_df[results_df['K'] == 10][['Model', 'mAP']]
                bench_df = bench_df.merge(acc_k10, on='Model', how='left')
            bench_df.to_csv(run_dir / "performance_benchmarks.csv", index=False)
            
            # Step 5: Visualize
            self.logger.info("📈 Step 4/4: Generating visualizations...")
            visualizer = ThesisVisualizer(run_dir)
            visualizer.plot_accuracy_comparison(results_df)
            visualizer.plot_inference_performance(bench_df)
            
            # t-SNE visualizations
            for model in self.valid_models:
                try:
                    visualizer.generate_tsne(db, model, n_samples=min(size, 500))
                except Exception as e:
                    self.logger.warning(f"t-SNE failed for {model}: {e}")
            
            # Summary report
            self._generate_summary(run_dir, size, results_df, bench_df)
            
            self.logger.info(f"✅ COMPLETE: {run_dir.absolute()}")
            
        except Exception as e:
            self.logger.error(f"💥 Experiment failed: {e}")
            raise
        finally:
            if db:
                db.close()

    def _evaluate_accuracy(self, db: Session) -> list:
        """Robust accuracy evaluation with proper error handling."""
        try:
            # Get test queries
            queries = db.query(ProductImage).join(Product).filter(
                Product.public_metadata['split'].astext == 'test',
                ProductImage.type == 'Search'
            ).order_by(func.random()).limit(100).all()

            if not queries:
                self.logger.warning("No test split found, using random sample...")
                queries = db.query(ProductImage).filter(
                    ProductImage.type == 'Search'
                ).order_by(func.random()).limit(50).all()

            if not queries:
                self.logger.error("No queries available!")
                return []

            self.logger.info(f"Evaluating {len(queries)} queries across {len(self.valid_models)} models")
            
            all_results = []
            for model in self.valid_models:
                self.logger.info(f"  🧠 Evaluating {model}...")
                results = self._evaluate_single_model(db, model, queries)
                all_results.extend(results)
            
            return all_results
            
        except SQLAlchemyError as e:
            self.logger.error(f"Database error in evaluation: {e}")
            return []

    def _evaluate_single_model(self, db: Session, model: str, queries) -> list:
        """Evaluate single model with consistent prefix mapping."""
        prefix = get_embedding_prefix(model)
        emb_col = f"embedding_{prefix}"
        
        metrics = {k: {"p": [], "r": [], "map": []} for k in [5, 10, 20]}
        evaluated_count = 0
        
        for q in queries:
            try:
                vector = getattr(q, emb_col)
                if vector is None:
                    continue
                
                meta = SearchService.get_categorization(db, q.product_id)
                gt_art = meta.get("article_type")
                if not gt_art:
                    continue
                
                # Search
                hits = SearchService.search_by_vector(
                    db, vector, model, limit=20, exclude_image_id=q.id
                )
                rel_mask = [h.article_type == gt_art for h in hits]
                
                # Total relevant count
                total_rel = db.query(func.count(Product.id)).join(
                    ProductClassification
                ).join(Taxon).filter(
                    Taxon.name == gt_art, 
                    Product.id != q.product_id
                ).scalar() or 0
                
                if total_rel == 0:
                    continue
                
                # Calculate metrics
                for k in [5, 10, 20]:
                    k_mask = rel_mask[:k]
                    metrics[k]["p"].append(sum(k_mask) / k)
                    metrics[k]["r"].append(sum(k_mask) / total_rel)
                    metrics[k]["map"].append(calculate_ap(k_mask))
                
                evaluated_count += 1
                
            except Exception as e:
                self.logger.debug(f"Query {q.id} failed: {e}")
                continue
        
        self.logger.info(f"    Processed {evaluated_count}/{len(queries)} queries")
        
        # Aggregate
        results = []
        for k in [5, 10, 20]:
            results.append({
                "Model": model, 
                "K": k,
                "mP": np.mean(metrics[k]["p"]) if metrics[k]["p"] else 0,
                "mR": np.mean(metrics[k]["r"]) if metrics[k]["r"] else 0,
                "mAP": np.mean(metrics[k]["map"]) if metrics[k]["map"] else 0
            })
        return results

    def _generate_summary(self, run_dir: Path, size: int, results_df: pd.DataFrame, bench_df: pd.DataFrame):
        """Generate comprehensive markdown summary."""
        summary_path = run_dir / "SUMMARY.md"
        
        with open(summary_path, "w", encoding='utf-8') as f:
            f.write(f"# 📊 Experiment Summary: {size:,} Items\n\n")
            f.write(f"**Run:** {datetime.now().strftime('%Y-%m-%d %H:%M:%S')}\n")
            f.write(f"**Models:** {', '.join(self.valid_models)}\n\n")
            
            if not results_df.empty:
                f.write("## 🎯 Accuracy @ K=10\n\n")
                k10 = results_df[results_df['K'] == 10].sort_values('mAP', ascending=False)
                f.write(k10.round(4).to_markdown(index=False))
                
                best_model = k10.iloc[0]
                f.write(f"\n\n**🏆 Best: {best_model['Model']} (mAP={best_model['mAP']:.4f})**\n")
            
            if not bench_df.empty:
                f.write("\n\n## ⚡ Performance\n\n")
                f.write(bench_df.round(2).to_markdown(index=False))
                
                fastest = bench_df.nsmallest(1, 'Avg Inference (ms)').iloc[0]
                f.write(f"\n\n**🚀 Fastest: {fastest['Model']} ({fastest['Avg Inference (ms)']:.1f}ms)**\n")
        
        self.logger.info(f"📋 Summary: {summary_path}")

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Thesis Experiment Runner")
    parser.add_argument('--sizes', type=int, nargs="+", default=[100, 500, 1000])
    parser.add_argument('--models', type=str, nargs="+", 
                       default=settings.AVAILABLE_MODELS)
    parser.add_argument('--output', type=str, default="results/thesis")
    args = parser.parse_args()
    
    runner = ThesisExperimentRunner(args.sizes, args.models, args.output)
    runner.run_all()
