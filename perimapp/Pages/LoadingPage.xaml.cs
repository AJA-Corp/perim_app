// Pages/LoadingPage.xaml.cs
using System;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using perimapp.Data;

namespace perimapp.Pages;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // 1. On récupère l'ID de l'utilisateur depuis le stockage sécurisé
        // SecureStorage.GetAsync retourne null si la clé n'existe pas
        string savedUserIdString = await SecureStorage.GetAsync("user_id");

        Console.WriteLine($"[DEBUG] ID utilisateur récupéré depuis SecureStorage : {savedUserIdString}");

        await Task.Delay(5000); // Délai d'initialisation

        // 2. On vérifie si un ID valide a été récupéré
        if (!string.IsNullOrEmpty(savedUserIdString))
        {
            // Pas besoin de stocker l'ID dans AppData.CurrentUserId.
            // Chaque fonction qui en a besoin le récupèrera via SecureStorage.
            await Shell.Current.GoToAsync(nameof(MainPage));
        }
        else
        {
            await Shell.Current.GoToAsync(nameof(StartingPage));
        }
    }
}
