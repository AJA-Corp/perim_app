using Microsoft.Maui.Controls;
using System;
using perimapp.Views;

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
            BindingContext = this;
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

                if (currentPage is ModifyProductView)
                {
                    if (!string.IsNullOrEmpty(ProductUniqueId))
                    {
                        string route = $"{nameof(DetailsView)}?ProductUniqueId={ProductUniqueId}";
                        await Shell.Current.GoToAsync(route, true);
                        Console.WriteLine($"SharedHeader [SUCCÈS] : Navigation de ModifyProductView vers DetailsView avec ID: {ProductUniqueId}.");
                    }
                    else
                    {
                        await Shell.Current.GoToAsync($"///{nameof(MainView)}", true);
                        Console.WriteLine("SharedHeader [INFO] : ProductUniqueId non trouvé, navigation vers MainView.");
                    }
                }
                else
                {
                    await Shell.Current.GoToAsync($"///{nameof(MainView)}", true);
                    Console.WriteLine("SharedHeader [SUCCÈS] : Navigation vers MainView.");
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

        }
    }
}