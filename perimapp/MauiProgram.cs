using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using Microsoft.Extensions.Logging;

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
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("InterBold.ttf", "InterBold");
                fonts.AddFont("InterExtraLight.ttf", "InterExtraLight");
                fonts.AddFont("InterLight.ttf", "InterLight");
                fonts.AddFont("InterMedium.ttf", "InterMedium");
                fonts.AddFont("InterThin.ttf", "InterThin");
            });

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}