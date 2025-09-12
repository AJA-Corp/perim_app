-- Database migration script for custom product names feature
-- This script creates the necessary table to store custom product names per home code

-- Create custom_product_names table
CREATE TABLE IF NOT EXISTS custom_product_names (
    id SERIAL PRIMARY KEY,
    barcode BIGINT NOT NULL,
    home_code INTEGER NOT NULL,
    custom_name VARCHAR(255) NOT NULL,
    last_modified TIMESTAMP WITH TIME ZONE DEFAULT CURRENT_TIMESTAMP,
    
    -- Constraints
    CONSTRAINT unique_barcode_home_code UNIQUE (barcode, home_code),
    CONSTRAINT fk_barcode FOREIGN KEY (barcode) REFERENCES products_data(barcode) ON DELETE CASCADE,
    CONSTRAINT fk_home_code FOREIGN KEY (home_code) REFERENCES users(home_code) ON DELETE CASCADE
);

-- Create indexes for better performance
CREATE INDEX IF NOT EXISTS idx_custom_product_names_barcode ON custom_product_names(barcode);
CREATE INDEX IF NOT EXISTS idx_custom_product_names_home_code ON custom_product_names(home_code);
CREATE INDEX IF NOT EXISTS idx_custom_product_names_barcode_home_code ON custom_product_names(barcode, home_code);

-- Add comments for documentation
COMMENT ON TABLE custom_product_names IS 'Stores custom product names defined by users within the same home code group';
COMMENT ON COLUMN custom_product_names.barcode IS 'Reference to the product barcode';
COMMENT ON COLUMN custom_product_names.home_code IS 'Home code identifying the family group';
COMMENT ON COLUMN custom_product_names.custom_name IS 'User-defined custom name for the product';
COMMENT ON COLUMN custom_product_names.last_modified IS 'Timestamp of the last modification';