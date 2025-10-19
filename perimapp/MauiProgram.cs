using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using perimapp.Pages; // Ajouté pour les pages
using perimapp.Services;

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
        // Enregistrement du service local
        builder.Services.AddSingleton<LocalProductService>();

        // --- ENREGISTREMENT DES PAGES ---
        // Enregistrement de MainPage et DeletedProductPage pour la DI
        builder.Services.AddSingleton<MainPage>(); 
        builder.Services.AddTransient<DeletedProductPage>();
        builder.Services.AddTransient<DetailsPage>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}