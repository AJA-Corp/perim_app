using Microsoft.Maui.Controls;
using System;

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
        }

        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            if (Navigation != null)
            {
                await Navigation.PopAsync();
            }
            else
            {
                Console.WriteLine("Navigation is null in SharedHeader.");
            }
        }

        private static void OnHeaderTextPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // Vous pouvez ajouter une logique ici si nécessaire lorsque le HeaderText change
        }
    }
}