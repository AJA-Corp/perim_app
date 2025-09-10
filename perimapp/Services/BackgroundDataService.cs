using System;
using System.Threading;
using System.Threading.Tasks;
using perimapp.Services;
using Microsoft.Maui.Storage;

namespace perimapp.Services
{
    public class BackgroundDataService
    {
        private static Timer? _refreshTimer;
        private static readonly object LockObject = new object();

        public static void StartPeriodicRefresh()
        {
            lock (LockObject)
            {
                if (_refreshTimer != null)
                    return; // Already started

                // Refresh cache every 5 minutes
                _refreshTimer = new Timer(
                    async _ => await RefreshCacheInBackground(),
                    null,
                    TimeSpan.FromMinutes(1), // Start after 1 minute
                    TimeSpan.FromMinutes(5)  // Repeat every 5 minutes
                );
            }
        }

        public static void StopPeriodicRefresh()
        {
            lock (LockObject)
            {
                _refreshTimer?.Dispose();
                _refreshTimer = null;
            }
        }

        private static async Task RefreshCacheInBackground()
        {
            try
            {
                // Only refresh if cache is about to expire or is expired
                if (ProductCacheService.IsCacheValid())
                    return;

                Console.WriteLine("[BackgroundDataService] Refreshing product cache in background");

                string userIdString = await SecureStorage.GetAsync("user_id");
                if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                    return;

                var productService = new NeonProductService();
                var products = await productService.GetUserProductsAsync(userId);
                
                ProductCacheService.SetCachedProducts(products);

                // Also update local storage
                var localService = new LocalProductService();
                await localService.SaveProductsAsync(products);

                Console.WriteLine($"[BackgroundDataService] Cache refreshed with {products.Count} products");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BackgroundDataService] Error refreshing cache: {ex.Message}");
            }
        }

        public static async Task PreloadDataAsync()
        {
            try
            {
                // Preload user products in background
                await Task.Run(async () =>
                {
                    string userIdString = await SecureStorage.GetAsync("user_id");
                    if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
                        return;

                    if (!ProductCacheService.IsCacheValid())
                    {
                        var productService = new NeonProductService();
                        var products = await productService.GetUserProductsAsync(userId);
                        ProductCacheService.SetCachedProducts(products);
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BackgroundDataService] Error preloading data: {ex.Message}");
            }
        }
    }
}