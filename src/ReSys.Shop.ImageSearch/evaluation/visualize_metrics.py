import logging
import os
import uuid
import time
import psutil
import torch
import numpy as np
import pandas as pd
import matplotlib.pyplot as plt
import seaborn as sns
from sklearn.manifold import TSNE
from collections import defaultdict
from sqlalchemy.orm import Session
from sqlalchemy import text, func
import sys
from pathlib import Path

# Add project root to path
sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.config import settings
from app.database import get_db, Product, ProductImage
from app.model_factory import model_manager
from app.model_mapping import get_embedding_prefix, get_embedding_column
from app.services.search_service import SearchService

# Setup logging
logger = logging.getLogger(__name__)

# Setup plotting style for Thesis
sns.set_theme(style="whitegrid", context="paper", font_scale=1.2)
plt.rcParams["figure.figsize"] = (10, 6)
plt.rcParams["savefig.dpi"] = 300


class ThesisVisualizer:
    def __init__(self, output_dir: Path):
        self.output_dir = output_dir
        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.metrics_dir = output_dir / "plots"
        self.metrics_dir.mkdir(exist_ok=True)

        # The 5 champion models grouped for thesis comparison
        self.model_groups = {
            "CNN": ["efficientnet_b0", "convnext_tiny"],
            "Transformer": ["clip_vit_b16", "fashion_clip", "dinov2_vits14"],
        }

    def plot_accuracy_comparison(self, results_df: pd.DataFrame):
        """Generates mAP comparison bar chart."""
        if results_df.empty:
            logger.warning("Empty results dataframe, skipping plot.")
            return
            
        plt.figure(figsize=(12, 6))
        # Filter for K=10
        df_k10 = results_df[results_df["K"] == 10].copy()

        # Add Architecture Type
        df_k10["Architecture"] = df_k10["Model"].apply(
            lambda x: "CNN" if x in self.model_groups["CNN"] else "Transformer"
        )

        ax = sns.barplot(data=df_k10, x="Model", y="mAP", hue="Architecture")
        plt.title("Mean Average Precision (mAP@10) Comparison: CNN vs. Transformers")
        plt.ylabel("mAP")
        plt.xticks(rotation=15)

        # Add values on top
        for p in ax.patches:
            if p.get_height() > 0:
                ax.annotate(
                    format(p.get_height(), ".3f"),
                    (p.get_x() + p.get_width() / 2.0, p.get_height()),
                    ha="center",
                    va="center",
                    xytext=(0, 9),
                    textcoords="offset points",
                )

        plt.savefig(self.metrics_dir / "accuracy_comparison.png", bbox_inches="tight")
        plt.close()

    def plot_inference_performance(self, bench_df: pd.DataFrame):
        """Plots Inference Time vs Accuracy trade-off."""
        if bench_df.empty:
            logger.warning("Empty benchmark dataframe, skipping plot.")
            return

        plt.figure(figsize=(10, 7))
        
        # Check if we have mAP column (merged from accuracy results)
        y_col = "mAP" if "mAP" in bench_df.columns else "Avg Inference (ms)"
        
        sns.scatterplot(
            data=bench_df,
            x="Avg Inference (ms)",
            y=y_col,
            hue="Model",
            s=200,
            style="Type",
        )

        plt.title("Efficiency vs. Accuracy Trade-off")
        plt.xlabel("Average Inference Time (ms)")
        plt.ylabel("mAP@10" if y_col == "mAP" else "Inference Time")
        plt.grid(True)

        # Annotate points
        for i, row in bench_df.iterrows():
            plt.text(
                row["Avg Inference (ms)"] + 0.2, row[y_col], row["Model"], fontsize=9
            )

        plt.savefig(self.metrics_dir / "efficiency_tradeoff.png", bbox_inches="tight")
        plt.close()

    def generate_tsne(self, db: Session, model_name: str, n_samples: int = 500):
        """Generates t-SNE plot for a specific model embeddings."""
        logger.info(f"Generating t-SNE for {model_name}...")

        emb_col = get_embedding_column(model_name)

        # Query embeddings
        sql = text(
            f"SELECT {emb_col}, product_id FROM eshopdb.product_images WHERE {emb_col} IS NOT NULL LIMIT {n_samples}"
        )
        data = db.execute(sql).fetchall()

        if not data:
            logger.warning(f"No embeddings found for {model_name}, skipping t-SNE.")
            return

        embeddings, labels = [], []
        for row in data:
            vec = row[0]
            if vec is None:
                continue

            # Vector is already a list or vector-like object from pgvector
            if hasattr(vec, "tolist"):
                vec = vec.tolist()
            elif isinstance(vec, str):
                # Handle string format if necessary
                vec = [float(x) for x in vec.strip("[]").split(",")]

            if isinstance(vec, list) and len(vec) > 0:
                embeddings.append(vec)
                # Get category label
                cat_info = SearchService.get_categorization(db, row[1])
                labels.append(cat_info["article_type"])

        if not embeddings:
            return

        X = np.array(embeddings)

        # Handle small samples
        perplexity = min(30, len(X) - 1)
        if perplexity < 5: perplexity = 5
        
        tsne = TSNE(n_components=2, random_state=42, perplexity=perplexity)
        X_2d = tsne.fit_transform(X)

        plt.figure(figsize=(12, 8))
        df = pd.DataFrame(X_2d, columns=["x", "y"])
        df["Category"] = labels

        # Plot top 10 categories for clarity
        top_cats = df["Category"].value_counts().nlargest(10).index
        plot_df = df[df["Category"].isin(top_cats)]
        
        sns.scatterplot(
            data=plot_df,
            x="x",
            y="y",
            hue="Category",
            palette="tab10",
            alpha=0.7,
            s=60
        )

        plt.title(f"Feature Space Visualization (t-SNE): {model_name}")
        plt.legend(bbox_to_anchor=(1.05, 1), loc=2, title="Top Categories")
        plt.tight_layout()
        plt.savefig(self.metrics_dir / f"tsne_{model_name}.png", bbox_inches="tight")
        plt.close()


def benchmark_models(db: Session, sample_size=50):
    """Benchmarks inference speed for all champion models."""
    logger.info(f"Benchmarking inference performance (sample_size={sample_size})...")
    results = []
    
    images = (
        db.query(ProductImage)
        .filter(ProductImage.type == "Search")
        .limit(sample_size)
        .all()
    )

    if not images:
        logger.error("No search images found for benchmarking.")
        return pd.DataFrame()

    for m_name in settings.AVAILABLE_MODELS:
        logger.info(f"  ⚡ Testing {m_name}...")
        embedder = model_manager.get_embedder(m_name)
        if not embedder:
            continue

        # Warmup
        embedder.extract_features(images[0].url)

        times = []
        for img in images:
            t0 = time.time()
            embedder.extract_features(img.url)
            times.append((time.time() - t0) * 1000)

        results.append(
            {
                "Model": m_name,
                "Type": "Transformer" if "clip" in m_name or "dino" in m_name else "CNN",
                "Avg Inference (ms)": np.mean(times),
                "P95 Inference (ms)": np.percentile(times, 95),
                "Throughput (img/sec)": len(images) / (sum(times) / 1000) if sum(times) > 0 else 0,
            }
        )

    return pd.DataFrame(results)


if __name__ == "__main__":
    # Integration test for visualizer
    db_gen = get_db()
    db = next(db_gen)
    try:
        output_path = Path("results/visuals_test")
        viz = ThesisVisualizer(output_path)
        
        print("Running benchmarks...")
        bench_df = benchmark_models(db, sample_size=10)
        print(bench_df)
        viz.plot_inference_performance(bench_df)
        
        print("\nGenerating t-SNE for default model...")
        viz.generate_tsne(db, settings.DEFAULT_MODEL, n_samples=100)
        
        print(f"\nTest visuals generated in: {output_path.absolute()}")
    finally:
        db.close()