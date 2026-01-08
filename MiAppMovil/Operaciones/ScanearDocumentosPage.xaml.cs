using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;
using ImageSharpImage = SixLabors.ImageSharp.Image;
using SixLabors.ImageSharp.Processing;


namespace MiAppMovil.Operaciones;

public partial class ScanearDocumentosPage : ContentPage
{
    public ObservableCollection<ImageSource> ImagenesCapturadas { get; set; } = new();

    private List<string> rutasImagenes = new();

    public ScanearDocumentosPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }

    // Al tomar la foto
    private async void OnTomarFotoClicked(object sender, EventArgs e)
    {
        try
        {
            var foto = await MediaPicker.CapturePhotoAsync();
            if (foto == null) return;

            // Copiar o usar ruta temporal
            var rutaOriginal = await CopiarFotoACacheAsync(foto);

            // Navegar a página recorte y esperar resultado
            var paginaRecorte = new RecortarImagenPage(rutaOriginal);
            paginaRecorte.ImagenRecortada += (rutaRecortada) =>
            {
                // Aquí agregas la imagen ya recortada
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ImagenesCapturadas.Add(ImageSource.FromFile(rutaRecortada));
                    rutasImagenes.Add(rutaRecortada);
                });
            };

            await Navigation.PushAsync(paginaRecorte);
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo tomar o cargar la foto: {ex.Message}", "OK");
        }
    }

    private async Task<string> CopiarFotoACacheAsync(FileResult foto)
    {
        if (foto == null)
            throw new ArgumentNullException(nameof(foto));

        // Ruta donde copiar la foto (carpeta cache)
        string nuevaRuta = System.IO.Path.Combine(FileSystem.CacheDirectory, foto.FileName);

        using var streamOrigen = await foto.OpenReadAsync();
        using var streamDestino = System.IO.File.Create(nuevaRuta);

        await streamOrigen.CopyToAsync(streamDestino);

        return nuevaRuta;
    }


    private async void EliminarImagen_Clicked(object sender, EventArgs e)
    {
        if (sender is Button btn && btn.BindingContext is ImageSource imgSrc)
        {
            bool confirmar = await DisplayAlert("Confirmar", "¿Quieres eliminar esta imagen?", "Sí", "No");
            if (confirmar)
            {
                int index = ImagenesCapturadas.IndexOf(imgSrc);
                if (index >= 0)
                {
                    ImagenesCapturadas.RemoveAt(index);
                    rutasImagenes.RemoveAt(index);
                }
            }
        }
    }


    private async void OnGenerarPdfClicked(object sender, EventArgs e)
    {
        if (!rutasImagenes.Any())
        {
            await DisplayAlert("Advertencia", "Debes tomar al menos una foto para generar el PDF.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            string fileName = $"escaner_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            string filePath = string.Empty;

            await Task.Run(() =>
            {
                var documento = new PdfDocument();

                foreach (var path in rutasImagenes)
                {
                    using var streamOriginal = File.OpenRead(path);
                    var imageSharpImage = ImageSharpImage.Load(streamOriginal);

                    imageSharpImage.Mutate(x =>
                    {
                        x.Resize(new SixLabors.ImageSharp.Processing.ResizeOptions
                        {
                            Mode = SixLabors.ImageSharp.Processing.ResizeMode.Max,
                            Size = new SixLabors.ImageSharp.Size(1240, 0)
                        });
                        x.AutoOrient();
                        x.Grayscale();
                        x.BinaryThreshold(0.6f);
                    });


                    using var msProcesada = new MemoryStream();
                    imageSharpImage.SaveAsJpeg(msProcesada, new JpegEncoder { Quality = 90 });
                    msProcesada.Position = 0;

                    var imagen = XImage.FromStream(() => msProcesada);
                    var page = new PdfPage
                    {
                        Width = imagen.PointWidth,
                        Height = imagen.PointHeight
                    };
                    documento.AddPage(page);

                    using var gfx = XGraphics.FromPdfPage(page);
                    gfx.DrawImage(imagen, 0, 0, page.Width, page.Height);
                }

#if ANDROID
            var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
            filePath = Path.Combine(downloadsPath, fileName);
            using var ms = new MemoryStream();
            documento.Save(ms);
            File.WriteAllBytes(filePath, ms.ToArray());
#elif IOS
            var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            filePath = Path.Combine(documents, fileName);
            using var ms = new MemoryStream();
            documento.Save(ms);
            File.WriteAllBytes(filePath, ms.ToArray());
#endif
            });

#if IOS
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Share.RequestAsync(new ShareFileRequest
            {
                Title = "PDF generado",
                File = new ShareFile(filePath)
            });
        });

        await DisplayAlert("Éxito", $"PDF listo para compartir.\n\nNombre: {fileName}", "OK");
#elif ANDROID
        await DisplayAlert("Éxito", $"PDF guardado en Descargas:\n\n{fileName}", "OK");
#else
            await DisplayAlert("PDF generado", "Tu plataforma aún no tiene soporte de guardado implementado.", "OK");
#endif

            // Limpiar imágenes y rutas para empezar desde cero
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                ImagenesCapturadas.Clear();
                rutasImagenes.Clear();
            });
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar el PDF: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsRunning = false;
            LoadingIndicator.IsVisible = false;
        }
    }


}
