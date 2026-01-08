using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiAppMovil.Models.ApiResponseId
{
    public class ValidacionResponse
    {
        public bool existe { get; set; }
        public bool esAdmin { get; set; }
        public bool tienePermiso { get; set; }
    }

}
