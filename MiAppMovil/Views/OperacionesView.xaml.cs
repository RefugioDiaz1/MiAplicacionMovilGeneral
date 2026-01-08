using MiAppMovil.Operaciones;

namespace MiAppMovil.Pages;

public partial class OperacionesView : ContentView
{
	public OperacionesView()
	{
		InitializeComponent();
	}

    private async void CotizadorRefacciones_Clicked(object sender, EventArgs e)
    {
        try
        {
            // Espera que la navegación esté lista
            await Task.Delay(100); // Le da tiempo a la UI para asegurar que Shell esté montado

            // Navega a la ruta si está registrada
            if (Shell.Current != null)
            {
                await Shell.Current.GoToAsync(nameof(CotizadorPage));
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Shell no está disponible. Verifica que AppShell sea la MainPage activa.", "OK");
            }
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }
    }



    private async void Unidades_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.Navigation.PushAsync(new UnidadesNuevasPage());
    }
}