using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiAppMovil.Models
{
    public class Dispositivo
    {
        public string DeviceId { get; set; }
        public bool EsAdmin { get; set; }
        public bool TienePermiso { get; set; }
    }
}
