namespace MiAppMovil;

public partial class CustomAlert : ContentView
{
    private Action _onOk;

    public CustomAlert()
    {
        InitializeComponent();
        this.IsVisible = false;
    }
    public Task MostrarAsync(string mensaje)
    {
        var tcs = new TaskCompletionSource<bool>();
        _onOk = () => tcs.SetResult(true);
        MensajeLabel.Text = mensaje;
        this.IsVisible = true;
        return tcs.Task;
    }

    public void Mostrar(string mensaje, Action onOk = null)
    {
        _onOk = onOk;
        MensajeLabel.Text = mensaje;
        this.IsVisible = true;
    }

    private void BtnOk_Clicked(object sender, EventArgs e)
    {
        this.IsVisible = false;
        _onOk?.Invoke();
    }
}

