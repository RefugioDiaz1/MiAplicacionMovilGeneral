using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiAppMovil.Models.WebServiceResponse
{
    public class RefaccionItem
    {
        public string NumeroParte { get; set; }
        public string Descripcion { get; set; }
        // Agrega esta propiedad para evitar recargar
        public List<SucursalInfo> Sucursales { get; set; }
    }

}
