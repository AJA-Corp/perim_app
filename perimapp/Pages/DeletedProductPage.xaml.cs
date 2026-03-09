using perimapp.Models;
using perimapp.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Microsoft.Maui.Networking;

namespace perimapp.Pages
{
    public partial class DeletedProductPage : ContentPage
    {
        private readonly LocalProductService _localProductService;
        private readonly NeonProductService _neonProductService;
        private readonly NeonUserService _userService;
        public ObservableCollection<ProductInfos> Products { get; set; }

        public DeletedProductPage(NeonProductService neonProductService, LocalProductService localProductService, NeonUserService userService)
        {
            InitializeComponent();
            _neonProductService = neonProductService;
            _localProductService = localProductService;
            _userService = userService;
            Products = new ObservableCollection<ProductInfos>();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var products = await _localProductService.LoadProductsAsync();
            Products.Clear();

            foreach (var product in products.Where(p => p.State == "Deleted"))
            {
                Products.Add(product);
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlertAsync(
                "Confirmation",
                "Voulez-vous supprimer définitivement tous les produits de la corbeille ?",
                "Oui",
                "Non"
            );

            if (confirm)
            {
                // Suppression locale
                await _localProductService.DeleteAllDeletedProductsAsync();

                // Synchronisation avec le serveur si connecté
                bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                if (hasInternet)
                {
                    string? userIdString = await SecureStorage.GetAsync("user_id");
                    if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
                    {
                        await _neonProductService.EmptyTrashOnlineAsync(userId);
                    }
                }

                Products.Clear();
            }
        }

        private async void OnRestoreClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton imageButton && imageButton.BindingContext is ProductInfos product)
            {
                bool confirm = await DisplayAlertAsync(
                    "Confirmation",
                    $"Voulez-vous restaurer {product.DisplayName} ?",
                    "Oui",
                    "Non"
                );

                if (confirm)
                {
                    System.Diagnostics.Debug.WriteLine($"DeletedProductPage: Tentative de restauration du produit {product.DisplayName}");
                    System.Diagnostics.Debug.WriteLine($"DeletedProductPage: - DLC: {product.Dlc:yyyy-MM-dd}");
                    System.Diagnostics.Debug.WriteLine($"DeletedProductPage: - DeletedAt: {(product.DeletedAt.HasValue ? product.DeletedAt.Value.ToString("yyyy-MM-dd HH:mm:ss") : "NULL")}");

                    bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                    if (localSuccess)
                    {
                        if (product.DeletedAt.HasValue)
                        {
                            bool wasExpiredAtDeletion = product.Dlc.Date < product.DeletedAt.Value.Date;

                            System.Diagnostics.Debug.WriteLine($"DeletedProductPage: - Produit était périmé à la suppression: {wasExpiredAtDeletion}");

                            if (wasExpiredAtDeletion)
                            {
                                string? userIdStr = await SecureStorage.GetAsync("user_id");
                                if (!string.IsNullOrEmpty(userIdStr) && int.TryParse(userIdStr, out int userId))
                                {
                                    await _userService.DecrementLostProductCountAsync(userId);
                                    System.Diagnostics.Debug.WriteLine($"DeletedProductPage: Produit périmé restauré, compteur décrémenté pour user {userId}");
                                }
                                else
                                {
                                    System.Diagnostics.Debug.WriteLine("DeletedProductPage: Impossible de décrémenter le compteur, ID utilisateur introuvable.");
                                }
                            }
                            else
                            {
                                System.Diagnostics.Debug.WriteLine("DeletedProductPage: Produit restauré avant la date de péremption, aucun changement de compteur nécessaire.");
                            }
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine("DeletedProductPage: Date de suppression non disponible, impossible de déterminer si le produit était périmé ou non.");
                        }


                        bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                        if (hasInternet)
                        {
                            await _neonProductService.UpdateProductStateAsync(product.Id.ToString(), "Active");
                        }

                        Products.Remove(product);
                    }
                    else
                    {
                        await DisplayAlertAsync("Erreur", "Impossible de restaurer le produit.", "OK");
                    }
                }
            }
        }
    }
}
