// SharedHeader.xaml.cs
using Microsoft.Maui.Controls;
using System;

namespace perimapp.Headers
{
    public partial class SharedHeader : ContentView // Toujours ContentView !
    {
        public static readonly BindableProperty HeaderTextProperty =
            BindableProperty.Create(nameof(HeaderText), typeof(string), typeof(SharedHeader), string.Empty, BindingMode.OneWay, null, OnHeaderTextPropertyChanged);

        public string HeaderText
        {
            get => (string)GetValue(HeaderTextProperty);
            set => SetValue(HeaderTextProperty, value);
        }

        // SUPPRIMEZ CETTE LIGNE SI ELLE EST PRÉSENTE :
        // public event EventHandler BackButtonClicked; 

        public SharedHeader()
        {
            InitializeComponent ();
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // C'est la ligne clé pour la navigation avec Shell
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync("..");
                Console.WriteLine("WORKING");
            }
            else
            {
                // Ce cas est très peu probable si votre application utilise Shell
                Console.WriteLine("Shell.Current is null. Cannot go back.");
            }
        }

        private static void OnHeaderTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Votre logique si nécessaire
        }
    }
}