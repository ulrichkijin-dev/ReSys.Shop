from sqlalchemy import create_engine, inspect
from app.config import settings

DATABASE_URL = settings.DATABASE_URL or "postgresql://postgres:12345678@localhost:5432/eshopdb"
engine = create_engine(DATABASE_URL)

def inspect_db():
    inspector = inspect(engine)
    tables = [
        'products', 'taxonomies', 'taxa', 'classification', 
        'variants', 'stock_items', 'product_images', 'stock_locations',
        'property_types', 'product_property_types', 'option_types', 'option_values', 'variant_option_values'
    ]
    
    for table in tables:
        print(f"\n--- Table: {table} ---")
        columns = inspector.get_columns(table, schema='eshopdb')
        for column in columns:
            nullable_str = "NULL" if column['nullable'] else "NOT NULL"
            default_str = f" DEFAULT {column['default']}" if column.get('default') is not None else ""
            print(f"  {column['name']}: {column['type']} {nullable_str}{default_str}")

if __name__ == "__main__":
    inspect_db()

