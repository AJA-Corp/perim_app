## UI Changes for Custom Product Names

### Before Changes:
- MainPage displayed: `{Binding Name}` - showing original product names from database
- DetailsPage displayed: `{Binding ProductDetail.Name}` - showing original product names

### After Changes:
- MainPage now displays: `{Binding DisplayName}` - showing custom names when available, fallback to original names
- DetailsPage now displays: `{Binding ProductDetail.DisplayName}` - showing custom names when available, fallback to original names

### How DisplayName Property Works:
```csharp
public string DisplayName => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : Name;
```

### User Experience:
1. **When user has custom names**: Shows the family-specific custom name
2. **When no custom name exists**: Shows the original product name
3. **Seamless experience**: No UI changes needed for users without custom names

### Example Scenarios:

**Scenario 1: Product with Custom Name**
- Original Name: "Yogurt Strawberry 150g"
- Custom Name: "Yaourt Fraise"
- MainPage displays: "Yaourt Fraise"
- DetailsPage displays: "Yaourt Fraise"

**Scenario 2: Product without Custom Name**
- Original Name: "Milk 1L"
- Custom Name: null
- MainPage displays: "Milk 1L" 
- DetailsPage displays: "Milk 1L"

The UI automatically adapts to show the appropriate name without any additional logic needed in the UI layer.