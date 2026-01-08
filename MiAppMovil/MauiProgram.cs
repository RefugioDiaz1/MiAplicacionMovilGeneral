using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using SkiaSharp.Views.Maui.Controls.Hosting;

namespace MiAppMovil;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseSkiaSharp() // ✔ SkiaSharp para firmas, gráficos, etc.
            .UseMauiCommunityToolkit(); // ✔ Community Toolkit (si lo necesitas)

        return builder.Build();
    }
}
