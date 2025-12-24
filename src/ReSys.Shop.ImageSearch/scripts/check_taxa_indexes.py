from sqlalchemy import create_engine, inspect
from app.config import settings

DATABASE_URL = settings.DATABASE_URL or "postgresql://postgres:12345678@localhost:5432/eshopdb"
engine = create_engine(DATABASE_URL)

def check_indexes():
    inspector = inspect(engine)
    for table in ['taxa', 'taxonomies', 'products']:
        indexes = inspector.get_indexes(table, schema='eshopdb')
        print(f"Indexes on eshopdb.{table}:")
        for idx in indexes:
            print(f" - Name: {idx['name']}, Columns: {idx['column_names']}, Unique: {idx['unique']}")

if __name__ == "__main__":
    check_indexes()
