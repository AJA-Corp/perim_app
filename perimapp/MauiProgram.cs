using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using perimapp.Pages; // Ajouté pour les pages
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

        // --- ENREGISTREMENT DES SERVICES ---
        builder.Services.AddSingleton<NeonProductService>();
        builder.Services.AddSingleton<NeonUserService>();
        // Enregistrement du service local
        builder.Services.AddSingleton<LocalProductService>();
        builder.Services.AddSingleton<LocalUserService>();

        // --- ENREGISTREMENT DES PAGES ---
        builder.Services.AddSingleton<MainPage>(); 
        builder.Services.AddTransient<DeletedProductPage>();
        builder.Services.AddTransient<DetailsPage>();
        builder.Services.AddTransient<ScannerPage>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.EnableHotReload();
#endif

        return builder.Build();
    }
}