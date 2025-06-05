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

        public SharedHeader()
        {
            InitializeComponent ();
            BindingContext = this;
        }

        // Fonction back button
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

                // Case ModifiyProductPage
                if (currentPage is ModifyProductPage)
                {
                    await Shell.Current.GoToAsync(nameof(DetailsPage), true);
                    Console.WriteLine("SharedHeader [SUCCÈS] : Navigation de ModifyProductPage vers DetailsPage.");
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
                // Cela devrait maintenant être beaucoup moins fréquent avec la correction ci-dessus.
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