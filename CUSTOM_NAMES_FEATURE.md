# Custom Product Names Feature

This feature allows users within the same home code (family group) to customize product names that are shared across all family members.

## How it works

1. **ModifyProductPage**: Users can edit the product name in the modification screen
2. **Custom Name Storage**: When a user changes a product name, it's saved as a "custom name" for their home code group
3. **AddProductPage**: When scanning barcodes, custom names are displayed instead of original product names
4. **Family Sharing**: All users with the same home code see the same custom names

## Technical Implementation

### Database Changes
A new table `custom_product_names` stores custom names:
- `barcode`: Product identifier
- `home_code`: Family group identifier  
- `custom_name`: User-defined product name
- `last_modified`: Timestamp of last change

### Code Changes
- **ProductInfos Model**: Added `CustomName`, `HomeCode`, and `DisplayName` properties
- **NeonProductService**: Added methods for custom name management
- **LocalProductService**: Added offline storage for custom names
- **UI Pages**: Modified to support name editing and display

### Access Control
- Custom names are scoped to home codes - only users in the same family group see the same custom names
- Original product names remain unchanged in the main database
- Users without home codes use original product names

## Setup Instructions

1. Run the SQL migration script: `database_migration.sql`
2. The feature works gracefully without the database table - it falls back to original names
3. Custom names are also stored locally for offline access

## Usage

1. Open a product in ModifyProductPage
2. Edit the product name field
3. Click "Enregistrer" to save
4. The custom name will appear for all family members when scanning the same barcode
5. Original product data remains unchanged for other users

## Fallback Behavior

- If the custom_product_names table doesn't exist, the feature degrades gracefully
- Users will see original product names if custom names aren't available
- Offline custom names are stored locally and sync when online