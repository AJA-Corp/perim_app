using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace perimapp.Pages;

public partial class DeletedProductPage : ContentPage
{
    public DeletedProductPage()
    {
        InitializeComponent();
    }
}

//j'ai commencer à faire regarde si ca peut t'aider 

/*
using perimapp.Models;
using perimapp.Services;
using Microsoft.Maui.Controls;

namespace perimapp.Pages
{
    public partial class DeletedProductPage : ContentPage
    {
        private readonly LocalProductService _localProductService;
        public ObservableCollection<ProductInfos> Products { get; set; }

        public DeletedProductPage()
        {
            InitializeComponent();
            _localProductService = new LocalProductService();
            Products = new ObservableCollection<ProductInfos>();
            BindingContext = this;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            var products = await _localProductService.LoadProductsAsync();
            Products.Clear();

            foreach (var product in products)
            {
                Products.Add(product);
            }
        }

        private async void OnDeleteClicked(object sender, EventArgs e)
        {
            if (sender is Button button && button.BindingContext is ProductInfos product)
            {
                bool confirm = await DisplayAlert(
                    "Confirmation",
                    $"Voulez-vous supprimer {product.Name} ?",
                    "Oui",
                    "Non"
                );

                if (confirm)
                {
                    // On passe directement la string ProductUniqueId
                    await _localProductService.RemoveProductAsync(product.ProductUniqueId);

                    // Mise à jour de la liste visible
                    Products.Remove(product);
                }
            }
        }
    }
}*/
