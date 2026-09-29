using System;
using System.Linq;
using Microsoft.Maui.Controls;
using perimapp.Models;
using perimapp.Services;
using perimapp.ViewModels;

namespace perimapp.Views
{
    public partial class MainView : ContentPage
    {
        private MainViewModel _viewModel;

        public MainView(LocalProductService localProductService, SyncService syncService)
        {
            InitializeComponent();
            _viewModel = new MainViewModel(localProductService, syncService);
            BindingContext = _viewModel;
            NavigationPage.SetHasNavigationBar(this, false);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            System.Diagnostics.Debug.WriteLine($"[MAINVIEW] OnAppearing - NeedsAutoRefresh: {perimapp.Data.AppData.NeedsAutoRefresh}");

            if (perimapp.Data.AppData.NeedsAutoRefresh)
            {
                System.Diagnostics.Debug.WriteLine("[MAINVIEW] Déclenchement du RefreshCommand");
                perimapp.Data.AppData.NeedsAutoRefresh = false;

                await _viewModel.RefreshCommand.ExecuteAsync(null);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("[MAINVIEW] Déclenchement du LoadProductsCommand");
                await _viewModel.LoadProductsCommand.ExecuteAsync(null);
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            System.Diagnostics.Debug.WriteLine("[MAINVIEW] OnDisappearing - Réactivation du NeedsAutoRefresh");
            // Réactiver le flag de refresh automatique quand on quitte la MainView
            // pour assurer un reload des données au retour
            perimapp.Data.AppData.NeedsAutoRefresh = true;
        }

        private void OnProductSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection != null && e.CurrentSelection.Any())
            {
                var selectedProduct = e.CurrentSelection.FirstOrDefault() as ProductInfos;

                if (selectedProduct != null)
                {
                    ((CollectionView)sender).SelectedItem = null;
                    _viewModel.ProductSelectedCommand.Execute(selectedProduct);
                }
            }
        }

        private double _lastScrollY = 0;
        private bool _isButtonVisible = true;

        private async void OnCollectionViewScrolled(object sender, ItemsViewScrolledEventArgs e)
        {
            double currentY = e.VerticalOffset;
            double delta = currentY - _lastScrollY;

            if (Math.Abs(delta) < 5) return;

            if (delta > 0 && _isButtonVisible)
            {
                _isButtonVisible = false;
                await FloatingBinButton.TranslateToAsync(0, 100, 250, Easing.CubicIn);
            }
            else if (delta < 0 && !_isButtonVisible)
            {
                _isButtonVisible = true;
                await FloatingBinButton.TranslateToAsync(0, 0, 250, Easing.CubicOut);
            }

            _lastScrollY = currentY;
        }
    }
}