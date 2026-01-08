#if ANDROID
using Android.Content;
using Android.Net;
using Android.OS;
using Android.App;
using Java.IO;
using System.IO;
using System.Threading.Tasks;

namespace MiAppMovil.Helpers
{
    public static class MediaHelper
    {
        public static async Task<bool> GuardarImagenEnGaleriaAndroidAsync(Stream imagenStream, string nombreArchivo)
{
    try
    {
        var picturesPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryPictures).AbsolutePath;
        string filePath = System.IO.Path.Combine(picturesPath, nombreArchivo);

        using (var fileStream = new System.IO.FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
        {
            await imagenStream.CopyToAsync(fileStream);
        }

        var mediaScanIntent = new Android.Content.Intent(Android.Content.Intent.ActionMediaScannerScanFile);
        var contentUri = Android.Net.Uri.FromFile(new Java.IO.File(filePath));
        mediaScanIntent.SetData(contentUri);
        Android.App.Application.Context.SendBroadcast(mediaScanIntent);

        return true;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"Error guardando imagen: {ex.Message}");
        return false;
    }
}

    }
}
#endif

#if IOS
using UIKit;
using Foundation;
using System.IO;

namespace MiAppMovil.Helpers
{
    public static class MediaHelper
    {
        public static UIImage LoadImageFromStream(Stream stream)
        {
            var data = NSData.FromStream(stream);
            return UIImage.LoadFromData(data);
        }

        public static void GuardarImagenEnGaleriaiOS(UIImage imagen)
        {
            imagen.SaveToPhotosAlbum((image, error) =>
            {
                if (error != null)
                {
                    System.Diagnostics.Debug.WriteLine("Error guardando imagen en galería: " + error.LocalizedDescription);
                }
            });
        }
    }
}
#endif
