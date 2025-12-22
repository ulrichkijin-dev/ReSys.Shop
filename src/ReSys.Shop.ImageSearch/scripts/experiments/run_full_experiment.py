import os
import sys
import logging
import time
import argparse
import pandas as pd
from pathlib import Path
from datetime import datetime
from sqlalchemy.orm import Session
import traceback

# Add project root to path
sys.path.append(str(Path(__file__).resolve().parent.parent.parent))

from app.config import settings
from app.database import SessionLocal
from app.dataset_loader import FashionDatasetLoader
from app.model_factory import model_manager
from evaluation.metrics import calculate_ap
from evaluation.visualize_metrics import ThesisVisualizer, benchmark_models

def setup_logging(output_dir: Path):
    """Setup logging with UTF-8 encoding to prevent Windows console errors."""
    log_file = output_dir / "experiment.log"
    
    # Configure root logger
    logger = logging.getLogger()
    logger.setLevel(logging.INFO)
    
    # File handler with UTF-8
    fh = logging.FileHandler(log_file, encoding='utf-8')
    fh.setFormatter(logging.Formatter('[%(asctime)s] %(levelname)s - %(message)s'))
    logger.addHandler(fh)
    
    # Console handler - we'll be careful with emojis here
    ch = logging.StreamHandler(sys.stdout)
    ch.setFormatter(logging.Formatter('%(levelname)s - %(message)s'))
    logger.addHandler(ch)
    
    return logging.getLogger(__name__)

class ThesisExperimentRunner:
    def __init__(self, sizes: list, models: list, output_root: str):
        self.dataset_sizes = sizes
        self.models_to_test = models
        self.timestamp = datetime.now().strftime("%Y%m%d_%H%M%S")
        self.base_output_dir = Path(output_root) / f"run_{self.timestamp}"
        self.base_output_dir.mkdir(parents=True, exist_ok=True)
        self.logger = setup_logging(self.base_output_dir)

    def run_all(self):
        self.logger.info(f"Starting Thesis Experiments (ID: {self.timestamp})")
        self.logger.info(f"Target Models: {self.models_to_test}")
        self.logger.info(f"Dataset Sizes: {self.dataset_sizes}")
        
        try:
            self.logger.info("Pre-warming models...")
            model_manager.warmup(self.models_to_test)
            
            for size in self.dataset_sizes:
                try:
                    self.run_single_experiment(size)
                except Exception as e:
                    self.logger.error(f"Failed experiment for size {size}: {e}")
                    self.logger.error(traceback.format_exc())
                    
            self.logger.info(f"All experiments finished. Results in {self.base_output_dir}")
        except Exception as e:
            self.logger.critical(f"Global experiment failure: {e}")
            self.logger.error(traceback.format_exc())

    def run_single_experiment(self, size: int):
        self.logger.info(f"\n{'='*60}\nRUNNING SIZE: {size}\n{'='*60}")
        
        run_dir = self.base_output_dir / f"size_{size}"
        run_dir.mkdir(exist_ok=True)
        
        # 1. Load Data
        self.logger.info(f"Step 1: Loading {size} products...")
        loader = FashionDatasetLoader(
            json_path="data/styles.csv",
            images_dir="data/images",
            total_images=size
        )
        loader.process_and_load(clear_existing=True)
        
        # 2. Evaluation
        db = SessionLocal()
        try:
            self.logger.info("Step 2: Evaluating Accuracy...")
            results_list = self._evaluate_accuracy(db)
            results_df = pd.DataFrame(results_list)
            results_df.to_csv(run_dir / "accuracy_metrics.csv", index=False)
            
            # 3. Benchmark
            self.logger.info("Step 3: Benchmarking Efficiency...")
            bench_df = benchmark_models(db, sample_size=min(size, 50))
            
            # Combine accuracy and benchmark for efficiency plot
            acc_k10 = results_df[results_df['K'] == 10][['Model', 'mAP']]
            bench_df = bench_df.merge(acc_k10, on='Model')
            bench_df.to_csv(run_dir / "performance_benchmarks.csv", index=False)
            
            # 4. Visualization
            self.logger.info("Step 4: Generating Visualizations...")
            visualizer = ThesisVisualizer(run_dir)
            visualizer.plot_accuracy_comparison(results_df)
            visualizer.plot_inference_performance(bench_df)
            
            for model in self.models_to_test:
                visualizer.generate_tsne(db, model, n_samples=min(size, 500))
                
            self._generate_summary(run_dir, size, results_df, bench_df)
            self.logger.info(f"Success for size {size}")
            
        finally:
            db.close()

    def _evaluate_accuracy(self, db: Session):
        from app.services.search_service import SearchService
        from app.database import ProductImage, Product, ProductClassification, Taxon
        from sqlalchemy import func
        import numpy as np

        queries = db.query(ProductImage).join(Product).filter(
            Product.public_metadata['split'].astext == 'test',
            ProductImage.type == 'Search'
        ).order_by(func.random()).limit(100).all()

        if not queries:
            self.logger.warning("No 'test' split queries found. Falling back to random sample for testing...")
            queries = db.query(ProductImage).filter(
                ProductImage.type == 'Search'
            ).order_by(func.random()).limit(50).all()

        if not queries:
            self.logger.warning("No queries found at all. Skipping accuracy evaluation.")
            return []

        all_results = []
        for model in self.models_to_test:
            self.logger.info(f"  Evaluating accuracy for {model}...")
            metrics = {k: {"p": [], "r": [], "map": []} for k in [5, 10, 20]}
            
            # Robust mapping consistent with main.py and SearchService
            prefix_map = {
                "efficientnet_b0": "efficientnet",
                "convnext_tiny": "convnext",
                "clip_vit_b16": "clip",
                "fashion_clip": "fclip",
                "dino_vit_s16": "dino",
                "resnet50": "resnet",
                "mobilenet_v3": "mobilenet"
            }
            prefix = prefix_map.get(model, model.split('_')[0])
            emb_col = f"embedding_{prefix}"

            for q in queries:
                meta = SearchService.get_categorization(db, q.product_id)
                gt_art = meta["article_type"]
                
                vector = getattr(q, emb_col)
                if vector is None: continue

                hits = SearchService.search_by_vector(db, vector, model, limit=20, exclude_image_id=q.id)
                rel_mask = [h.article_type == gt_art for h in hits]

                total_rel = db.query(func.count(Product.id)).join(ProductClassification).join(Taxon).filter(
                    Taxon.name == gt_art, Product.id != q.product_id
                ).scalar()
                
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

    def _generate_summary(self, run_dir, size, results_df, bench_df):
        summary_path = run_dir / "experiment_summary.md"
        with open(summary_path, "w", encoding='utf-8') as f:
            f.write(f"# Experiment Summary: Size {size}\n\n")
            f.write(f"Generated at: {datetime.now()}\n\n")
            f.write("## Accuracy Metrics (K=10)\n")
            f.write(results_df[results_df['K'] == 10].to_markdown(index=False))
            f.write("\n\n## Performance Benchmarks\n")
            f.write(bench_df.to_markdown(index=False))

if __name__ == "__main__":
    parser = argparse.ArgumentParser(description="Thesis Experiment Runner")
    parser.add_argument("--sizes", type=int, nargs="+", default=[100, 500], help="Dataset sizes to test")
    parser.add_argument("--models", type=str, nargs="+", default=settings.AVAILABLE_MODELS, help="Models to evaluate")
    parser.add_argument("--output", type=str, default="docs/thesis/results", help="Output directory root")
    
    args = parser.parse_args()
    
    runner = ThesisExperimentRunner(sizes=args.sizes, models=args.models, output_root=args.output)
    runner.run_all()
