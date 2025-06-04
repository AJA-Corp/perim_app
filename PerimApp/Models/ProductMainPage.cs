// Models/ProductMainPage.cs
using System;

namespace perimapp.Models
{
    public class ProductMainPage
    {
        // Rend les propriétés 'string' nullables pour éviter l'avertissement CS8618
        public string? url_image { get; set; }
        public string? product_name { get; set; }
        public string? product_quantity { get; set; }
        public DateTime product_dlc { get; set; } 

        public int DaysRemaining => (int)(product_dlc.Date - DateTime.Today.Date).TotalDays;
        public string DaysRemainingText => $"{DaysRemaining}j"; 
    }
}