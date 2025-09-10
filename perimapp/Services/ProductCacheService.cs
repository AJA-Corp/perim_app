using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using perimapp.Models;

namespace perimapp.Services
{
    public class ProductCacheService
    {
        private static List<ProductInfos>? _cachedProducts;
        private static DateTime _lastCacheTime;
        private static readonly TimeSpan CacheExpiry = TimeSpan.FromMinutes(5); // Cache for 5 minutes

        public static bool IsCacheValid()
        {
            return _cachedProducts != null && 
                   DateTime.Now - _lastCacheTime < CacheExpiry;
        }

        public static List<ProductInfos>? GetCachedProducts()
        {
            return IsCacheValid() ? _cachedProducts : null;
        }

        public static void SetCachedProducts(List<ProductInfos> products)
        {
            _cachedProducts = products;
            _lastCacheTime = DateTime.Now;
        }

        public static void InvalidateCache()
        {
            _cachedProducts = null;
        }

        public static void UpdateProductInCache(ProductInfos updatedProduct)
        {
            if (_cachedProducts == null) return;

            var index = _cachedProducts.FindIndex(p => p.ProductUniqueId == updatedProduct.ProductUniqueId);
            if (index >= 0)
            {
                _cachedProducts[index] = updatedProduct;
            }
        }

        public static void AddProductToCache(ProductInfos newProduct)
        {
            if (_cachedProducts == null) return;
            
            _cachedProducts.Add(newProduct);
        }

        public static void RemoveProductFromCache(string productUniqueId)
        {
            if (_cachedProducts == null) return;
            
            _cachedProducts.RemoveAll(p => p.ProductUniqueId == productUniqueId);
        }
    }
}