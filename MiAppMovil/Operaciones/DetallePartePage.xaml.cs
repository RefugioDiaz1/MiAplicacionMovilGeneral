using MiAppMovil.Models.WebServiceResponse;
using System.Net;
using System.Text;
using System.Xml.Linq;

namespace MiAppMovil.Operaciones;

public partial class DetallePartePage : ContentPage
{
    private static readonly HttpClient _httpClient = new HttpClient();

    private readonly Dictionary<string, string> SucursalDiccionario = new()
    {
        {"11", "11 - Atasta"}, {"12", "12 - Centro"}, {"16", "16 - FAD Veracruz"},
        {"31", "31 - Carmen"}, {"51", "51 - Chontalpa"}, {"52", "52 - Comalcalco"},
        {"81", "81 - Centro"}, {"82", "82 - Atasta"}, {"83", "83 - Chontalpa"},
        {"84", "84 - Comalcalco"}, {"85", "85 - Carmen"}
    };

    private readonly string _numeroParte;
    private readonly string _descripcion;
    private readonly List<SucursalInfo> _sucursales;


    public DetallePartePage(string numeroParte)
    {
        InitializeComponent();
        _numeroParte = numeroParte;
       
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarDetalleAsync(_numeroParte);
    }

    private void MostrarCargando(bool mostrar)
    {
        LoadingIndicator.IsVisible = mostrar;
        LoadingIndicator.IsRunning = mostrar;
    }

    private async Task CargarDetalleAsync(string numeroParte)
    {
        try
        {
            MostrarCargando(true);

            string soapRequest = $@"
            <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"" xmlns:ser=""http://service.ford.com/"">
                <soapenv:Header/>
                <soapenv:Body>
                    <ser:getInfoPartev3>
                        <parte>{numeroParte.ToUpper()}</parte>
                        <empresa>F</empresa>
                        <id>3781e50f0eb2973a99964b8b710a9999</id>
                    </ser:getInfoPartev3>
                </soapenv:Body>
            </soapenv:Envelope>";

            var content = new StringContent(soapRequest, Encoding.UTF8, "text/xml");
            content.Headers.Add("SOAPAction", "");

            var response = await _httpClient.PostAsync("https://fordsureste.mx/WSRefacciones/WebServiceGP", content);
            string responseXml = await response.Content.ReadAsStringAsync();
            string raw = ObtenerContenidoReturnDecodificado(responseXml);

            if (raw.Contains("<v>NO</v>"))
            {
                await DisplayAlert("Error", "No se encontró el número de parte.", "OK");
                await Navigation.PopAsync();
                return;
            }

            string parte = ExtraerTag(raw, "p");
            string descripcion = ExtraerTag(raw, "d");

            ParteLabel.Text = parte;
            DescripcionLabel.Text = descripcion;

            var sucursales = ObtenerSucursalesDesdeXML(raw);
            DetalleView.ItemsSource = sucursales;

            int total = sucursales.Sum(s => int.TryParse(s.Existencia, out int e) ? e : 0);
            ExistenciaTotalLabel.Text = total.ToString();
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"Error al consultar: {ex.Message}", "OK");
        }
        finally
        {
            MostrarCargando(false);
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


    //private async void CargarDetalle()
    //{
    //    try
    //    {
    //        var resultado = await _client.getInfoPartev3Async(_numeroParte, "F", "3781e50f0eb2973a99964b8b710a9999");
    //        var raw = resultado.Body.@return;

    //        if (raw.Contains("<v>NO</v>"))
    //        {
    //            await DisplayAlert("Atención", "No se encontró el número de parte ingresado.", "OK");
    //            await Navigation.PopAsync(); // regresar
    //            return;
    //        }

    //        ParteLabel.Text = ExtraerTag(raw, "p");
    //        DescripcionLabel.Text = ExtraerTag(raw, "d");

    //        var sucursales = new List<SucursalInfo>();

    //        foreach (var kvp in SucursalDiccionario)
    //        {
    //            string id = kvp.Key;
    //            string nombre = kvp.Value;

    //            if (raw.Contains($"<e{id}>"))
    //            {
    //                sucursales.Add(new SucursalInfo
    //                {
    //                    NombreSucursal = nombre,
    //                    Existencia = ExtraerTag(raw, $"e{id}"),
    //                    Reservas = ExtraerTag(raw, $"r{id}"),
    //                    Disponibles = ExtraerTag(raw, $"di{id}"),
    //                    CostoMedio = "$" + Formatear(ExtraerTag(raw, $"c{id}")),
    //                    PrecioVenta = "$" + Formatear(ExtraerTag(raw, $"pv{id}"))
    //                });
    //            }
    //        }

    //        int totalExistencia = sucursales.Sum(s =>
    //        {
    //            int.TryParse(s.Existencia, out int existencia);
    //            return existencia;
    //        });

    //        // Mostrar total en el Entry correspondiente
    //        ExistenciaTotalLabel.Text = totalExistencia.ToString();

    //        DetalleView.ItemsSource = sucursales;
    //    }
    //    catch (Exception ex)
    //    {
    //        await DisplayAlert("Error", "Error al consultar: " + ex.Message, "OK");
    //    }
    //}

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




    private string ExtraerTag(string xml, string tag)
    {
        var start = $"<{tag}>";
        var end = $"</{tag}>";
        var i1 = xml.IndexOf(start);
        var i2 = xml.IndexOf(end);
        if (i1 == -1 || i2 == -1 || i2 <= i1) return string.Empty;
        return xml.Substring(i1 + start.Length, i2 - i1 - start.Length);
    }

    private async void Volver_Clicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync(); // Regresa
    }
}
