namespace MiAppMovil.Pages;

public partial class BloqueadoPage : ContentPage
{
	public BloqueadoPage()
	{
		InitializeComponent();
	}
    protected override void OnAppearing()
    {
        base.OnAppearing();
        MensajeLabel.Text = App.MensajeBloqueo ?? "Este dispositivo no tiene acceso autorizado.";
    }


    public async Task MostrarAlerta(string mensaje)
    {
        if (Alerta != null)
            await Alerta.MostrarAsync(mensaje);
    }

    private async void Revalidar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//validando");
        await Task.Delay(500);
        await ((App)Application.Current).ValidarDispositivoPublico();
    }


}