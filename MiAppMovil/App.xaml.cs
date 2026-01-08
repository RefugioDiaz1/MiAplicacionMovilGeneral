using MiAppMovil.Helpers;
using MiAppMovil.Models.ApiResponseId;
using MiAppMovil.Pages;
using System.Text;
using System.Text.Json;

namespace MiAppMovil
{
    public partial class App : Application
    {
        public static string MensajeBloqueo { get; set; } = "Este dispositivo no tiene acceso autorizado.";


        public App()
        {
            InitializeComponent();
            MainPage = new AppShell(); // Aquí defines tu Shell como página principal
          
            
            RegistrarDispositivoAsync().ConfigureAwait(false); // Llamada asíncrona sin esperar
                                                               // Llama el permiso después de que MainPage está configurada
            //Application.Current.Dispatcher.Dispatch(async () =>
            //{
            //    await Task.Delay(300); // ⏱️ pequeña espera para que aparezca bien la UI
            //    await ValidarDispositivoAsync(); // ⬅️ validación con timeout abajo
            //});

        }

        private async Task SolicitarPermisoUbicacion()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            }

            if (status != PermissionStatus.Granted)
            {
                // Aquí puedes mostrar un mensaje de alerta si quieres
                await Application.Current.MainPage.DisplayAlert("Permiso necesario", "Se requiere acceso a la ubicación para continuar.", "OK");
            }
        }

        public async Task ValidarDispositivoPublico()
        {
            await ValidarDispositivoAsync();
        }


        private async Task ValidarDispositivoAsync()
        {
            string deviceId = DeviceIdHelper.GetOrCreateDeviceId();
            var httpClient = new HttpClient();
            var json = JsonSerializer.Serialize(new { deviceId });
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var timeoutTask = Task.Delay(5000); // ⏱️ tiempo máximo
            var validacionTask = httpClient.PostAsync("http://10.30.89.208:5070/api/Dispositivos/validar", content);

            var completedTask = await Task.WhenAny(validacionTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                await Shell.Current.GoToAsync("//bloqueado");
                return;
            }

            try
            {
                var response = await validacionTask;
                if (!response.IsSuccessStatusCode)
                {
                    App.MensajeBloqueo = "❌ No se pudo conectar con el servidor. Verifica tu red.";
                    await Shell.Current.GoToAsync("//bloqueado");
                    return;
                }

                var body = await response.Content.ReadAsStringAsync();
                var resultado = JsonSerializer.Deserialize<ValidacionResponse>(body);

                if (!resultado.existe)
                {
                    App.MensajeBloqueo = "🚫 Dispositivo no registrado en la base de datos.";
                    await Shell.Current.GoToAsync("//bloqueado");
                    return;
                }

                if (!resultado.tienePermiso)
                {
                    App.MensajeBloqueo = "🔒 Este dispositivo no tiene permisos para acceder.";
                    await Shell.Current.GoToAsync("//bloqueado");
                    return;
                }


                // ✅ Acceso válido
                await Shell.Current.GoToAsync("//mainlayout");
                await Task.Delay(300);

                if (Shell.Current.CurrentPage is Pages.MainLayoutPage layout)
                {
                    string msg = resultado.esAdmin ? "Bienvenido, Administrador." : "Bienvenido, Cliente.";
                    await layout.MostrarAlerta(msg);
                }
            }
            catch (Exception ex)
            {
                App.MensajeBloqueo = "❌ Error inesperado: " + ex.Message;
                await Shell.Current.GoToAsync("//bloqueado");
            }
        }




        private async Task RegistrarDispositivoAsync()
        {
            try
            {
                var deviceId = DeviceIdHelper.GetOrCreateDeviceId();
                var modelo = DeviceInfo.Model;
                var telefono = DeviceInfo.Name; // o algún otro dato disponible
                var so = DeviceInfo.Platform.ToString();
                var versionSO = DeviceInfo.VersionString;

                var dispositivo = new
                {
                    DeviceId = deviceId,
                    Modelo = modelo,
                    Telefono = telefono,
                    SistemaOperativo = so,
                    VersionSO = versionSO
                };

                var json = JsonSerializer.Serialize(dispositivo);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var httpClient = new HttpClient();
                var response = await httpClient.PostAsync("http://10.30.89.208:5070/api/Dispositivos/registrar", content);

                // Puedes leer el resultado si quieres mostrar o registrar internamente
                // var respuesta = await response.Content.ReadAsStringAsync();
            }
            catch
            {
                // Error silencioso, no bloquea ni lanza excepción visible
            }
        }


        private Task MostrarAlerta(string mensaje)
        {
            return Application.Current?.MainPage?.DisplayAlert("Aviso", mensaje, "OK") ?? Task.CompletedTask;
        }

    }
}