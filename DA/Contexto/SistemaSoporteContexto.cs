using Abtracciones.Modelos;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace DA.Contexto
{
    //sesion de trbaajo contra la base de datos que vamos a crear sistema soporte
    //solo capa de acceso a datos conoce este tipo, el resto del sistema habla con interfaces 
    public class SistemaSoporteContexto : DbContext
    {
        //las opciones(proveedor, cadena de conexión) llegan por inyección de dependencias desde program.cs
        public SistemaSoporteContexto(DbContextOptions<SistemaSoporteContexto> options) : base(options)
        {
        }
        //cada dbset se convierte en una tabla, el nombre de la propiedad define el nombre de la tabla
        public DbSet<Cliente> Clientes => Set<Cliente>();
    }
}
