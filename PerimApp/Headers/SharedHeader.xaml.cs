// SharedHeader.xaml.cs
using Microsoft.Maui.Controls;
using System;
using perimapp.Pages; // Indispensable pour référencer vos pages

namespace perimapp.Headers
{
    public partial class SharedHeader : ContentView
    {
        public static readonly BindableProperty HeaderTextProperty =
            BindableProperty.Create(nameof(HeaderText), typeof(string), typeof(SharedHeader), string.Empty, BindingMode.OneWay, null, OnHeaderTextPropertyChanged);

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        // NOUVELLE PROPRIÉTÉ BINDABLE POUR L'ID DU PRODUIT
        public static readonly BindableProperty ProductUniqueIdProperty =
            BindableProperty.Create(nameof(ProductUniqueId), typeof(string), typeof(SharedHeader), string.Empty);

        public string ProductUniqueId
        {
            get => (string)GetValue(ProductUniqueIdProperty);
            set => SetValue(ProductUniqueIdProperty, value);
        }

        public SharedHeader()
        {
            InitializeComponent ();
            BindingContext = this; // Le BindingContext de ContentView doit être lui-même pour les BindableProperties internes.
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            if (Shell.Current == null)
            {
                Console.WriteLine("SharedHeader [ERREUR] : Shell.Current est null. Impossible de naviguer.");
                return;
            }

            try
            {
                var currentPage = Shell.Current.CurrentPage;

                // Case ModifyProductPage
                if (currentPage is ModifyProductPage)
                {
                    // Vérifiez si ProductUniqueId est défini avant de naviguer
                    if (!string.IsNullOrEmpty(ProductUniqueId))
                    {
                        // Construisez l'itinéraire de retour avec l'ID du produit
                        string route = $"{nameof(DetailsPage)}?ProductUniqueId={ProductUniqueId}";
                        await Shell.Current.GoToAsync(route, true); // Le 'true' signifie animation
                        Console.WriteLine($"SharedHeader [SUCCÈS] : Navigation de ModifyProductPage vers DetailsPage avec ID: {ProductUniqueId}.");
                    }
                    else
                    {
                        // Si l'ID n'est pas disponible, retournez à la page principale ou affichez une erreur
                        await Shell.Current.GoToAsync(nameof(MainPage), true);
                        Console.WriteLine("SharedHeader [INFO] : ProductUniqueId non trouvé, navigation vers MainPage.");
                    }
                }
                // Majority Case to MainPage
                else
                {
                    await Shell.Current.GoToAsync(nameof(MainPage), true);
                    Console.WriteLine("SharedHeader [SUCCÈS] : Navigation vers MainPage.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SharedHeader [ERREUR] : Erreur critique lors de la navigation : {ex.Message}");
                Console.WriteLine($"SharedHeader [DÉTAILS] : Type d'exception : {ex.GetType().Name}");
                Console.WriteLine($"SharedHeader [DÉTAILS] : Pile d'appels : {ex.StackTrace}");
            }
        }

        private static void OnHeaderTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Votre logique si nécessaire
        }
    }
}