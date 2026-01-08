namespace MiAppMovil.Pages;

using MiAppMovil.Helpers;
using Microsoft.Maui.Storage;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;

public partial class ConfiguracionPage : ContentPage
{
    private Action? _onCerrarAlerta;

    public ConfiguracionPage()
	{
		InitializeComponent();

    }

    private async void OnObtenerUbicacionClicked(object sender, EventArgs e)
    {
        try
        {
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                MostrarAlerta("Permiso denegado. La app no tiene acceso a la ubicación.");
                return;
            }

            var location = await Geolocation.GetLastKnownLocationAsync();

            if (location == null)
            {
                location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Medium,
                    Timeout = TimeSpan.FromSeconds(10)
                });
            }

            if (location != null)
            {
                MostrarAlerta($"Latitud: {location.Latitude}\nLongitud: {location.Longitude}");
                
            }
            else
            {
                MostrarAlerta("Ubicación no disponible. No se pudo obtener la ubicación actual.");
            }
        }
        catch (Exception ex)
        {
            MostrarAlerta($"No se pudo obtener la ubicación.{ex.Message}");
        }
    }

    private void TemaOscuroSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        bool activarOscuro = e.Value;

        Preferences.Set("TemaOscuro", activarOscuro);

        // Esto aplica el tema globalmente en tiempo real
        Application.Current.UserAppTheme = activarOscuro ? AppTheme.Dark : AppTheme.Light;

        MostrarAlerta(activarOscuro ? "🌙 Tema oscuro activado." : "☀️ Tema claro activado.");
    }



    private void ToggleAvisoPrivacidad(object sender, EventArgs e)
    {
        AvisoPrivacidadPanel.IsVisible = !AvisoPrivacidadPanel.IsVisible;
    }


    private void CerrarSesion_Clicked(object sender, EventArgs e)
    {
        MostrarAlerta("🔒 Cerrando sesión...");
    }


    private void ToggleMisionVisionValores(object sender, EventArgs e)
    {
        MisionVisionValoresPanel.IsVisible = !MisionVisionValoresPanel.IsVisible;
    }

    private void ToggleSobreApp(object sender, EventArgs e)
    {
        SobreAppPanel.IsVisible = !SobreAppPanel.IsVisible;
    }

    private async Task EsperarAlertaCerradaAsync()
    {
        while (Alerta.IsVisible)
        {
            await Task.Delay(100);
        }
    }

    private async void MostrarAlerta(string mensaje)
    {
        EnviarContactoBtn.IsEnabled = false;
        Alerta.Mostrar(mensaje, () => {
            // callback cuando se cierra la alerta
        });

        await EsperarAlertaCerradaAsync();
        EnviarContactoBtn.IsEnabled = true;
    }


    private void CerrarAlerta(object sender, EventArgs e)
    {
        IsVisible = false;
    }


    private  void OnMostrarIdClicked(object sender, EventArgs e)
    {
        string deviceId =  DeviceIdHelper.GetOrCreateDeviceId();
        MostrarAlerta(deviceId);
    }

    private void LimpiarCampos()
    {
        NombreEntry.Text = "";
        CorreoEntry.Text = "";
        TelefonoEntry.Text = "";
        AreaPicker.SelectedIndex = -1;
        QuejaEditor.Text = "";
    }



    private void MostrarId_Clicked(object sender, EventArgs e)
    {
        // Tu lógica para mostrar el ID
        DisplayAlert("ID del dispositivo", "Aquí va el ID", "OK");
    }

    private void MostrarFormularioContacto(object sender, EventArgs e)
    {
        // Mostrar el formulario de contacto
        FormularioContacto.IsVisible = !FormularioContacto.IsVisible;
    }

    private async void EnviarContacto_Clicked(object sender, EventArgs e)
    {
        EnviarContactoBtn.IsEnabled = false; // 🔒 Desactiva el botón

        // Validaciones individuales
        if (string.IsNullOrWhiteSpace(NombreEntry.Text))
        {
            MostrarAlerta("⚠️ El campo *Nombre completo* es requerido.");
            return;
        }

        if (string.IsNullOrWhiteSpace(CorreoEntry.Text))
        {
            MostrarAlerta("⚠️ El campo *Correo electrónico* es requerido.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TelefonoEntry.Text))
        {
            MostrarAlerta("⚠️ El campo *Teléfono* es requerido.");
            return;
        }

        if (AreaPicker.SelectedItem == null)
        {
            MostrarAlerta("⚠️ Debes seleccionar un *Área*.");
            return;
        }

        if (string.IsNullOrWhiteSpace(QuejaEditor.Text))
        {
            MostrarAlerta("⚠️ El campo *Comentario* es requerido.");
            return;
        }

        var contacto = new
        {
            Nombre = NombreEntry.Text,
            Correo = CorreoEntry.Text,
            Telefono = TelefonoEntry.Text,
            Area = AreaPicker.SelectedItem?.ToString() ?? "",
            Comentario = QuejaEditor.Text
        };

        var json = JsonSerializer.Serialize(contacto);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        var httpClient = new HttpClient();

        try
        {
            var response = await httpClient.PostAsync("http://10.30.89.208:5070/api/Dispositivos/enviar", content);
            var respuesta = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var data = JsonSerializer.Deserialize<JsonElement>(respuesta);
                string mensaje = data.GetProperty("mensaje").GetString();
                await MostrarAlertaAsync("✅ " + mensaje);
                FormularioContacto.IsVisible = false; // Cierra el formulario
                LimpiarCampos(); // Limpia los campos si tienes ese método
            }
            else
            {
                var error = JsonSerializer.Deserialize<JsonElement>(respuesta);
                string mensaje = error.GetProperty("mensaje").GetString();
                await MostrarAlertaAsync("⚠️ " + mensaje);
            }

        }
        catch (Exception ex)
        {
            MostrarAlerta("❌ Error de conexión: " + ex.Message);
        }
        finally
        {
            EnviarContactoBtn.IsEnabled = true; // 🔓 Reactiva el botón
        }
    }

    private async Task MostrarAlertaAsync(string mensaje)
    {
        EnviarContactoBtn.IsEnabled = false;
        Alerta.Mostrar(mensaje, () => {
            // callback cuando se cierra la alerta
        });

        EnviarContactoBtn.IsEnabled = true;
    }



    private void CancelarContacto_Clicked(object sender, EventArgs e)
    {
        // Ocultar formulario y limpiar campos
        NombreEntry.Text = "";
        CorreoEntry.Text = "";
        TelefonoEntry.Text = "";
        AreaPicker.SelectedIndex = -1;
        QuejaEditor.Text = "";
        FormularioContacto.IsVisible = false;
    }

}