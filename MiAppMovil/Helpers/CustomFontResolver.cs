using PdfSharpCore.Fonts;
using System.Reflection;

public class CustomFontResolver : IFontResolver
{
    public string DefaultFontName => "Roboto"; // ✔ Requerido por la interfaz

    public byte[] GetFont(string faceName)
    {
        if (faceName == "Roboto")
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resource = "MiAppMovil.Resources.Fonts.Roboto-Regular.ttf"; // Asegúrate de que este sea correcto

            using var stream = assembly.GetManifestResourceStream(resource)
                ?? throw new InvalidOperationException($"No se encontró el recurso embebido: {resource}");

            using var ms = new MemoryStream();
            stream.CopyTo(ms);
            return ms.ToArray();
        }

        throw new InvalidOperationException("Fuente no soportada: " + faceName);
    }

    public FontResolverInfo ResolveTypeface(string familyName, bool isBold, bool isItalic)
    {
        if (familyName == "Roboto")
        {
            if (isBold && isItalic)
                return new FontResolverInfo("Roboto-BoldItalic");
            if (isBold)
                return new FontResolverInfo("Roboto-Bold");
            if (isItalic)
                return new FontResolverInfo("Roboto-Italic");
            return new FontResolverInfo("Roboto");
        }

        return PlatformFontResolver.ResolveTypeface(familyName, isBold, isItalic);
    }

}
