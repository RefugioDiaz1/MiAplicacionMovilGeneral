using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using System.IO;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using Image = Microsoft.Maui.Controls.Image;
using Rectangle = Microsoft.Maui.Graphics.Rect;

namespace MiAppMovil.Operaciones;

public partial class RecortarImagenPage : ContentPage
{
    private string rutaImagenOriginal;
    private double startX, startY;
    private BoxView currentHandle;

    public event Action<string> ImagenRecortada;

    public RecortarImagenPage(string ruta)
    {
        InitializeComponent();
        rutaImagenOriginal = ruta;
        ImagenRecortar.Source = ImageSource.FromFile(rutaImagenOriginal);
    }

    private void OnHandlePanUpdated(object sender, PanUpdatedEventArgs e)
    {
        if (sender is not BoxView handle)
            return;

        switch (e.StatusType)
        {
            case GestureStatus.Started:
                currentHandle = handle;
                startX = e.TotalX;
                startY = e.TotalY;
                break;

            case GestureStatus.Running:
                if (currentHandle == null)
                    return;

                var bounds = AbsoluteLayout.GetLayoutBounds(currentHandle);

                double newX = bounds.X + e.TotalX - startX;
                double newY = bounds.Y + e.TotalY - startY;

                newX = Math.Max(0, newX);
                newY = Math.Max(0, newY);

                var topLeft = AbsoluteLayout.GetLayoutBounds(HandleTopLeft);
                var topRight = AbsoluteLayout.GetLayoutBounds(HandleTopRight);
                var bottomLeft = AbsoluteLayout.GetLayoutBounds(HandleBottomLeft);
                var bottomRight = AbsoluteLayout.GetLayoutBounds(HandleBottomRight);

                if (currentHandle == HandleTopLeft)
                {
                    AbsoluteLayout.SetLayoutBounds(HandleTopLeft, new Rectangle(newX, newY, bounds.Width, bounds.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleTopRight, new Rectangle(topRight.X, newY, topRight.Width, topRight.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleBottomLeft, new Rectangle(newX, bottomLeft.Y, bottomLeft.Width, bottomLeft.Height));
                }
                else if (currentHandle == HandleTopRight)
                {
                    AbsoluteLayout.SetLayoutBounds(HandleTopRight, new Rectangle(newX, newY, bounds.Width, bounds.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleTopLeft, new Rectangle(topLeft.X, newY, topLeft.Width, topLeft.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleBottomRight, new Rectangle(newX, bottomRight.Y, bottomRight.Width, bottomRight.Height));
                }
                else if (currentHandle == HandleBottomLeft)
                {
                    AbsoluteLayout.SetLayoutBounds(HandleBottomLeft, new Rectangle(newX, newY, bounds.Width, bounds.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleTopLeft, new Rectangle(newX, topLeft.Y, topLeft.Width, topLeft.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleBottomRight, new Rectangle(bottomRight.X, newY, bottomRight.Width, bottomRight.Height));
                }
                else if (currentHandle == HandleBottomRight)
                {
                    AbsoluteLayout.SetLayoutBounds(HandleBottomRight, new Rectangle(newX, newY, bounds.Width, bounds.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleTopRight, new Rectangle(newX, topRight.Y, topRight.Width, topRight.Height));
                    AbsoluteLayout.SetLayoutBounds(HandleBottomLeft, new Rectangle(bottomLeft.X, newY, bottomLeft.Width, bottomLeft.Height));
                }

                // NO actualizar rectángulo aquí para evitar parpadeo

                startX = e.TotalX;
                startY = e.TotalY;

                break;

            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                // Actualiza rectángulo solo al final del movimiento
                ActualizarRectangulo();
                currentHandle = null;
                break;
        }
    }

    private async void OnRecortarClicked(object sender, EventArgs e)
    {
        if (sender is Button btn)
            btn.IsEnabled = false;

        IndicadorCarga.IsVisible = true;
        IndicadorCarga.IsRunning = true;

        try
        {
            using var img = SixLabors.ImageSharp.Image.Load(rutaImagenOriginal);

            // Opcional: redimensionar si imagen muy grande para acelerar
            int maxDimension = 2000;
            if (img.Width > maxDimension || img.Height > maxDimension)
            {
                double scale = Math.Min((double)maxDimension / img.Width, (double)maxDimension / img.Height);
                int newWidth = (int)(img.Width * scale);
                int newHeight = (int)(img.Height * scale);
                img.Mutate(i => i.Resize(newWidth, newHeight));
            }

            // Espera a que ImagenRecortar esté renderizada para tomar su tamaño real
            double imageViewWidth = ImagenRecortar.Width;
            double imageViewHeight = ImagenRecortar.Height;

            // Si aún no tiene tamaño, espera un poco (mejor hacerlo en OnAppearing)
            if (imageViewWidth <= 0 || imageViewHeight <= 0)
                await Task.Delay(100);

            imageViewWidth = ImagenRecortar.Width;
            imageViewHeight = ImagenRecortar.Height;

            var rect = AbsoluteLayout.GetLayoutBounds(RectanguloRecorte);

            // Tamaño real de imagen y control
            int imgWidth = img.Width;
            int imgHeight = img.Height;

            double scaleX = imageViewWidth / imgWidth;
            double scaleY = imageViewHeight / imgHeight;
            double displayScale = Math.Min(scaleX, scaleY);


            // Tamaño de la imagen renderizada en el Image
            double renderedWidth = imgWidth * displayScale;
            double renderedHeight = imgHeight * displayScale;

            double offsetX = (imageViewWidth - renderedWidth) / 2;
            double offsetY = (imageViewHeight - renderedHeight) / 2;

            double rectX = rect.X - offsetX;
            double rectY = rect.Y - offsetY;

            rectX = Math.Max(0, rectX);
            rectY = Math.Max(0, rectY);

            int x = (int)(rectX / displayScale);
            int y = (int)(rectY / displayScale);
            int w = (int)(rect.Width / displayScale);
            int h = (int)(rect.Height / displayScale);


            // Validar límites
            x = Math.Clamp(x, 0, imgWidth - 1);
            y = Math.Clamp(y, 0, imgHeight - 1);
            if (x + w > imgWidth) w = imgWidth - x;
            if (y + h > imgHeight) h = imgHeight - y;


            // Asegurar que el rectángulo no se salga de la imagen
            x = Math.Clamp(x, 0, img.Width - 1);
            y = Math.Clamp(y, 0, img.Height - 1);
            if (x + w > img.Width) w = img.Width - x;
            if (y + h > img.Height) h = img.Height - y;

            img.Mutate(i => i.Crop(new SixLabors.ImageSharp.Rectangle(x, y, w, h)));

            string nuevaRuta = System.IO.Path.Combine(FileSystem.CacheDirectory, $"recortada_{System.IO.Path.GetFileName(rutaImagenOriginal)}");
            await img.SaveAsJpegAsync(nuevaRuta);

            ImagenRecortada?.Invoke(nuevaRuta);
            await Navigation.PopAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudo recortar: {ex.Message}", "OK");
        }
        finally
        {
            if (sender is Button btn2)
                btn2.IsEnabled = true;

            IndicadorCarga.IsRunning = false;
            IndicadorCarga.IsVisible = false;
        }
    }



    private void ActualizarRectangulo()
    {
        var topLeft = AbsoluteLayout.GetLayoutBounds(HandleTopLeft);
        var bottomRight = AbsoluteLayout.GetLayoutBounds(HandleBottomRight);

        double x = Math.Min(topLeft.X, bottomRight.X);
        double y = Math.Min(topLeft.Y, bottomRight.Y);
        double width = Math.Abs(bottomRight.X - topLeft.X);
        double height = Math.Abs(bottomRight.Y - topLeft.Y);

        AbsoluteLayout.SetLayoutBounds(RectanguloRecorte, new Rectangle(x, y, width, height));
    }

 
}
