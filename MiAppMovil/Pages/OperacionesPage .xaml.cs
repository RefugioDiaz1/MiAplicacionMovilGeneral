namespace MiAppMovil.Pages;

public partial class OperacionesPage : TabbedPage
{
	public OperacionesPage()
	{
		InitializeComponent();
	}

    private async void OnBackClicked(object sender, EventArgs e)
    {
        // Navegar hacia atrás (cerrar pestañas y volver a MainLayoutPage)
        // Asumiendo que esta página fue abierta con Navigation.PushAsync

        if (Navigation.NavigationStack.Count > 1)
            await Navigation.PopAsync();
        else
            await Navigation.PopModalAsync();
    }
}