using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace perimapp.Services
{
    public class ImageCacheService
    {
        private static readonly HttpClient HttpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
        private static readonly ConcurrentDictionary<string, string> CachedPaths = new();
        private static readonly string CacheDirectory = Path.Combine(FileSystem.CacheDirectory, "images");

        static ImageCacheService()
        {
            // Ensure cache directory exists
            Directory.CreateDirectory(CacheDirectory);
        }

        public static async Task<string?> GetCachedImagePathAsync(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl))
                return null;

            // Check if already cached in memory
            if (CachedPaths.TryGetValue(imageUrl, out var cachedPath) && File.Exists(cachedPath))
            {
                return cachedPath;
            }

            try
            {
                // Generate cache file path
                var fileName = Path.GetFileName(new Uri(imageUrl).AbsolutePath);
                if (string.IsNullOrEmpty(fileName))
                {
                    fileName = imageUrl.GetHashCode().ToString() + ".jpg";
                }
                
                var localPath = Path.Combine(CacheDirectory, fileName);

                // Check if file already exists on disk
                if (File.Exists(localPath))
                {
                    CachedPaths[imageUrl] = localPath;
                    return localPath;
                }

                // Download and cache the image
                var imageData = await HttpClient.GetByteArrayAsync(imageUrl);
                await File.WriteAllBytesAsync(localPath, imageData);
                
                CachedPaths[imageUrl] = localPath;
                return localPath;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error caching image {imageUrl}: {ex.Message}");
                return imageUrl; // Fallback to original URL
            }
        }

        public static void ClearCache()
        {
            try
            {
                CachedPaths.Clear();
                
                if (Directory.Exists(CacheDirectory))
                {
                    Directory.Delete(CacheDirectory, true);
                    Directory.CreateDirectory(CacheDirectory);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error clearing image cache: {ex.Message}");
            }
        }

        public static long GetCacheSize()
        {
            try
            {
                if (!Directory.Exists(CacheDirectory))
                    return 0;

                var files = Directory.GetFiles(CacheDirectory);
                long totalSize = 0;
                
                foreach (var file in files)
                {
                    totalSize += new FileInfo(file).Length;
                }
                
                return totalSize;
            }
            catch
            {
                return 0;
            }
        }
    }
}