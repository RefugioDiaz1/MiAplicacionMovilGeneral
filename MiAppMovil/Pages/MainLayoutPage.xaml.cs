namespace MiAppMovil.Pages;

public partial class MainLayoutPage : ContentPage
{
    public MainLayoutPage()
    {
        InitializeComponent();
        paginaActiva = 0;
        LoadPage(new InicioPage());
        ActualizarNavbar();
    }

    private int paginaActiva = 0; // 0 = Inicio, 1 = Perfil, 2 = Config
    public bool AlertaActiva => Alerta?.IsVisible == true;


    private void ActualizarNavbar()
    {
        // Limpia todos (fondo transparente)
        btnInicio.BackgroundColor = Colors.Transparent;
        lblInicio.TextColor = Colors.Black;

        btnOperaciones.BackgroundColor = Colors.Transparent;
        lblOperaciones.TextColor = Colors.Black;

        btnConfig.BackgroundColor = Colors.Transparent;
        lblConfig.TextColor = Colors.Black;

        // Marca el activo
        switch (paginaActiva)
        {
            case 0:
                btnInicio.BackgroundColor = Colors.LightBlue;
                lblInicio.TextColor = Colors.Blue;
                break;
            case 1:
                btnOperaciones.BackgroundColor = Colors.LightBlue;
                lblOperaciones.TextColor = Colors.Blue;
                break;
            case 2:
                btnConfig.BackgroundColor = Colors.LightBlue;
                lblConfig.TextColor = Colors.Blue;
                break;
        }
    }

   
    protected override bool OnBackButtonPressed()
    {
        if (AlertaActiva)
        {
            // Evitar cerrar la página mientras la alerta está visible
            return true; // true cancela el back button
        }
        return base.OnBackButtonPressed();
    }


    private void LoadPage(ContentPage page)
    {
        ContentArea.Content = page.Content;
    }


    private void GoInicio(object sender, EventArgs e)
    {
        paginaActiva = 0;
        LoadPage(new InicioPage());
        ActualizarNavbar();
    }

    private async void GoOperaciones(object sender, EventArgs e)
    {
        paginaActiva = 1;
        LoadPage(new OperacionesPage());

        ActualizarNavbar();
    }

    private void GoConfig(object sender, EventArgs e)
    {
        paginaActiva = 2;
        LoadPage(new ConfiguracionPage());
        ActualizarNavbar();
    }

    public async Task MostrarAlerta(string mensaje)
    {
        if (Alerta != null)
            await Alerta.MostrarAsync(mensaje);
    }

}