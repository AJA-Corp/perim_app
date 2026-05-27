using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using perimapp.Views;
using perimapp.Services;
using DotNet.Meteor.HotReload.Plugin;
using BarcodeScanning;

namespace perimapp;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseMauiCommunityToolkit()
            .UseMauiCommunityToolkitCore()
            .UseLocalNotification()
            .UseBarcodeScanning()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("InterBold.ttf", "InterBold");
                fonts.AddFont("InterExtraLight.ttf", "InterExtraLight");
                fonts.AddFont("InterLight.ttf", "InterLight");
                fonts.AddFont("InterMedium.ttf", "InterMedium");
                fonts.AddFont("InterThin.ttf", "InterThin");
            });

        // Enregistrement des services d'API
        builder.Services.AddSingleton<ApiProductService>();
        builder.Services.AddSingleton<ApiProfileService>();
        builder.Services.AddSingleton<AuthService>();
        builder.Services.AddSingleton<SyncService>();

        // Enregistrement du service local
        builder.Services.AddSingleton<LocalProductService>();
        builder.Services.AddSingleton<LocalUserService>();

        // Enregistrement des Views
        builder.Services.AddSingleton<MainView>();
        builder.Services.AddSingleton<ProfileView>();

        builder.Services.AddTransient<AddProductView>();
        builder.Services.AddTransient<DeletedProductView>();
        builder.Services.AddTransient<DetailsView>();
        builder.Services.AddTransient<ModifyProductView>();
        builder.Services.AddTransient<ScannerView>();
        builder.Services.AddTransient<LoadingView>();
        builder.Services.AddTransient<StartingView>();
        builder.Services.AddTransient<LogInView>();
        builder.Services.AddTransient<SignUpView>();

        // Enregistrement des ViewModels
        builder.Services.AddSingleton<perimapp.ViewModels.MainViewModel>();
        builder.Services.AddSingleton<perimapp.ViewModels.ProfileViewModel>();

        builder.Services.AddTransient<perimapp.ViewModels.AddProductViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.DeletedProductViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.DetailsViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.ModifyProductViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.ScannerViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.StartingViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.LoadingViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.LogInViewModel>();
        builder.Services.AddTransient<perimapp.ViewModels.SignUpViewModel>();

        // AddSingleton crée une seule instance pour toute la durée de vie de l'application
        // AddTransient crée une nouvelle instance à chaque fois que le service est demandé

#if DEBUG
        builder.Logging.AddDebug();
        builder.EnableHotReload();
#endif

        return builder.Build();
    }
}