"""
Thesis API Integration Tests
============================

Comprehensive API testing for:
- Search Feature: EfficientNet-B0, Fashion-CLIP, DINOv2
- Recommendation Feature: CLIP ViT-B/16

Tests validate:
1. Endpoint functionality
2. Response formats
3. Model availability
4. Performance requirements
5. Error handling
"""

import os
import sys
import time
import json
import requests
from pathlib import Path
from typing import Dict, List, Optional
import pandas as pd

# Configuration
API_BASE_URL = os.getenv("API_BASE_URL", "http://127.0.0.1:8000")
API_KEY = os.getenv("API_KEY", "thesis-secure-api-key-2025")
HEADERS = {"X-API-Key": API_KEY}

# Thesis Model Configuration
SEARCH_MODELS = ["efficientnet_b0", "fashion_clip", "dinov2_vits14"]
RECOMMENDATION_MODEL = "clip_vit_b16"
ALL_MODELS = SEARCH_MODELS + [RECOMMENDATION_MODEL]

# Test Requirements
REQUIREMENTS = {
    'max_inference_time_ms': 500,  # Max acceptable inference time
    'min_search_results': 5,        # Minimum results returned
    'min_search_score': 0.5         # Minimum similarity score
}


class ThesisAPITester:
    """Comprehensive API testing suite for thesis validation."""
    
    def __init__(self):
        self.results = []
        self.sample_data = None
        
    def run_all_tests(self):
        """Execute complete test suite."""
        print("="*70)
        print(" THESIS API INTEGRATION TESTS")
        print(" Fashion Image Search System")
        print("="*70)
        print(f"\nAPI URL: {API_BASE_URL}")
        print(f"Search Models: {SEARCH_MODELS}")
        print(f"Recommendation Model: {RECOMMENDATION_MODEL}\n")
        
        # Phase 1: Basic Connectivity
        print("📡 PHASE 1: Basic Connectivity")
        if not self.test_root():
            print("❌ CRITICAL: Cannot connect to API")
            return False
        
        if not self.test_health():
            print("❌ CRITICAL: Health check failed")
            return False
        
        # Phase 2: Model Availability
        print("\n🤖 PHASE 2: Model Availability")
        if not self.test_models_endpoint():
            print("❌ CRITICAL: Models not properly loaded")
            return False
        
        # Phase 3: Get Sample Data
        print("\n📊 PHASE 3: Sample Data Retrieval")
        if not self.get_sample_data():
            print("❌ CRITICAL: No sample data available")
            print("   Load data first: python -m app.dataset_loader --json data/styles.csv --images data/images --total 500")
            return False
        
        # Phase 4: Search Feature Tests
        print("\n🔍 PHASE 4: Search Feature Tests (3 Models)")
        search_passed = True
        for model in SEARCH_MODELS:
            if not self.test_search_by_id(model):
                search_passed = False
            if not self.test_search_by_upload(model):
                search_passed = False
        
        # Phase 5: Recommendation Feature Tests
        print("\n🎯 PHASE 5: Recommendation Feature Tests")
        rec_passed = self.test_recommendations(RECOMMENDATION_MODEL)
        
        # Phase 6: Model Comparison
        print("\n⚖️ PHASE 6: Cross-Model Comparison")
        comp_passed = self.test_model_comparison()
        
        # Phase 7: Performance Requirements
        print("\n⚡ PHASE 7: Performance Validation")
        perf_passed = self.test_performance_requirements()
        
        # Phase 8: Evaluation Metrics
        print("\n📈 PHASE 8: Retrieval Metrics")
        metrics_passed = self.test_evaluation_metrics()
        
        # Generate Summary
        self.print_summary()
        
        return all([search_passed, rec_passed, comp_passed, perf_passed, metrics_passed])
    
    def test_root(self) -> bool:
        """Test root endpoint."""
        print("  → Testing root endpoint...")
        try:
            response = requests.get(f"{API_BASE_URL}/", timeout=5)
            if response.status_code == 200:
                data = response.json()
                print(f"    ✓ Service: {data.get('service')}")
                print(f"    ✓ Version: {data.get('version')}")
                self._record_result('root', True)
                return True
            else:
                print(f"    ✗ Unexpected status: {response.status_code}")
                self._record_result('root', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result('root', False)
            return False
    
    def test_health(self) -> bool:
        """Test health check endpoint."""
        print("  → Testing health check...")
        try:
            response = requests.get(f"{API_BASE_URL}/health", timeout=5)
            if response.status_code == 200:
                data = response.json()
                print(f"    ✓ Database: {data['database']}")
                print(f"    ✓ Indexed images: {data['indexed_images']}")
                
                # Check model status
                loaded_models = [m['model_name'] for m in data['models'] if m['loaded']]
                missing = set(ALL_MODELS) - set(loaded_models)
                
                if missing:
                    print(f"    ⚠ Missing models: {missing}")
                else:
                    print(f"    ✓ All required models loaded")
                
                self._record_result('health', len(missing) == 0)
                return len(missing) == 0
            else:
                print(f"    ✗ Health check failed: {response.status_code}")
                self._record_result('health', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result('health', False)
            return False
    
    def test_models_endpoint(self) -> bool:
        """Test models list endpoint."""
        print("  → Testing models endpoint...")
        try:
            response = requests.get(f"{API_BASE_URL}/models", headers=HEADERS, timeout=5)
            if response.status_code == 200:
                data = response.json()
                models = data['models']
                
                all_present = True
                for model in ALL_MODELS:
                    if model not in models or not models[model]['loaded']:
                        print(f"    ✗ {model} not loaded")
                        all_present = False
                    else:
                        info = models[model]
                        print(f"    ✓ {model}: {info['architecture']} ({info['dimensions']}D)")
                
                self._record_result('models_list', all_present)
                return all_present
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result('models_list', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result('models_list', False)
            return False
    
    def get_sample_data(self) -> bool:
        """Retrieve sample data for testing."""
        print("  → Retrieving sample data...")
        try:
            sys.path.append(str(Path(__file__).resolve().parent.parent))
            from app.database import SessionLocal, Product, ProductImage
            
            db = SessionLocal()
            try:
                sample_image = db.query(ProductImage).filter(
                    ProductImage.type == 'Search',
                    ProductImage.embedding_efficientnet != None
                ).first()
                
                if not sample_image:
                    print("    ✗ No sample images with embeddings found")
                    return False
                
                self.sample_data = {
                    'product_id': str(sample_image.product_id),
                    'image_id': str(sample_image.id),
                    'image_path': sample_image.url
                }
                
                print(f"    ✓ Product ID: {self.sample_data['product_id'][:8]}...")
                print(f"    ✓ Image ID: {self.sample_data['image_id'][:8]}...")
                return True
            finally:
                db.close()
        except Exception as e:
            print(f"    ✗ Error: {e}")
            return False
    
    def test_search_by_id(self, model: str) -> bool:
        """Test search by image ID."""
        print(f"  → Testing search by ID ({model})...")
        if not self.sample_data:
            print("    ⚠ Skipping: No sample data")
            return False
        
        try:
            t0 = time.time()
            response = requests.get(
                f"{API_BASE_URL}/search/by-id/{self.sample_data['image_id']}",
                headers=HEADERS,
                params={'model': model, 'limit': 10},
                timeout=10
            )
            elapsed_ms = (time.time() - t0) * 1000
            
            if response.status_code == 200:
                data = response.json()
                results = data['results']
                
                # Validation checks
                checks = {
                    'results_count': len(results) >= REQUIREMENTS['min_search_results'],
                    'has_scores': all('score' in r for r in results),
                    'scores_valid': all(r['score'] >= REQUIREMENTS['min_search_score'] for r in results[:3]),
                    'performance': elapsed_ms < REQUIREMENTS['max_inference_time_ms']
                }
                
                passed = all(checks.values())
                
                print(f"    {'✓' if checks['results_count'] else '✗'} Results: {len(results)}")
                print(f"    {'✓' if checks['scores_valid'] else '✗'} Top score: {results[0]['score']:.4f}")
                print(f"    {'✓' if checks['performance'] else '✗'} Time: {elapsed_ms:.1f}ms")
                
                self._record_result(f'search_by_id_{model}', passed, {
                    'model': model,
                    'results': len(results),
                    'time_ms': elapsed_ms,
                    'top_score': results[0]['score']
                })
                return passed
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result(f'search_by_id_{model}', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result(f'search_by_id_{model}', False)
            return False
    
    def test_search_by_upload(self, model: str) -> bool:
        """Test search by uploading image."""
        print(f"  → Testing search by upload ({model})...")
        if not self.sample_data or not os.path.exists(self.sample_data['image_path']):
            print("    ⚠ Skipping: Image file not found")
            return False
        
        try:
            with open(self.sample_data['image_path'], 'rb') as f:
                files = {'file': (os.path.basename(self.sample_data['image_path']), f, 'image/jpeg')}
                
                t0 = time.time()
                response = requests.post(
                    f"{API_BASE_URL}/search/by-upload",
                    headers=HEADERS,
                    params={'model': model, 'limit': 10},
                    files=files,
                    timeout=15
                )
                elapsed_ms = (time.time() - t0) * 1000
            
            if response.status_code == 200:
                data = response.json()
                results = data['results']
                
                passed = len(results) >= REQUIREMENTS['min_search_results']
                print(f"    {'✓' if passed else '✗'} Results: {len(results)}, Time: {elapsed_ms:.1f}ms")
                
                self._record_result(f'search_by_upload_{model}', passed, {
                    'model': model,
                    'results': len(results),
                    'time_ms': elapsed_ms
                })
                return passed
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result(f'search_by_upload_{model}', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result(f'search_by_upload_{model}', False)
            return False
    
    def test_recommendations(self, model: str) -> bool:
        """Test product recommendations."""
        print(f"  → Testing recommendations ({model})...")
        if not self.sample_data:
            print("    ⚠ Skipping: No sample data")
            return False
        
        try:
            response = requests.get(
                f"{API_BASE_URL}/recommendations/by-product-id/{self.sample_data['product_id']}",
                headers=HEADERS,
                params={'model': model, 'limit': 10},
                timeout=10
            )
            
            if response.status_code == 200:
                data = response.json()
                results = data['results']
                
                # Check diversity (different products)
                unique_products = len(set(r['product_id'] for r in results))
                diversity_ok = unique_products >= len(results) * 0.8
                
                passed = len(results) >= REQUIREMENTS['min_search_results'] and diversity_ok
                
                print(f"    {'✓' if passed else '✗'} Results: {len(results)}, Unique: {unique_products}")
                
                self._record_result(f'recommendations_{model}', passed, {
                    'model': model,
                    'results': len(results),
                    'unique': unique_products
                })
                return passed
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result(f'recommendations_{model}', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result(f'recommendations_{model}', False)
            return False
    
    def test_model_comparison(self) -> bool:
        """Test model comparison endpoint."""
        print(f"  → Testing model comparison...")
        if not self.sample_data:
            print("    ⚠ Skipping: No sample data")
            return False
        
        try:
            response = requests.get(
                f"{API_BASE_URL}/diagnostics/compare-models/{self.sample_data['image_id']}",
                headers=HEADERS,
                params={'models': SEARCH_MODELS, 'limit': 5},
                timeout=15
            )
            
            if response.status_code == 200:
                data = response.json()
                results_by_model = data['results_by_model']
                
                all_models_ok = all(
                    model in results_by_model and 
                    results_by_model[model]['count'] > 0
                    for model in SEARCH_MODELS
                )
                
                if all_models_ok:
                    print(f"    ✓ All {len(SEARCH_MODELS)} models compared successfully")
                    for model in SEARCH_MODELS:
                        count = results_by_model[model]['count']
                        print(f"      - {model}: {count} results")
                else:
                    print(f"    ✗ Some models failed comparison")
                
                self._record_result('model_comparison', all_models_ok)
                return all_models_ok
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result('model_comparison', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result('model_comparison', False)
            return False
    
    def test_performance_requirements(self) -> bool:
        """Validate performance against thesis requirements."""
        print(f"  → Validating performance requirements...")
        
        # Extract performance data from previous tests
        search_tests = [r for r in self.results if 'search_by_id' in r['test'] and r.get('metadata')]
        
        if not search_tests:
            print("    ⚠ No performance data available")
            return False
        
        all_passed = True
        for test in search_tests:
            meta = test['metadata']
            time_ok = meta['time_ms'] < REQUIREMENTS['max_inference_time_ms']
            
            if not time_ok:
                print(f"    ✗ {meta['model']}: {meta['time_ms']:.1f}ms (exceeds {REQUIREMENTS['max_inference_time_ms']}ms)")
                all_passed = False
            else:
                print(f"    ✓ {meta['model']}: {meta['time_ms']:.1f}ms")
        
        self._record_result('performance_requirements', all_passed)
        return all_passed
    
    def test_evaluation_metrics(self) -> bool:
        """Test evaluation metrics endpoint."""
        print(f"  → Testing evaluation metrics endpoint...")
        
        try:
            # Test with one search model
            model = SEARCH_MODELS[0]
            response = requests.get(
                f"{API_BASE_URL}/evaluation/metrics",
                headers=HEADERS,
                params={'model': model, 'sample_size': 20},
                timeout=60
            )
            
            if response.status_code == 200:
                data = response.json()
                
                has_global = 'global_metrics' in data
                has_category = 'category_breakdown' in data
                
                if has_global and has_category:
                    print(f"    ✓ Metrics computed successfully")
                    print(f"      - Global metrics: {list(data['global_metrics'].keys())}")
                    print(f"      - Categories: {len(data['category_breakdown'])}")
                    
                    self._record_result('evaluation_metrics', True)
                    return True
                else:
                    print(f"    ✗ Incomplete metrics response")
                    self._record_result('evaluation_metrics', False)
                    return False
            else:
                print(f"    ✗ Failed: {response.status_code}")
                self._record_result('evaluation_metrics', False)
                return False
        except Exception as e:
            print(f"    ✗ Error: {e}")
            self._record_result('evaluation_metrics', False)
            return False
    
    def print_summary(self):
        """Print comprehensive test summary."""
        print("\n" + "="*70)
        print(" TEST SUMMARY")
        print("="*70)
        
        passed = sum(1 for r in self.results if r['passed'])
        total = len(self.results)
        
        print(f"\nTotal Tests: {total}")
        print(f"Passed: {passed}")
        print(f"Failed: {total - passed}")
        print(f"Success Rate: {passed/total*100:.1f}%\n")
        
        # Group by category
        categories = {}
        for r in self.results:
            test_name = r['test']
            if 'search' in test_name:
                cat = 'Search Feature'
            elif 'recommendation' in test_name:
                cat = 'Recommendation Feature'
            elif 'model_comparison' in test_name:
                cat = 'Model Comparison'
            elif 'performance' in test_name:
                cat = 'Performance'
            elif 'evaluation' in test_name:
                cat = 'Evaluation Metrics'
            else:
                cat = 'Basic Connectivity'
            
            if cat not in categories:
                categories[cat] = []
            categories[cat].append(r)
        
        for cat, tests in categories.items():
            cat_passed = sum(1 for t in tests if t['passed'])
            print(f"{cat}: {cat_passed}/{len(tests)}")
            for t in tests:
                status = "✓" if t['passed'] else "✗"
                print(f"  {status} {t['test']}")
        
        # Save detailed results
        output_dir = Path("results/api_tests")
        output_dir.mkdir(parents=True, exist_ok=True)
        
        df = pd.DataFrame(self.results)
        df.to_csv(output_dir / "api_test_results.csv", index=False)
        print(f"\nDetailed results saved to: {output_dir}/api_test_results.csv")
    
    def _record_result(self, test_name: str, passed: bool, metadata: Optional[Dict] = None):
        """Record test result."""
        self.results.append({
            'test': test_name,
            'passed': passed,
            'metadata': metadata or {}
        })


if __name__ == "__main__":
    tester = ThesisAPITester()
    success = tester.run_all_tests()
    sys.exit(0 if success else 1)