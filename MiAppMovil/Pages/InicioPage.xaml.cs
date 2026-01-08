namespace MiAppMovil.Pages;

public partial class InicioPage : ContentPage
{
	public InicioPage()
	{
		InitializeComponent();

	}
 

    private void ComenzarClicked(object sender, EventArgs e)
    {
        // Aquí puedes definir acción al dar clic, como navegar a otra página
        DisplayAlert("¡Listo!", "Vamos a comenzar.", "OK");
    }
}