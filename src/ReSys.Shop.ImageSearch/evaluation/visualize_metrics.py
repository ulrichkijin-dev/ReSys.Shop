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

        self.model_groups = {
            "CNN": ["resnet50", "mobilenet_v3", "efficientnet_b0", "convnext_tiny"],
            "Transformer": ["clip_vit_b16", "fashion_clip", "dino_vit_s16"],
        }

    def plot_accuracy_comparison(self, results_df: pd.DataFrame):
        """Generates mAP comparison bar chart."""
        plt.figure()
        # Filter for K=10
        df_k10 = results_df[results_df["K"] == 10].copy()

        # Add Architecture Type
        df_k10["Type"] = df_k10["Model"].apply(
            lambda x: "CNN" if x in self.model_groups["CNN"] else "Transformer"
        )

        ax = sns.barplot(data=df_k10, x="Model", y="mAP", hue="Type")
        plt.title("Mean Average Precision (mAP@10) Comparison")
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
        plt.figure()
        # Ensure we have data
        if bench_df.empty:
            logger.warning("Empty benchmark dataframe, skipping plot.")
            return

        sns.scatterplot(
            data=bench_df,
            x="Avg Inference (ms)",
            y="mAP",
            hue="Model",
            s=200,
            style="Type",
        )

        plt.title("Efficiency vs. Accuracy Trade-off")
        plt.xlabel("Average Inference Time (ms)")
        plt.ylabel("mAP@10")
        plt.grid(True)

        # Annotate points
        for i, row in bench_df.iterrows():
            plt.text(
                row["Avg Inference (ms)"] + 0.5, row["mAP"], row["Model"], fontsize=9
            )

        plt.savefig(self.metrics_dir / "efficiency_tradeoff.png", bbox_inches="tight")
        plt.close()

    def generate_tsne(self, db: Session, model_name: str, n_samples: int = 500):
        """Generates t-SNE plot for a specific model."""
        logger.info(f"Generating t-SNE for {model_name}...")

        # Robust prefix mapping consistent with rest of app
        prefix_map = {
            "efficientnet_b0": "efficientnet",
            "convnext_tiny": "convnext",
            "clip_vit_b16": "clip",
            "fashion_clip": "fclip",
            "dino_vit_s16": "dino",
            "resnet50": "resnet",
            "mobilenet_v3": "mobilenet",
        }
        prefix = prefix_map.get(model_name, model_name.split("_")[0])
        emb_col = f"embedding_{prefix}"

        sql = text(
            f"SELECT {emb_col}, product_id FROM product_images WHERE {emb_col} IS NOT NULL LIMIT {n_samples}"
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

            # Handle SQLite/String storage
            if isinstance(vec, str):
                try:
                    # Remove numpy markers and brackets
                    clean = (
                        vec.replace("np.str_", "")
                        .replace('"', "")
                        .replace("'", "")
                        .strip()
                    )
                    if clean.startswith("["):
                        clean = clean[1:]
                    if clean.endswith("]"):
                        clean = clean[:-1]
                    vec = [float(x) for x in clean.split(",") if x.strip()]
                except Exception as e:
                    logger.debug(f"Failed to parse vector string: {e}")
                    continue

            # Handle numpy/list
            if hasattr(vec, "tolist"):
                vec = vec.tolist()

            if isinstance(vec, list) and len(vec) > 0:
                embeddings.append(vec)
                labels.append(
                    SearchService.get_categorization(db, row[1])["article_type"]
                )

        if not embeddings:
            return

        from sklearn.manifold import TSNE

        X = np.array(embeddings)

        # Handle small samples
        perplexity = min(30, len(X) - 1)
        if perplexity < 5:
            perplexity = 5
        if len(X) < 10:
            logger.warning(f"Too few samples ({len(X)}) for t-SNE of {model_name}")
            return

        tsne = TSNE(n_components=2, random_state=42, perplexity=perplexity)
        X_2d = tsne.fit_transform(X)

        plt.figure(figsize=(12, 8))
        df = pd.DataFrame(X_2d, columns=["x", "y"])
        df["Category"] = labels

        # Plot top 10 categories
        top_cats = df["Category"].value_counts().nlargest(10).index
        sns.scatterplot(
            data=df[df["Category"].isin(top_cats)],
            x="x",
            y="y",
            hue="Category",
            palette="tab10",
            alpha=0.6,
        )

        plt.title(f"Feature Space Visualization (t-SNE): {model_name}")
        plt.legend(bbox_to_anchor=(1.05, 1), loc=2)
        plt.tight_layout()
        plt.savefig(self.metrics_dir / f"tsne_{model_name}.png", bbox_inches="tight")
        plt.close()


def benchmark_models(db: Session, sample_size=50):
    """Benchmarks inference speed and resource usage."""
    results = []
    images = (
        db.query(ProductImage)
        .filter(ProductImage.type == "Search")
        .limit(sample_size)
        .all()
    )

    for m_name in settings.AVAILABLE_MODELS:
        embedder = model_manager.get_embedder(m_name)
        if not embedder:
            continue

        # Warmup
        if images:
            embedder.extract_features(images[0].url)

        start_time = time.time()
        times = []
        for img in images:
            t0 = time.time()
            embedder.extract_features(img.url)
            times.append((time.time() - t0) * 1000)

        results.append(
            {
                "Model": m_name,
                "Type": "Transformer"
                if "clip" in m_name or "dino" in m_name
                else "CNN",
                "Avg Inference (ms)": np.mean(times),
                "P95 Inference (ms)": np.percentile(times, 95),
                "Throughput (img/sec)": len(images) / (time.time() - start_time),
            }
        )

    return pd.DataFrame(results)


if __name__ == "__main__":
    # This is usually called from the experiment script
    pass

if __name__ == "__main__":
    db_gen = get_db()
    db = next(db_gen)
    try:
        benchmark_models(db)
        for model in COL_MAP.keys():
            visualize_embeddings(db, model)
            generate_confusion_matrix(db, model)
        print(f"\nAll diagrams generated in: {OUTPUT_DIR.absolute()}")
    finally:
        db.close()
