using System;

// Simplified test for DisplayName property logic
public class ProductInfosSimple
{
    public string Name { get; set; } = "";
    public string? CustomName { get; set; }
    
    public string DisplayName => !string.IsNullOrWhiteSpace(CustomName) ? CustomName : Name;
}

public class SimpleTests
{
    public static void Main()
    {
        Console.WriteLine("Running Custom Product Names Tests...");
        
        // Test 1: Custom name when available
        var product1 = new ProductInfosSimple
        {
            Name = "Original Product Name",
            CustomName = "Custom Family Name"
        };
        
        Console.WriteLine($"Test 1 - Expected: 'Custom Family Name', Actual: '{product1.DisplayName}', Pass: {product1.DisplayName == "Custom Family Name"}");
        
        // Test 2: Original name when no custom name
        var product2 = new ProductInfosSimple
        {
            Name = "Original Product Name",
            CustomName = null
        };
        
        Console.WriteLine($"Test 2 - Expected: 'Original Product Name', Actual: '{product2.DisplayName}', Pass: {product2.DisplayName == "Original Product Name"}");
        
        // Test 3: Original name when custom name is empty
        var product3 = new ProductInfosSimple
        {
            Name = "Original Product Name",
            CustomName = ""
        };
        
        Console.WriteLine($"Test 3 - Expected: 'Original Product Name', Actual: '{product3.DisplayName}', Pass: {product3.DisplayName == "Original Product Name"}");
        
        // Test 4: Original name when custom name is whitespace
        var product4 = new ProductInfosSimple
        {
            Name = "Original Product Name",
            CustomName = "   "
        };
        
        Console.WriteLine($"Test 4 - Expected: 'Original Product Name', Actual: '{product4.DisplayName}', Pass: {product4.DisplayName == "Original Product Name"}");
        
        Console.WriteLine("All tests completed!");
    }
}