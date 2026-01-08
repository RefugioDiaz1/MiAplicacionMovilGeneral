using MiAppMovil.Operaciones;
using MiAppMovil.Pages;

namespace MiAppMovil
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(CotizadorPage), typeof(CotizadorPage));
            Routing.RegisterRoute(nameof(UnidadesNuevasPage), typeof(UnidadesNuevasPage));
            Routing.RegisterRoute(nameof(EntregaRefaccionesPage), typeof(EntregaRefaccionesPage));
            Routing.RegisterRoute(nameof(FusionarArchivosPage), typeof(FusionarArchivosPage));
            Routing.RegisterRoute(nameof(ScanearDocumentosPage), typeof(ScanearDocumentosPage));
        }
    }
}
