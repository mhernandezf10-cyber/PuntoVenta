using Microsoft.EntityFrameworkCore;

namespace ApiEmpresa.Models
{
    public class Conexiones : DbContext
    {
        public Conexiones(DbContextOptions<Conexiones> options)
            : base(options)
        {
        }

        public DbSet<Clientes> Clientes { get; set; }

        public DbSet<Usuarios> Usuarios { get; set; }

        public DbSet<Empleados> Empleados { get; set; }

        public DbSet<Puestos> Puestos { get; set; }

        public DbSet<Proveedores> Proveedores { get; set; }

        public DbSet<Marcas> Marcas { get; set; }

        public DbSet<Productos> Productos { get; set; }

        public DbSet<Compras> Compras { get; set; }

        public DbSet<Compras_detalle> Compras_detalle { get; set; }

        public DbSet<Ventas> Ventas { get; set; }

public DbSet<Ventas_detalle> Ventas_detalle { get; set; }

    }
}