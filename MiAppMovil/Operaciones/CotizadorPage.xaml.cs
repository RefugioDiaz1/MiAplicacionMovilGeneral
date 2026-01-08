
using MiAppMovil.Models.WebServiceResponse;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace MiAppMovil.Operaciones;

public partial class CotizadorPage : ContentPage
{
    private static readonly HttpClient _httpClient = new HttpClient();
    public CotizadorPage()
	{
		InitializeComponent();
        BindingContext = this;
        //_client = new WebServiceGPClient(WebServiceGPClient.EndpointConfiguration.WebServiceGPPort);
    }

    public Command OpenFordPageCommand => new Command(() =>
    {
        Launcher.OpenAsync("https://www.ford.mx");
    });

    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Navega hacia atrás en la pila
        await Navigation.PopAsync();
    }

    private void FordSwitch_Toggled(object sender, ToggledEventArgs e)
    {
        if (e.Value) // Switch encendido
        {
            FordLabel.Text = "Ford - 1/";
        }
        else // Switch apagado
        {
            FordLabel.Text = "No Ford - 0/";
        }
    }

    private void MostrarError(string mensaje)
    {
        ErrorAlertLabel.Text = mensaje;
        ErrorAlert.IsVisible = true;
    }
    private void CerrarErrorAlert_Clicked(object sender, EventArgs e)
    {
        ErrorAlert.IsVisible = false;
    }

    //private async void ResultadosView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    //{
    //    if (e.CurrentSelection.Count == 0)
    //        return;

    //    var seleccionado = e.CurrentSelection.FirstOrDefault() as RefaccionItem;
    //    if (seleccionado == null)
    //        return;

    //    // Aquí llamas al mismo método que usarás desde el botón Buscar
    //    await BuscarPorNumeroParte(seleccionado.NumeroParte);

    //    // Limpia la selección visual
    //    ResultadosView.SelectedItem = null;
    //}

    private readonly Dictionary<string, string> SucursalDiccionario = new()
{
    {"11", "11 - Atasta"},
    {"12", "12 - Centro"},
    {"16", "16 - FAD Veracruz"},
    {"31", "31 - Carmen"},
    {"51", "51 - Chontalpa"},
    {"52", "52 - Comalcalco"},
    {"81", "81 - Centro"},
    {"82", "82 - Atasta"},
    {"83", "83 - Chontalpa"},
    {"84", "84 - Comalcalco"},
    {"85", "85 - Carmen"}
};

    private async void OnResultadoTapped(object sender, TappedEventArgs e)
    {
        if (e.Parameter is RefaccionItem item && sender is VisualElement card)
        {
            await card.FadeTo(0.6, 70);
            await card.FadeTo(1, 70);

            await Navigation.PushAsync(new DetallePartePage(item.NumeroParte));
        }
    }






    private void MostrarCargando(bool mostrar)
    {
        LoadingIndicator.IsVisible = mostrar;
        LoadingIndicator.IsRunning = mostrar;
    }


    //private async Task BuscarPorNumeroParte(string numeroParte)
    //{
    //    OcultarTeclado();
    //    ErrorAlert.IsVisible = false;

    //    if (string.IsNullOrWhiteSpace(numeroParte))
    //    {
    //        MostrarError("Debe ingresar un número de parte.");
    //        return;
    //    }

    //    try
    //    {
    //        var resultado = await _client.getInfoPartev3Async(numeroParte, "F", "3781e50f0eb2973a99964b8b710a9999");
    //        var raw = resultado.Body.@return;

    //        if (raw.Contains("<v>NO</v>"))
    //        {
    //            MostrarError("No se encontró el número de parte ingresado.");
    //            ResultadosView.ItemsSource = null;
    //            DetalleView.ItemsSource = null;
    //            ParteLabel.Text = string.Empty;
    //            DescripcionLabel.Text = string.Empty;
    //            return;
    //        }

    //        // Extraer datos generales
    //        string parte = ExtraerTag(raw, "p");
    //        string descripcion = ExtraerTag(raw, "d");

    //        // Mostrar encabezado
    //        ParteLabel.Text = parte;
    //        DescripcionLabel.Text = descripcion;

    //        // Usar método centralizado
    //        var sucursales = ObtenerSucursalesDesdeXML(raw);

    //        // Mostrar resultados
    //        DetalleView.ItemsSource = sucursales;
    //    }
    //    catch (Exception ex)
    //    {
    //        MostrarError("Error al consultar: " + ex.Message);
    //    }
    //}




    private string ExtraerTag(string raw, string tag)
    {
        var inicio = raw.IndexOf($"<{tag}>") + tag.Length + 2;
        var fin = raw.IndexOf($"</{tag}>");
        if (inicio < tag.Length + 2 || fin < 0 || fin <= inicio)
            return "-";
        return raw[inicio..fin];
    }




    private async void Buscar_Clicked(object sender, EventArgs e)
    {
        MostrarCargando(true);
        ResultadosView.ItemsSource = null;
        ErrorAlert.IsVisible = false;

        var input = NumeroParteEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(input))
        {
            MostrarError("Debe agregar un número de parte.");
            MostrarCargando(false);
            return;
        }

        if (!input.StartsWith("0/") && !input.StartsWith("1/"))
        {
            string prefijo = FordSwitch.IsToggled ? "1/" : "0/";
            input = prefijo.ToUpper() + input.ToUpper();
        }
        else
        {
            input = input.ToUpper();
        }

            try
            {
                string soapRequest = $@"
        <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.ford.com/"">
            <soapenv:Header/>
            <soapenv:Body>
                <ser:getInfoPartev3>
                    <parte>{input}</parte>
                    <empresa>F</empresa>
                    <id>3781e50f0eb2973a99964b8b710a9999</id>
                </ser:getInfoPartev3>
            </soapenv:Body>
        </soapenv:Envelope>";

                var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
                content.Headers.Add("SOAPAction", "");

                var response = await _httpClient.PostAsync("https://fordsureste.mx/WSRefacciones/WebServiceGP", content);
                var responseXml = await response.Content.ReadAsStringAsync();

                string raw = ObtenerContenidoReturnDecodificado(responseXml);

                if (raw.Contains("<v>NO</v>"))
                {
                    MostrarError("No se encontró el número de parte ingresado.");
                    MostrarCargando(false);
                    return;
                }

                string parte = ExtraerTag(raw, "p");
                //string descripcion = ExtraerTag(raw, "d");
                //var sucursales = ObtenerSucursalesDesdeXML(raw);

                //// Si quieres mostrar resultados en la lista:
                //ResultadosView.ItemsSource = sucursales;

                // O navega a la página detalle con el número de parte
                await Navigation.PushAsync(new DetallePartePage(parte));
            }
            catch (Exception ex)
            {
                MostrarError("Error al consultar: " + ex.Message);
            }
            finally
            {
                MostrarCargando(false);
            }
    }


    private string ObtenerContenidoReturnDecodificado(string responseXml)
    {
        try
        {
            var doc = XDocument.Parse(responseXml);

            // Encuentra el nodo <return> sin importar el prefijo
            var returnElement = doc
                .Descendants()
                .FirstOrDefault(e => e.Name.LocalName == "return");

            if (returnElement == null)
                return "";

            return WebUtility.HtmlDecode(returnElement.Value);
        }
        catch (Exception ex)
        {
            Console.WriteLine("❌ Error al parsear XML: " + ex.Message);
            return "";
        }
    }


    public List<SucursalInfo> ObtenerSucursalesDesdeXML(string xml)
    {
        var lista = new List<SucursalInfo>();

        foreach (var kvp in SucursalDiccionario)
        {
            string codigo = kvp.Key;
            string nombre = kvp.Value;

            try
            {
                string existencia = ExtraerValor(xml, $"<e{codigo}>", $"</e{codigo}>");
                string reservas = ExtraerValor(xml, $"<r{codigo}>", $"</r{codigo}>");
                string disponibles = ExtraerValor(xml, $"<di{codigo}>", $"</di{codigo}>");
                string costoMedio = ExtraerValor(xml, $"<c{codigo}>", $"</c{codigo}>");
                string precioVenta = ExtraerValor(xml, $"<pv{codigo}>", $"</pv{codigo}>");

                // Solo agrega si hay datos relevantes
                if (!string.IsNullOrWhiteSpace(existencia) || !string.IsNullOrWhiteSpace(reservas))
                {
                    lista.Add(new SucursalInfo
                    {
                        NombreSucursal = nombre,
                        Existencia = existencia,
                        Reservas = reservas,
                        Disponibles = disponibles,
                        CostoMedio = costoMedio,
                        PrecioVenta = precioVenta
                    });
                }
            }
            catch
            {
                // Puedes loguear si lo deseas, pero se omite la sucursal si no hay datos
            }
        }

        return lista;
    }

    private string ExtraerValor(string xml, string inicio, string fin)
    {
        int i = xml.IndexOf(inicio);
        if (i == -1) return string.Empty;

        i += inicio.Length;
        int j = xml.IndexOf(fin, i);
        if (j == -1) return string.Empty;

        return xml.Substring(i, j - i).Trim();
    }


    //    private void OcultarTeclado()
    //    {
    //#if ANDROID
    //    Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.CurrentFocus?.ClearFocus();
    //#else
    //        NumeroParteEntry.Unfocus();
    //#endif
    //    }


    private async void Filtrar_Clicked(object sender, EventArgs e)
    {
        ResultadosView.ItemsSource = null;
        ErrorAlert.IsVisible = false;

        string texto = NumeroParteEntry.Text?.Trim();

        if (string.IsNullOrWhiteSpace(texto))
        {
            MostrarError("Debe agregar un número de parte.");
            return;
        }

        string prefijo = FordSwitch.IsToggled ? "1/" : "0/";
        string filtro = $"{prefijo}%{texto.ToUpper()}%";

        try
        {
            MostrarCargando(true);

            string xmlResponse = await ConsumirSoapFiltradoAsync(filtro, "F", "3781e50f0eb2973a99964b8b710a9999");

            if (xmlResponse.Contains("<v>NO</v>"))
            {
                MostrarError("No se encontró el número de parte ingresado.");
                ResultadosView.ItemsSource = null;
                return;
            }

            var items = ParsearResultado(xmlResponse); // ya tienes este método
            ResultadosView.ItemsSource = items;
        }
        catch (Exception ex)
        {
            MostrarError($"Error al filtrar: {ex.Message}");
        }
        finally
        {
            MostrarCargando(false);
        }
    }


    public async Task<string> ConsumirSoapFiltradoAsync(string filtro, string empresa, string token)
    {
        var soapEnvelope = $@"
    <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.ford.com/"">
       <soapenv:Header/>
       <soapenv:Body>
          <ser:getFiltradoPartev3>
             <semiparte>{filtro}</semiparte>
             <empresa>{empresa}</empresa>
             <id>{token}</id>
          </ser:getFiltradoPartev3>
       </soapenv:Body>
    </soapenv:Envelope>";

        var content = new StringContent(soapEnvelope, Encoding.UTF8, "text/xml");
        content.Headers.Clear();
        content.Headers.Add("Content-Type", "text/xml; charset=utf-8");
        content.Headers.Add("SOAPAction", "");

        using var client = new HttpClient();
        var response = await client.PostAsync("https://fordsureste.mx/WSRefacciones/WebServiceGP", content);

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Error SOAP: {response.StatusCode}");

        return await response.Content.ReadAsStringAsync();
    }



    private List<RefaccionItem> ParsearResultado(string raw)
    {
        var items = new List<RefaccionItem>();

        // Extraer todo lo que esté entre $1$ y $2$
        var inicio = raw.IndexOf("$1$");
        var fin = raw.IndexOf("$2$");

        if (inicio == -1 || fin == -1 || fin <= inicio) return items;

        var contenido = raw.Substring(inicio + 3, fin - (inicio + 3));

        // Separar por el delimitador de registros ¿?
        var registros = contenido.Split("¿?", StringSplitOptions.RemoveEmptyEntries);

        foreach (var registro in registros)
        {
            var partes = registro.Split('#');
            if (partes.Length == 2)
            {
                items.Add(new RefaccionItem
                {
                    NumeroParte = partes[0],
                    Descripcion = partes[1]
                });
            }
        }

        return items;
    }



    private void Limpiar_Clicked(object sender, EventArgs e)
    {
       // OcultarTeclado();

        // Limpia el campo de entrada
        NumeroParteEntry.Text = string.Empty;

        // Limpia los resultados mostrados
        ResultadosView.ItemsSource = null;

        // Oculta cualquier alerta activa
        ErrorAlert.IsVisible = false;

        // (opcional) restablece el switch a True si quieres
        // FordSwitch.IsToggled = true;
    }

}