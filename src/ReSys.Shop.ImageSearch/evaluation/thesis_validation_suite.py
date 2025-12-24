"""
Comprehensive Thesis Validation Suite
=====================================

Tests and validates the Fashion Image Search system with:
- 3 Search Models: EfficientNet-B0, Fashion-CLIP, DINOv2
- 1 Recommendation Model: CLIP ViT-B/16

Validation includes:
1. Model Performance Metrics (P@K, R@K, mAP)
2. Cross-Model Comparison
3. Category-Level Analysis
4. Inference Speed Benchmarking
5. Statistical Significance Testing
"""

import sys
import logging
import time
import numpy as np
import pandas as pd
from pathlib import Path
from collections import defaultdict
from typing import Dict, List, Tuple
from scipy import stats
from sqlalchemy import func
from sqlalchemy.orm import Session

sys.path.append(str(Path(__file__).resolve().parent.parent))

from app.config import settings
from app.database import (
    get_db, Product, ProductImage, ProductClassification, Taxon
)
from app.model_factory import model_manager
from app.model_mapping import get_embedding_column
from app.services.search_service import SearchService

logging.basicConfig(
    level=logging.INFO,
    format='[%(asctime)s] %(levelname)s - %(message)s'
)
logger = logging.getLogger(__name__)

# THESIS CONFIGURATION
SEARCH_MODELS = [
    "efficientnet_b0",    # CNN Baseline
    "fashion_clip",        # Fashion-Specific
    "dinov2_vits14"        # Visual Structure
]

RECOMMENDATION_MODEL = "clip_vit_b16"  # General Semantic

ALL_TEST_MODELS = SEARCH_MODELS + [RECOMMENDATION_MODEL]

TEST_CONFIGS = {
    "sample_size": 100,        # Test queries
    "top_k_values": [5, 10, 20],
    "confidence_level": 0.95,
    "min_category_samples": 5
}


class ThesisValidator:
    """Main validation orchestrator for thesis experiments."""
    
    def __init__(self, output_dir: str = "results/thesis_validation"):
        self.output_dir = Path(output_dir)
        self.output_dir.mkdir(parents=True, exist_ok=True)
        self.db = next(get_db())
        
        # Warmup models
        logger.info("🔥 Pre-warming models...")
        model_manager.warmup(ALL_TEST_MODELS)
        
    def run_full_validation(self):
        """Execute complete validation suite."""
        logger.info("="*80)
        logger.info("THESIS VALIDATION SUITE - FASHION IMAGE SEARCH")
        logger.info("="*80)
        
        results = {}
        
        # 1. Search Feature Validation (3 models)
        logger.info("\n📊 PHASE 1: Search Feature Validation")
        results['search'] = self.validate_search_feature()
        
        # 2. Recommendation Feature Validation (1 model)
        logger.info("\n🎯 PHASE 2: Recommendation Feature Validation")
        results['recommendation'] = self.validate_recommendation_feature()
        
        # 3. Cross-Model Comparison
        logger.info("\n⚖️ PHASE 3: Statistical Comparison")
        results['comparison'] = self.compare_models_statistically(
            results['search']
        )
        
        # 4. Performance Benchmarking
        logger.info("\n⚡ PHASE 4: Inference Performance")
        results['performance'] = self.benchmark_inference_speed()
        
        # 5. Generate Reports
        logger.info("\n📝 PHASE 5: Generating Reports")
        self.generate_thesis_report(results)
        
        logger.info(f"\n✅ Validation complete! Results: {self.output_dir}")
        return results
    
    def validate_search_feature(self) -> Dict:
        """Validate 3 search models with retrieval metrics."""
        logger.info(f"Testing models: {SEARCH_MODELS}")
        
        # Get test queries
        queries = self._get_test_queries(TEST_CONFIGS['sample_size'])
        if not queries:
            logger.error("No test data available!")
            return {}
        
        results = defaultdict(lambda: defaultdict(list))
        category_results = defaultdict(lambda: defaultdict(lambda: defaultdict(list)))
        
        for model_name in SEARCH_MODELS:
            logger.info(f"  🔍 Evaluating {model_name}...")
            emb_col = get_embedding_column(model_name)
            
            for query_img in queries:
                vector = getattr(query_img, emb_col)
                if vector is None:
                    continue
                
                # Get ground truth category
                cat_info = SearchService.get_categorization(
                    self.db, query_img.product_id
                )
                gt_category = cat_info['article_type']
                
                # Retrieve similar items
                hits = SearchService.search_by_vector(
                    self.db, vector, model_name, 
                    limit=max(TEST_CONFIGS['top_k_values']),
                    exclude_image_id=query_img.id
                )
                
                # Calculate relevance
                relevance = [h.article_type == gt_category for h in hits]
                
                # Total relevant items in DB
                total_relevant = self._count_relevant_items(
                    gt_category, query_img.product_id
                )
                
                if total_relevant == 0:
                    continue
                
                # Metrics at different K values
                for k in TEST_CONFIGS['top_k_values']:
                    rel_k = relevance[:k]
                    
                    precision = sum(rel_k) / k
                    recall = sum(rel_k) / total_relevant
                    ap = self._calculate_ap(rel_k)
                    
                    results[model_name][f'P@{k}'].append(precision)
                    results[model_name][f'R@{k}'].append(recall)
                    results[model_name][f'AP@{k}'].append(ap)
                    
                    # Category-level tracking
                    if k == 10:  # Focus on K=10 for categories
                        category_results[model_name][gt_category]['P@10'].append(precision)
                        category_results[model_name][gt_category]['R@10'].append(recall)
                        category_results[model_name][gt_category]['AP@10'].append(ap)
        
        # Aggregate results
        aggregated = self._aggregate_metrics(results)
        category_agg = self._aggregate_category_metrics(category_results)
        
        # Save to CSV
        pd.DataFrame(aggregated).to_csv(
            self.output_dir / "search_metrics.csv", index=False
        )
        pd.DataFrame(category_agg).to_csv(
            self.output_dir / "search_category_breakdown.csv", index=False
        )
        
        return {
            'global_metrics': aggregated,
            'category_metrics': category_agg,
            'raw_results': dict(results)
        }
    
    def validate_recommendation_feature(self) -> Dict:
        """Validate recommendation model with diversity and relevance."""
        logger.info(f"Testing model: {RECOMMENDATION_MODEL}")
        
        queries = self._get_test_queries(50)
        if not queries:
            return {}
        
        emb_col = get_embedding_column(RECOMMENDATION_MODEL)
        metrics = {
            'same_category_ratio': [],
            'diversity_score': [],
            'avg_distance': []
        }
        
        for query_img in queries:
            vector = getattr(query_img, emb_col)
            if vector is None:
                continue
            
            cat_info = SearchService.get_categorization(
                self.db, query_img.product_id
            )
            source_category = cat_info['article_type']
            
            # Get recommendations
            recs = SearchService.search_by_vector(
                self.db, vector, RECOMMENDATION_MODEL,
                limit=10, exclude_product_id=query_img.product_id
            )
            
            if not recs:
                continue
            
            # Same category ratio (relevance)
            same_cat = sum(1 for r in recs if r.article_type == source_category)
            metrics['same_category_ratio'].append(same_cat / len(recs))
            
            # Diversity (unique categories)
            unique_cats = len(set(r.article_type for r in recs))
            metrics['diversity_score'].append(unique_cats / len(recs))
            
            # Average similarity distance
            avg_dist = np.mean([1 - r.score for r in recs])
            metrics['avg_distance'].append(avg_dist)
        
        summary = {
            'model': RECOMMENDATION_MODEL,
            'avg_same_category': np.mean(metrics['same_category_ratio']),
            'avg_diversity': np.mean(metrics['diversity_score']),
            'avg_distance': np.mean(metrics['avg_distance']),
            'n_queries': len(metrics['same_category_ratio'])
        }
        
        logger.info(f"  ✓ Relevance: {summary['avg_same_category']:.3f}")
        logger.info(f"  ✓ Diversity: {summary['avg_diversity']:.3f}")
        
        pd.DataFrame([summary]).to_csv(
            self.output_dir / "recommendation_metrics.csv", index=False
        )
        
        return summary
    
    def compare_models_statistically(self, search_results: Dict) -> Dict:
        """Perform statistical significance tests between models."""
        if not search_results or 'raw_results' not in search_results:
            return {}
        
        raw = search_results['raw_results']
        comparisons = []
        
        # Compare mAP@10 across search models
        metric = 'AP@10'
        
        for i, model1 in enumerate(SEARCH_MODELS):
            for model2 in SEARCH_MODELS[i+1:]:
                if metric not in raw[model1] or metric not in raw[model2]:
                    continue
                
                data1 = raw[model1][metric]
                data2 = raw[model2][metric]
                
                # Paired t-test
                t_stat, p_value = stats.ttest_rel(data1, data2)
                
                # Effect size (Cohen's d)
                diff = np.mean(data1) - np.mean(data2)
                pooled_std = np.sqrt(
                    (np.std(data1)**2 + np.std(data2)**2) / 2
                )
                cohens_d = diff / pooled_std if pooled_std > 0 else 0
                
                comparisons.append({
                    'Model_A': model1,
                    'Model_B': model2,
                    'Mean_A': np.mean(data1),
                    'Mean_B': np.mean(data2),
                    'Difference': diff,
                    't_statistic': t_stat,
                    'p_value': p_value,
                    'significant': p_value < 0.05,
                    'cohens_d': cohens_d
                })
        
        df = pd.DataFrame(comparisons)
        df.to_csv(self.output_dir / "statistical_comparison.csv", index=False)
        
        logger.info(f"  ✓ {len(comparisons)} pairwise comparisons completed")
        return {'comparisons': comparisons}
    
    def benchmark_inference_speed(self) -> Dict:
        """Measure inference performance for all models."""
        images = self.db.query(ProductImage).filter(
            ProductImage.type == 'Search'
        ).limit(100).all()
        
        if not images:
            return {}
        
        benchmarks = []
        
        for model_name in ALL_TEST_MODELS:
            logger.info(f"  ⚡ Benchmarking {model_name}...")
            embedder = model_manager.get_embedder(model_name)
            if not embedder:
                continue
            
            # Warmup
            embedder.extract_features(images[0].url)
            
            # Measure
            times = []
            for img in images:
                t0 = time.perf_counter()
                embedder.extract_features(img.url)
                times.append((time.perf_counter() - t0) * 1000)
            
            benchmarks.append({
                'Model': model_name,
                'Role': 'Search' if model_name in SEARCH_MODELS else 'Recommendation',
                'Avg_Inference_ms': np.mean(times),
                'Std_Inference_ms': np.std(times),
                'P95_Inference_ms': np.percentile(times, 95),
                'Throughput_img_sec': 1000 / np.mean(times)
            })
        
        df = pd.DataFrame(benchmarks)
        df.to_csv(self.output_dir / "performance_benchmarks.csv", index=False)
        
        return {'benchmarks': benchmarks}
    
    def generate_thesis_report(self, results: Dict):
        """Generate comprehensive markdown report for thesis."""
        report_path = self.output_dir / "THESIS_REPORT.md"
        
        with open(report_path, 'w', encoding='utf-8') as f:
            f.write("# Fashion Image Search - Thesis Validation Report\n\n")
            f.write(f"Generated: {pd.Timestamp.now()}\n\n")
            
            # Section 1: Search Models
            f.write("## 1. Search Feature Performance\n\n")
            f.write("### Selected Models\n")
            for model in SEARCH_MODELS:
                f.write(f"- **{model}**\n")
            
            if 'search' in results and 'global_metrics' in results['search']:
                f.write("\n### Global Metrics (mAP@10)\n\n")
                search_df = pd.DataFrame(results['search']['global_metrics'])
                search_df = search_df[search_df['K'] == 10].sort_values('mAP', ascending=False)
                f.write(search_df.to_markdown(index=False))
                f.write("\n\n")
            
            # Section 2: Recommendation Model
            f.write("## 2. Recommendation Feature Performance\n\n")
            f.write(f"### Selected Model: {RECOMMENDATION_MODEL}\n\n")
            
            if 'recommendation' in results:
                rec = results['recommendation']
                f.write(f"- **Relevance**: {rec.get('avg_same_category', 0):.3f}\n")
                f.write(f"- **Diversity**: {rec.get('avg_diversity', 0):.3f}\n")
                f.write(f"- **Avg Distance**: {rec.get('avg_distance', 0):.3f}\n\n")
            
            # Section 3: Statistical Comparison
            if 'comparison' in results and 'comparisons' in results['comparison']:
                f.write("## 3. Statistical Significance\n\n")
                comp_df = pd.DataFrame(results['comparison']['comparisons'])
                comp_df = comp_df[['Model_A', 'Model_B', 'Difference', 'p_value', 'significant']]
                f.write(comp_df.to_markdown(index=False))
                f.write("\n\n")
            
            # Section 4: Performance
            if 'performance' in results and 'benchmarks' in results['performance']:
                f.write("## 4. Inference Performance\n\n")
                perf_df = pd.DataFrame(results['performance']['benchmarks'])
                f.write(perf_df.to_markdown(index=False))
                f.write("\n\n")
            
            f.write("## 5. Recommendations\n\n")
            f.write("### For Production Deployment:\n")
            f.write("- **Search**: Use all 3 models in ensemble or select based on accuracy/speed trade-off\n")
            f.write(f"- **Recommendations**: Use {RECOMMENDATION_MODEL} for balanced semantic understanding\n\n")
        
        logger.info(f"  ✓ Report saved: {report_path}")
    
    # Helper methods
    def _get_test_queries(self, limit: int) -> List:
        """Get test split queries."""
        queries = self.db.query(ProductImage).join(Product).filter(
            Product.public_metadata['split'].astext == 'test',
            ProductImage.type == 'Search'
        ).order_by(func.random()).limit(limit).all()
        
        if not queries:
            logger.warning("No test split found, using random samples")
            queries = self.db.query(ProductImage).filter(
                ProductImage.type == 'Search'
            ).order_by(func.random()).limit(limit).all()
        
        return queries
    
    def _count_relevant_items(self, category: str, exclude_product_id) -> int:
        """Count relevant items in database."""
        return self.db.query(func.count(Product.id)).join(
            ProductClassification
        ).join(Taxon).filter(
            Taxon.name == category,
            Product.id != exclude_product_id
        ).scalar() or 0
    
    def _calculate_ap(self, relevance: List[bool]) -> float:
        """Calculate Average Precision."""
        if not any(relevance):
            return 0.0
        precisions = []
        rel_count = 0
        for i, rel in enumerate(relevance):
            if rel:
                rel_count += 1
                precisions.append(rel_count / (i + 1))
        return np.mean(precisions) if precisions else 0.0
    
    def _aggregate_metrics(self, results: Dict) -> List[Dict]:
        """Aggregate raw metrics to summary statistics."""
        aggregated = []
        for model, metrics in results.items():
            for metric_name, values in metrics.items():
                if '@' in metric_name:
                    metric_type, k = metric_name.split('@')
                    aggregated.append({
                        'Model': model,
                        'Metric': metric_type,
                        'K': int(k),
                        'Mean': np.mean(values),
                        'Std': np.std(values),
                        'Min': np.min(values),
                        'Max': np.max(values),
                        'N': len(values)
                    })
        
        # Add mAP column
        df = pd.DataFrame(aggregated)
        if not df.empty:
            map_df = df[df['Metric'] == 'AP'][['Model', 'K', 'Mean']].copy()
            map_df.columns = ['Model', 'K', 'mAP']
            return pd.merge(
                df, map_df, on=['Model', 'K'], how='left'
            ).to_dict('records')
        
        return aggregated
    
    def _aggregate_category_metrics(self, category_results: Dict) -> List[Dict]:
        """Aggregate category-level metrics."""
        aggregated = []
        for model, categories in category_results.items():
            for category, metrics in categories.items():
                if len(metrics['P@10']) >= TEST_CONFIGS['min_category_samples']:
                    aggregated.append({
                        'Model': model,
                        'Category': category,
                        'mP@10': np.mean(metrics['P@10']),
                        'mR@10': np.mean(metrics['R@10']),
                        'mAP@10': np.mean(metrics['AP@10']),
                        'N': len(metrics['P@10'])
                    })
        return aggregated
    
    def __del__(self):
        """Cleanup database connection."""
        if hasattr(self, 'db'):
            self.db.close()


if __name__ == "__main__":
    validator = ThesisValidator()
    results = validator.run_full_validation()