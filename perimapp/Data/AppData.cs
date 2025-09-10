using System.Collections.ObjectModel;
using perimapp.Models;

namespace perimapp.Data
{
    public static class AppData
    {
        // Id de l'utilisateur connecté
        public static int CurrentUserId { get; set; } = -1;

        // Liste observable des produits (pour binding UI)
        public static ObservableCollection<ProductInfos> CurrentProducts { get; set; } = new();

        // Optionnel : utilisateur courant complet
        // public static UserProfile CurrentUserProfile { get; set; }
        public static UserProfileDetails? CurrentUser { get; set; }
    }
}
