using System;
using System.Collections.Generic;
using System.Text;

namespace Abtracciones.Excepciones
{
    public class ReglaNegocio : Exception
    {
        public ReglaNegocio(string mensaje) : base(mensaje) { }
    }
    public class NotFoundException : Exception
    {
        public NotFoundException(string mensaje) : base(mensaje) { }
    }
}