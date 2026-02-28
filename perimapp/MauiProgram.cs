using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;
using Plugin.LocalNotification;
using perimapp.Services;
using DotNet.Meteor.HotReload.Plugin;

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

        builder.Services.AddSingleton<NeonProductService>();

#if DEBUG
        builder.Logging.AddDebug();
        builder.EnableHotReload();
#endif

        return builder.Build();
    }
}