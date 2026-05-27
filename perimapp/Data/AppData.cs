using System.Collections.ObjectModel;
using perimapp.Models;

namespace perimapp.Data
{
    public static class AppData
    {
        public static int CurrentUserId { get; set; } = -1;

        public static ObservableCollection<ProductInfos> CurrentProducts { get; set; } = new();

        public static UserProfileDetails? CurrentUser { get; set; }

        public static bool NeedsAutoRefresh { get; set; } = true;

        public static void Clear()
        {
            CurrentUserId = -1;
            CurrentProducts.Clear();
            CurrentUser = null;
            NeedsAutoRefresh = true;
        }
    }
}