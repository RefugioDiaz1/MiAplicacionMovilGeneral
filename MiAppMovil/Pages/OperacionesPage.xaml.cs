using MiAppMovil.Operaciones;

namespace MiAppMovil.Pages;

public partial class OperacionesPage : ContentPage
{
	public OperacionesPage()
	{
		InitializeComponent();

	}
    private async void CotizadorRefacciones_Clicked(object sender, EventArgs e)
    {
        if (Shell.Current == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Shell.Current es null. Verifica que MainPage = new AppShell();", "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(CotizadorPage));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }
    }



    private async void Unidades_Clicked(object sender, EventArgs e)
    {
        if (Shell.Current == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Shell.Current es null. Verifica que MainPage = new AppShell();", "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(UnidadesNuevasPage));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }
       
    }

    private async void EntregaRefacciones_Clicked(object sender, EventArgs e)
    {
        if (Shell.Current == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Shell.Current es null. Verifica que MainPage = new AppShell();", "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(EntregaRefaccionesPage));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }

    }


    private async void FusionarArchivos_Clicked(object sender, EventArgs e)
    {
        if (Shell.Current == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Shell.Current es null. Verifica que MainPage = new AppShell();", "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(FusionarArchivosPage));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }

    }

    private async void EscanearDocumentos_Clicked(object sender, EventArgs e)
    {
        if (Shell.Current == null)
        {
            await Application.Current.MainPage.DisplayAlert("Error", "Shell.Current es null. Verifica que MainPage = new AppShell();", "OK");
            return;
        }

        try
        {
            await Shell.Current.GoToAsync(nameof(ScanearDocumentosPage));
        }
        catch (Exception ex)
        {
            await Application.Current.MainPage.DisplayAlert("Excepción", ex.Message, "OK");
        }

    }


}