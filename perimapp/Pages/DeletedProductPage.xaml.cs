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
        public ObservableCollection<ProductInfos> Products { get; set; }

        public DeletedProductPage(NeonProductService neonProductService, LocalProductService localProductService)
        {
            InitializeComponent();
            _neonProductService = neonProductService;
            _localProductService = localProductService;
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
                    // Mise à jour locale (State = "Active", DeletedAt = null)
                    bool localSuccess = await _localProductService.UpdateProductStateAsync(product.ProductUniqueId, "Active");

                    if (localSuccess)
                    {
                        // Synchronisation avec le serveur si connecté
                        bool hasInternet = Connectivity.Current.NetworkAccess == NetworkAccess.Internet;
                        if (hasInternet)
                        {
                            // Utilise product.Id (l'ID entier de la DB) pour la mise à jour serveur
                            await _neonProductService.UpdateProductStateAsync(product.Id.ToString(), "Active");
                        }

                        // Retire le produit de la liste affichée
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
