using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiAppMovil.Models.WebServiceResponse
{
    public class SucursalInfo
    {
        public string NombreSucursal { get; set; }
        public string Existencia { get; set; }
        public string Reservas { get; set; }
        public string Disponibles { get; set; }
        public string CostoMedio { get; set; }
        public string PrecioVenta { get; set; }
    }

}
