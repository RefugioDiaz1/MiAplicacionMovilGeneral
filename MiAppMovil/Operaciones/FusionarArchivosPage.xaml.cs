using PdfSharpCore.Drawing;
using PdfSharpCore.Pdf;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Collections.ObjectModel;

namespace MiAppMovil.Operaciones;

public partial class FusionarArchivosPage : ContentPage
{
    public class ArchivoAdjuntado
    {
        public string NombreArchivo { get; set; }
        public string RutaArchivo { get; set; }
        public string Extension => Path.GetExtension(RutaArchivo)?.ToLowerInvariant();
    }

    public ObservableCollection<ArchivoAdjuntado> Archivos { get; set; } = new ObservableCollection<ArchivoAdjuntado>();

    string pdfGeneradoPath = null;

    public FusionarArchivosPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async void OnGuardarPdfClicked(object sender, EventArgs e)
    {
        // Generar el PDF en bytes
        byte[] pdfBytes = GenerarPdfBytes();
        string fileName = $"fusionado_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

        // Guardar el PDF desde los bytes
        bool guardado = await GuardarPdfEnDescargasAsync(pdfBytes, fileName);

        if (guardado)
            await DisplayAlert("Listo", $"PDF guardado en la carpeta Descargas con nombre:\n{fileName}", "OK");
        else
            await DisplayAlert("Error", "No se pudo guardar el PDF.", "OK");
    }




    private async void OnAgregarArchivosClicked(object sender, EventArgs e)
    {
        bool permisosConcedidos = false;

#if ANDROID
    permisosConcedidos = await SolicitarPermisosAsync();
#elif IOS
    permisosConcedidos = await SolicitarPermisosIOSAsync();
#else
        permisosConcedidos = true;
#endif

        if (!permisosConcedidos)
        {
            await DisplayAlert("Permisos requeridos", "Debes otorgar permisos para seleccionar archivos.", "OK");
            return;
        }

        // Mostrar opciones
        var opcion = await DisplayActionSheet("Seleccionar desde:", "Cancelar", null, "Galería", "Archivos", "Cámara");

        if (opcion == "Galería")
            await SeleccionarDesdeGaleriaAsync();
        else if (opcion == "Archivos")
            await SeleccionarDesdeArchivosAsync();
        else if (opcion == "Cámara")
            await TomarFotoAsync();
    }

    private async Task SeleccionarDesdeArchivosAsync()
    {
        try
        {
            var files = await FilePicker.PickMultipleAsync(new PickOptions
            {
                PickerTitle = "Selecciona imágenes o PDFs",
                FileTypes = new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
            {
                { DevicePlatform.Android, new[] { "image/*", "application/pdf" } },
                { DevicePlatform.iOS, new[] { "public.image", "com.adobe.pdf" } },
                { DevicePlatform.WinUI, new[] { ".png", ".jpg", ".jpeg", ".pdf" } },
            })
            });

            if (files != null)
            {
                foreach (var file in files)
                {
                    var tempPath = Path.Combine(FileSystem.CacheDirectory, file.FileName);

                    using (var sourceStream = await file.OpenReadAsync())
                    using (var destStream = File.Create(tempPath))
                        await sourceStream.CopyToAsync(destStream);

                    Archivos.Add(new ArchivoAdjuntado
                    {
                        NombreArchivo = file.FileName,
                        RutaArchivo = tempPath
                    });
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar archivos: {ex.Message}", "OK");
        }
    }

    private async Task TomarFotoAsync()
    {
        try
        {
            var photo = await MediaPicker.CapturePhotoAsync(new MediaPickerOptions
            {
                Title = $"foto_{DateTime.Now:yyyyMMdd_HHmmss}.jpg"
            });

            if (photo != null)
            {
                var tempPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using (var sourceStream = await photo.OpenReadAsync())
                using (var destStream = File.Create(tempPath))
                    await sourceStream.CopyToAsync(destStream);

                Archivos.Add(new ArchivoAdjuntado
                {
                    NombreArchivo = photo.FileName,
                    RutaArchivo = tempPath
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo capturar la foto: {ex.Message}", "OK");
        }
    }


    private async Task SeleccionarDesdeGaleriaAsync()
    {
        try
        {
            var photo = await MediaPicker.PickPhotoAsync(new MediaPickerOptions
            {
                Title = "Selecciona una imagen"
            });

            if (photo != null)
            {
                var tempPath = Path.Combine(FileSystem.CacheDirectory, photo.FileName);

                using (var sourceStream = await photo.OpenReadAsync())
                using (var destStream = File.Create(tempPath))
                    await sourceStream.CopyToAsync(destStream);

                Archivos.Add(new ArchivoAdjuntado
                {
                    NombreArchivo = Path.GetFileName(tempPath),
                    RutaArchivo = tempPath
                });
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo cargar la imagen desde galería: {ex.Message}", "OK");
        }
    }




    private async void OnEliminarArchivoClicked(object sender, EventArgs e)
    {
        bool eliminar = await DisplayAlert("Eliminar foto", "¿Quieres eliminar esta foto?", "Sí", "No");
        if (eliminar)
        {
            if (sender is Button btn && btn.CommandParameter is ArchivoAdjuntado archivo)
            {
                Archivos.Remove(archivo);
            }
        }
        

        
    }

    async Task<bool> SolicitarPermisosAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.StorageRead>();
        }

        var writeStatus = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
        if (writeStatus != PermissionStatus.Granted)
        {
            writeStatus = await Permissions.RequestAsync<Permissions.StorageWrite>();
        }

        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraStatus != PermissionStatus.Granted)
        {
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return status == PermissionStatus.Granted &&
               writeStatus == PermissionStatus.Granted &&
               cameraStatus == PermissionStatus.Granted;
    }

    async Task<bool> SolicitarPermisosIOSAsync()
    {
        var status = await Permissions.CheckStatusAsync<Permissions.Photos>();
        if (status != PermissionStatus.Granted)
        {
            status = await Permissions.RequestAsync<Permissions.Photos>();
        }

        var cameraStatus = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (cameraStatus != PermissionStatus.Granted)
        {
            cameraStatus = await Permissions.RequestAsync<Permissions.Camera>();
        }

        return status == PermissionStatus.Granted && cameraStatus == PermissionStatus.Granted;
    }

    static bool fontResolverRegistrado = false;

    private byte[] GenerarPdfBytes()
    {
        var documento = new PdfDocument();

        foreach (var archivo in Archivos)
        {
            var ext = Path.GetExtension(archivo.RutaArchivo)?.ToLowerInvariant();

            if (ext == ".pdf")
            {
                using var fs = File.OpenRead(archivo.RutaArchivo);
                var pdf = PdfSharpCore.Pdf.IO.PdfReader.Open(fs, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Import);

                for (int i = 0; i < pdf.PageCount; i++)
                {
                    documento.AddPage(pdf.Pages[i]);
                }
            }
            else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
            {
                using var stream = File.OpenRead(archivo.RutaArchivo);
                var imagen = XImage.FromStream(() => stream);

                var page = documento.AddPage();
                page.Width = imagen.PointWidth;
                page.Height = imagen.PointHeight;

                using var gfx = XGraphics.FromPdfPage(page);
                gfx.DrawImage(imagen, 0, 0, page.Width, page.Height);
            }
            else
            {
                // Opción: Ignorar o lanzar excepción si hay otro tipo de archivo
            }
        }

        using var ms = new MemoryStream();
        documento.Save(ms);
        return ms.ToArray();
    }





    private async Task<bool> GuardarPdfEnDescargasAsync(string sourceFilePath, string fileName)
    {
#if ANDROID
    try
    {
        var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
            return false;

        var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        var filePath = Path.Combine(downloadsPath, fileName);

        File.Copy(sourceFilePath, filePath, true);
        return true;
    }
    catch
    {
        return false;
    }
#elif IOS
    try
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var destPath = Path.Combine(documents, fileName);
        File.Copy(sourceFilePath, destPath, true);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir PDF",
            File = new ShareFile(destPath)
        });

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al guardar PDF en iOS: " + ex.Message);
        return false;
    }
#else
        return false;
#endif
    }




    private async void OnFusionarGuardarClicked(object sender, EventArgs e)
    {
        if (!Archivos.Any())
        {
            await DisplayAlert("Error", "Debes adjuntar al menos un archivo antes de fusionar.", "OK");
            return;
        }

        LoadingIndicator.IsVisible = true;
        LoadingIndicator.IsRunning = true;

        try
        {
            // Genera el PDF fusionado a partir de los archivos
            string tempPdfPath = await GenerarPdfFusionadoAsync(Archivos);

            if (!File.Exists(tempPdfPath))
            {
                await DisplayAlert("Error", "No se pudo generar el archivo PDF.", "OK");
                return;
            }

            string fileName = $"fusionado_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";

            bool guardado = await GuardarPdfEnDescargasAsync(tempPdfPath, fileName);

            if (guardado)
            {
                await DisplayAlert("Listo", $"PDF guardado como:\n{fileName}", "OK");
                BtnLimpiar.IsVisible = true;
                PdfWebView.IsVisible = false;
                BtnGuardar.IsVisible = false;
            }
            else
            {
                await DisplayAlert("Error", "No se pudo guardar el PDF.", "OK");
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo generar o guardar el PDF: {ex.Message}", "OK");
        }
        finally
        {
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
        }
    }







    private async Task<bool> GuardarPdfEnDescargasAsync(byte[] pdfBytes, string fileName)
    {
#if ANDROID
    try
    {
        var status = await Permissions.RequestAsync<Permissions.StorageWrite>();
        if (status != PermissionStatus.Granted)
            return false;

        var downloadsPath = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryDownloads).AbsolutePath;
        var filePath = Path.Combine(downloadsPath, fileName);

        File.WriteAllBytes(filePath, pdfBytes);
        return true;
    }
    catch
    {
        return false;
    }
#elif IOS
    try
    {
        var documents = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
        var destPath = Path.Combine(documents, fileName);
        File.WriteAllBytes(destPath, pdfBytes);

        await Share.RequestAsync(new ShareFileRequest
        {
            Title = "Compartir PDF",
            File = new ShareFile(destPath)
        });

        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error al guardar PDF en iOS: " + ex.Message);
        return false;
    }
#else
        return false;
#endif
    }

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Navega hacia atrás en la pila
        await Navigation.PopAsync();
    }

    private async void OnLimpiarClicked(object sender, EventArgs e)
    {
        Archivos.Clear();
        PdfWebView.IsVisible = false;
        BtnLimpiar.IsVisible = false;
        pdfGeneradoPath = null;
    }

    private async Task<string> GenerarPdfFusionadoAsync(IEnumerable<ArchivoAdjuntado> archivos)
    {
        return await Task.Run(() =>
        {
            // Crear documento PDF
            var documento = new PdfDocument();

            foreach (var archivo in archivos)
            {
                var ext = archivo.Extension;

                if (ext == ".pdf")
                {
                    // Insertar páginas de PDF existentes
                    using (var fs = File.OpenRead(archivo.RutaArchivo))
                    {
                        var pdf = PdfSharpCore.Pdf.IO.PdfReader.Open(fs, PdfSharpCore.Pdf.IO.PdfDocumentOpenMode.Import);
                        for (int i = 0; i < pdf.PageCount; i++)
                        {
                            documento.AddPage(pdf.Pages[i]);
                        }
                    }
                }
                else if (ext == ".jpg" || ext == ".jpeg" || ext == ".png" || ext == ".bmp")
                {
                    // Insertar imagen como página PDF
                    using (var fileStream = File.OpenRead(archivo.RutaArchivo))
                    {
                        using var memoryStream = new MemoryStream();
                        fileStream.CopyTo(memoryStream);
                        memoryStream.Position = 0;

                        var original = SixLabors.ImageSharp.Image.Load(memoryStream.ToArray());

                        // Redimensionar a máximo 1000px de ancho o alto
                        int maxDimension = 1000;
                        int newWidth = original.Width;
                        int newHeight = original.Height;

                        if (original.Width > original.Height && original.Width > maxDimension)
                        {
                            newWidth = maxDimension;
                            newHeight = original.Height * maxDimension / original.Width;
                        }
                        else if (original.Height > maxDimension)
                        {
                            newHeight = maxDimension;
                            newWidth = original.Width * maxDimension / original.Height;
                        }

                        original.Mutate(x => x.Resize(newWidth, newHeight));

                        // Guardar la imagen redimensionada en nuevo stream
                        using var resizedStream = new MemoryStream();
                        original.SaveAsJpeg(resizedStream);
                        resizedStream.Position = 0;

                        var image = XImage.FromStream(() => new MemoryStream(resizedStream.ToArray()));


                        var page = new PdfPage
                        {
                            Width = image.PointWidth,
                            Height = image.PointHeight
                        };

                        documento.AddPage(page);

                        using var gfx = XGraphics.FromPdfPage(page);
                        gfx.DrawImage(image, 0, 0, page.Width, page.Height);
                    }

                }
            }

            // Guardar PDF en carpeta temporal
            string pdfPath = Path.Combine(FileSystem.CacheDirectory, $"fusionado_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
            documento.Save(pdfPath);

            return pdfPath;
        });
    }
}
