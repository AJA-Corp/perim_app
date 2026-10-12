using System;
using CommunityToolkit.Maui.Extensions;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using perimapp.PopUp;
using perimapp.Views;

namespace perimapp.Components
{
    public partial class BottomNavBar : ContentView
    {
        public static readonly BindableProperty ActiveTabProperty =
            BindableProperty.Create(nameof(ActiveTab), typeof(string), typeof(BottomNavBar), string.Empty, propertyChanged: OnActiveTabChanged);

        public string ActiveTab
        {
            get => (string)GetValue(ActiveTabProperty);
            set => SetValue(ActiveTabProperty, value);
        }

        public BottomNavBar()
        {
            InitializeComponent();
            UpdateActiveTabUI();
        }

        private static void OnActiveTabChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is BottomNavBar bar)
            {
                bar.UpdateActiveTabUI();
            }
        }

        private void UpdateActiveTabUI()
        {
            if (HomeBtn == null || ProfileBtn == null || NotificationsBtn == null || SettingsBtn == null)
                return;

            if (string.IsNullOrWhiteSpace(ActiveTab))
            {
                HomeBtn.Opacity = 1.0;
                ProfileBtn.Opacity = 1.0;
                NotificationsBtn.Opacity = 1.0;
                SettingsBtn.Opacity = 1.0;
                return;
            }

            HomeBtn.Opacity = string.Equals(ActiveTab, "Home", StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.72;
            ProfileBtn.Opacity = string.Equals(ActiveTab, "Profile", StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.72;
            NotificationsBtn.Opacity = string.Equals(ActiveTab, "Notifications", StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.72;
            SettingsBtn.Opacity = string.Equals(ActiveTab, "Settings", StringComparison.OrdinalIgnoreCase) ? 1.0 : 0.72;
        }

        private async void OnHomeClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current?.CurrentPage is not MainView)
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainView)}", true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BottomNavBar] Erreur navigation Accueil: {ex.Message}");
            }
        }

        private async void OnProfileClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current?.CurrentPage is not ProfileView)
                {
                    await Shell.Current.GoToAsync(nameof(ProfileView), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BottomNavBar] Erreur navigation Profil: {ex.Message}");
            }
        }

        private async void OnAddProductClicked(object sender, EventArgs e)
        {
            try
            {
                var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.Camera>();
                }

                if (status == PermissionStatus.Granted)
                {
                    var scannerView = new ScannerView();

                    scannerView.OnBarcodeScanned = (scannedCode) =>
                    {
                        MainThread.BeginInvokeOnMainThread(async () =>
                        {
                            try
                            {
                                await Shell.Current.GoToAsync($"{nameof(AddProductView)}?barcode={scannedCode}");
                            }
                            catch (Exception navEx)
                            {
                                Console.WriteLine($"[BottomNavBar] Erreur navigation AddProductView : {navEx.Message}");
                            }
                        });
                    };

                    await Shell.Current.CurrentPage.Navigation.PushModalAsync(scannerView);
                }
                else
                {
                    var errorPopup = new InfosPopUp("Permission refusée", "La permission de la caméra est requise pour scanner un produit.", "OK");
                    await Shell.Current.CurrentPage.ShowPopupAsync(errorPopup);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BottomNavBar] Erreur lancement scanner: {ex.Message}");
            }
        }

        private async void OnNotificationsClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current?.CurrentPage is not NotificationsView)
                {
                    await Shell.Current.GoToAsync(nameof(NotificationsView), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BottomNavBar] Erreur navigation Notifications: {ex.Message}");
            }
        }

        private async void OnSettingsClicked(object sender, EventArgs e)
        {
            try
            {
                if (Shell.Current?.CurrentPage is not SettingsView)
                {
                    await Shell.Current.GoToAsync(nameof(SettingsView), true);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[BottomNavBar] Erreur navigation Paramètres: {ex.Message}");
            }
        }
    }
}
