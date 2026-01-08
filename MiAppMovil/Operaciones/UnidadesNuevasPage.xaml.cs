namespace MiAppMovil.Operaciones;

public partial class UnidadesNuevasPage : ContentPage
{
	public UnidadesNuevasPage()
	{
		InitializeComponent();
	}
    private async void OnBackButtonClicked(object sender, EventArgs e)
    {
        // Navega hacia atrás en la pila
        await Navigation.PopAsync();
    }
}