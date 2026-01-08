using SkiaSharp;
using SkiaSharp.Views.Maui;
using SkiaSharp.Views.Maui.Controls;
using System.Collections.ObjectModel;
using System.Collections.ObjectModel;
using Microsoft.Maui.Storage;
using CommunityToolkit.Maui.Media;
using MiAppMovil.Helpers; // importa el namespace donde esté la clase

namespace MiAppMovil.Operaciones
{
    public partial class EntregaRefaccionesPage : ContentPage
    {
        private SKPath signaturePath = new SKPath();
        private ObservableCollection<ImageSource> Imagenes = new ObservableCollection<ImageSource>();
        private bool estaDibujando = false;

        public EntregaRefaccionesPage()
        {
            InitializeComponent();
            VendedorPicker.ItemsSource = new[] { "Juan", "Mario", "Carlos" };
            FotosCollection.ItemsSource = Imagenes;
        }

        private void OnPaintSurface(object sender, SKPaintSurfaceEventArgs e)
        {
            var canvas = e.Surface.Canvas;
            canvas.Clear(SKColors.White);

            using var paint = new SKPaint
            {
                Style = SKPaintStyle.Stroke,
                Color = SKColors.Black,
                StrokeWidth = 4,
                IsAntialias = true
            };

            canvas.DrawPath(signaturePath, paint);
        }


        private void OnCanvasViewTouch(object sender, SKTouchEventArgs e)
        {
            switch (e.ActionType)
            {
                case SKTouchAction.Pressed:
                    estaDibujando = true;
                    signaturePath.MoveTo(e.Location);
                    e.Handled = true;
                    break;

                case SKTouchAction.Moved:
                    if (estaDibujando)
                    {
                        signaturePath.LineTo(e.Location);
                        e.Handled = true;
                    }
                    break;

                case SKTouchAction.Released:
                case SKTouchAction.Cancelled:
                    estaDibujando = false;
                    e.Handled = true;
                    break;
            }

       ((SKCanvasView)sender).InvalidateSurface();
        }



        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            // Navega hacia atrás en la pila
            await Navigation.PopAsync();
        }


        private void OnLimpiarFirma(object sender, EventArgs e)
        {
            signaturePath.Reset();
            SignaturePad.InvalidateSurface();
        }

        // Aquí puedes usar FirmaCanvas
        void OnClearSignature(object sender, EventArgs e)
        {
            signaturePath.Reset();            // Limpia la ruta guardada
            SignaturePad.InvalidateSurface(); // Refresca el canvas para que se vea limpio
        }

        private async void OnAgregarFotos(object sender, EventArgs e)
        {
            string action = await DisplayActionSheet("Agregar foto", "Cancelar", null, "Desde galería", "Tomar foto");

            if (action == "Desde galería")
            {
                try
                {
                    var photo = await FilePicker.PickAsync(new PickOptions
                    {
                        PickerTitle = "Selecciona una imagen",
                        FileTypes = FilePickerFileType.Images
                    });

                    if (photo != null)
                    {
                        var originalStream = await photo.OpenReadAsync();

                        using var memoryStream = new MemoryStream();
                        await originalStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        bool guardado = false;

#if ANDROID
                guardado = await MediaHelper.GuardarImagenEnGaleriaAndroidAsync(new MemoryStream(memoryStream.ToArray()), photo.FileName);
#elif IOS
                var imagen = MediaHelper.LoadImageFromStream(new MemoryStream(memoryStream.ToArray()));
                MediaHelper.GuardarImagenEnGaleriaiOS(imagen);
                guardado = true;
#else
                        guardado = false;
#endif

                        if (guardado)
                        {
                            Imagenes.Add(ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray())));
                        }
                        else
                        {
                            await DisplayAlert("Error", "No se pudo guardar la imagen en la galería.", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo seleccionar la imagen: {ex.Message}", "OK");
                }
            }
            else if (action == "Tomar foto")
            {
                try
                {
                    var photo = await MediaPicker.CapturePhotoAsync();

                    if (photo != null)
                    {
                        var originalStream = await photo.OpenReadAsync();

                        using var memoryStream = new MemoryStream();
                        await originalStream.CopyToAsync(memoryStream);
                        memoryStream.Position = 0;

                        bool guardado = false;

#if ANDROID
                guardado = await MediaHelper.GuardarImagenEnGaleriaAndroidAsync(new MemoryStream(memoryStream.ToArray()), $"camara_{DateTime.Now.Ticks}.jpg");
#elif IOS
                var imagen = MediaHelper.LoadImageFromStream(new MemoryStream(memoryStream.ToArray()));
                MediaHelper.GuardarImagenEnGaleriaiOS(imagen);
                guardado = true;
#else
                        guardado = false;
#endif

                        if (guardado)
                        {
                            Imagenes.Add(ImageSource.FromStream(() => new MemoryStream(memoryStream.ToArray())));
                        }
                        else
                        {
                            await DisplayAlert("Error", "No se pudo guardar la imagen en la galería.", "OK");
                        }
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo tomar la foto: {ex.Message}", "OK");
                }
            }
        }




        private async Task<string> GuardarImagenEnDiscoAsync(Stream imagenStream, string nombreArchivo)
        {
            // Ruta del archivo en el almacenamiento local
            string ruta = Path.Combine(FileSystem.AppDataDirectory, nombreArchivo);

            using var fileStream = File.Create(ruta);
            await imagenStream.CopyToAsync(fileStream);

            return ruta; // Devuelve la ruta completa
        }


        private async void OnImagenSeleccionada(object sender, SelectionChangedEventArgs e)
        {
            if (e.CurrentSelection.FirstOrDefault() is ImageSource selectedImage)
            {
                bool eliminar = await DisplayAlert("Eliminar foto", "¿Quieres eliminar esta foto?", "Sí", "No");
                if (eliminar)
                {
                    Imagenes.Remove(selectedImage);
                }
                else
                {
                    // Mostrar foto en grande (opcional)
                    await Navigation.PushModalAsync(new ContentPage
                    {
                        BackgroundColor = Colors.Black,
                        Content = new Grid
                        {
                            Children =
        {
            new Image
            {
                Source = selectedImage,
                Aspect = Aspect.AspectFit,
                VerticalOptions = LayoutOptions.CenterAndExpand,
                HorizontalOptions = LayoutOptions.Center
            },
            new Button
            {
                Text = "Cerrar",
                BackgroundColor = Color.FromArgb("#FF3B30"),
                TextColor = Colors.White,
                CornerRadius = 12,
                WidthRequest = 80,
                HeightRequest = 40,
                HorizontalOptions = LayoutOptions.End,
                VerticalOptions = LayoutOptions.Start,
                Margin = new Thickness(20),
                Command = new Command(async () => await Navigation.PopModalAsync())
            }
        }
                        }
                    });

                }
                FotosCollection.SelectedItem = null; // Deseleccionar
            }
        }

        private async void OnImagenTocada(object sender, EventArgs e)
        {
            if (sender is Image img && img.Source is ImageSource imagen)
            {
                var zoomImage = new Image
                {
                    Source = imagen,
                    Aspect = Aspect.AspectFit,
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                };

                var container = new Grid
                {
                    BackgroundColor = Colors.Black,
                    Children = { zoomImage }
                };

                double startScale = 1;
                double currentScale = 1;
                double xOffset = 0;
                double yOffset = 0;

                var pinchGesture = new PinchGestureRecognizer();
                pinchGesture.PinchUpdated += (s, args) =>
                {
                    if (args.Status == GestureStatus.Started)
                    {
                        startScale = currentScale;
                    }
                    else if (args.Status == GestureStatus.Running)
                    {
                        // Escalado centrado en el punto de origen
                        currentScale = Math.Max(1, startScale * args.Scale);
                        zoomImage.Scale = currentScale;
                    }
                    else if (args.Status == GestureStatus.Completed)
                    {
                        // Guardar valores actuales
                        xOffset = zoomImage.TranslationX;
                        yOffset = zoomImage.TranslationY;
                    }
                };

                var panGesture = new PanGestureRecognizer();
                panGesture.PanUpdated += (s, args) =>
                {
                    if (zoomImage.Scale > 1)
                    {
                        switch (args.StatusType)
                        {
                            case GestureStatus.Running:
                                zoomImage.TranslationX = xOffset + args.TotalX;
                                zoomImage.TranslationY = yOffset + args.TotalY;
                                break;

                            case GestureStatus.Completed:
                                xOffset = zoomImage.TranslationX;
                                yOffset = zoomImage.TranslationY;
                                break;
                        }
                    }
                };

                var doubleTap = new TapGestureRecognizer { NumberOfTapsRequired = 2 };
                doubleTap.Tapped += (s, args) =>
                {
                    zoomImage.Scale = 1;
                    zoomImage.TranslationX = 0;
                    zoomImage.TranslationY = 0;
                    currentScale = 1;
                    xOffset = 0;
                    yOffset = 0;
                };

                zoomImage.GestureRecognizers.Add(pinchGesture);
                zoomImage.GestureRecognizers.Add(panGesture);
                zoomImage.GestureRecognizers.Add(doubleTap);

                // Botón para cerrar
                var cerrarButton = new Button
                {
                    Text = "Cerrar",
                    BackgroundColor = Color.FromArgb("#FF3B30"),
                    TextColor = Colors.White,
                    CornerRadius = 12,
                    WidthRequest = 80,
                    HeightRequest = 40,
                    HorizontalOptions = LayoutOptions.End,
                    VerticalOptions = LayoutOptions.Start,
                    Margin = new Thickness(20),
                    Command = new Command(async () => await Navigation.PopModalAsync())
                };

                container.Children.Add(cerrarButton);

                // Mostrar modal
                await Navigation.PushModalAsync(new ContentPage
                {
                    Content = container,
                    BackgroundColor = Colors.Black
                });
            }
        }


        private async void OnEliminarFoto(object sender, EventArgs e)
        {
            if (sender is Button btn && btn.CommandParameter is ImageSource imagen)
            {
                bool confirm = await DisplayAlert("Eliminar foto", "¿Quieres eliminar esta foto?", "Sí", "No");
                if (confirm)
                    Imagenes.Remove(imagen);
            }
        }



        private async void OnFinalizarEntrega(object sender, EventArgs e)
        {
            try
            {
                // Validar campos requeridos
                string credito = CreditoEntry.Text;
                string cliente = ClienteEntry.Text;
                string vendedor = VendedorPicker.SelectedItem?.ToString();

                if (string.IsNullOrWhiteSpace(credito) ||
                    string.IsNullOrWhiteSpace(cliente) ||
                    string.IsNullOrWhiteSpace(vendedor))
                {
                    await DisplayAlert("Error", "Completa todos los campos antes de finalizar.", "OK");
                    return;
                }

                if (Imagenes == null || Imagenes.Count == 0)
                {
                    await DisplayAlert("Error", "Por favor, adjunta al menos una foto antes de finalizar.", "OK");
                    return;
                }

                // Validar firma
                if (signaturePath.IsEmpty)
                {
                    await DisplayAlert("Error", "Por favor, firma el documento antes de finalizar.", "OK");
                    return;
                }

                // Validar conexión a Internet
                if (Connectivity.NetworkAccess != NetworkAccess.Internet)
                {
                    await DisplayAlert("Sin conexión", "No tienes conexión a internet. Revisa tu red WiFi o datos móviles.", "OK");
                    return;
                }

                // Obtener ubicación actual
                Location location = null;
                try
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    }

                    if (status == PermissionStatus.Granted)
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
                        location = await Geolocation.Default.GetLocationAsync(request);
                    }
                    else
                    {
                        await DisplayAlert("Aviso", "Permiso de ubicación no concedido. Se continuará sin guardar la ubicación.", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Aviso", $"No se pudo obtener la ubicación: {ex.Message}", "OK");
                }

                // Convertir firma a imagen base64
                var firmaStream = await ObtenerStreamDeFirmaAsync();
                byte[] firmaBytes;
                using (var ms = new MemoryStream())
                {
                    await firmaStream.CopyToAsync(ms);
                    firmaBytes = ms.ToArray();
                }

                string firmaBase64 = Convert.ToBase64String(firmaBytes);

                // Convertir imágenes a base64
                List<string> imagenesBase64 = new();

                foreach (var img in Imagenes)
                {
                    if (img is FileImageSource fileImage)
                    {
                        // Imagen cargada desde archivo local
                        byte[] bytes = File.ReadAllBytes(fileImage.File);
                        imagenesBase64.Add(Convert.ToBase64String(bytes));
                    }
                    else if (img is StreamImageSource streamImage)
                    {
                        // Imagen cargada desde stream
                        using var stream = await streamImage.Stream(CancellationToken.None);
                        using var ms = new MemoryStream();
                        await stream.CopyToAsync(ms);
                        byte[] bytes = ms.ToArray();
                        imagenesBase64.Add(Convert.ToBase64String(bytes));
                    }
                    else
                    {
                        // Otros tipos ImageSource que puedas manejar si quieres
                    }
                }


                // Preparar payload
                var datosEntrega = new
                {
                    NumeroCredito = credito,
                    Cliente = cliente,
                    Vendedor = vendedor,
                    FirmaBase64 = firmaBase64,
                    Imagenes = imagenesBase64,
                    Ubicacion = location != null ? new { location.Latitude, location.Longitude } : null,
                    Fecha = DateTime.UtcNow
                };

                // TODO: Aquí iría el envío a la API
                // await EnviarDatosAApiAsync(datosEntrega);

                await DisplayAlert("Éxito", "Entrega finalizada correctamente.", "OK");

                // Limpiar campos
                CreditoEntry.Text = string.Empty;
                ClienteEntry.Text = string.Empty;
                VendedorPicker.SelectedItem = null;
                signaturePath.Reset();
                SignaturePad.InvalidateSurface();
                Imagenes.Clear();
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Ocurrió un error al finalizar la entrega:\n{ex.Message}", "OK");
            }
        }

        private async Task<Stream> ObtenerStreamDeFirmaAsync()
        {
            var width = (int)SignaturePad.CanvasSize.Width;
            var height = (int)SignaturePad.CanvasSize.Height;

            if (width <= 0 || height <= 0)
                return null;

            using var surface = SKSurface.Create(new SKImageInfo(width, height));
            var canvas = surface.Canvas;
            canvas.Clear(SKColors.White);

            using (var paint = new SKPaint
            {
                Color = SKColors.Black,
                StrokeWidth = 3,
                IsAntialias = true,
                Style = SKPaintStyle.Stroke
            })
            {
                canvas.DrawPath(signaturePath, paint);
            }

            using var image = surface.Snapshot();
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            // Convertir a stream (NO usar using aquí para no cerrar el stream)
            var memoryStream = new MemoryStream();
            data.SaveTo(memoryStream);
            memoryStream.Position = 0; // Reiniciar posición
            return memoryStream;
        }



    }
}
