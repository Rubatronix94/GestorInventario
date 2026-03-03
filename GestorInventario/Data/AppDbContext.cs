using Microsoft.EntityFrameworkCore;
using GestorInventario.Models;

namespace GestorInventario.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ──────────────────────────────────────────────
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }

        // ── Configuración Fluent API ─────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Producto → Categoria (obligatorio, no cascade delete por defecto)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Producto → Proveedor (opcional)
            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            // MovimientoStock → Producto
            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice en Producto.Nombre para búsquedas rápidas
            modelBuilder.Entity<Producto>()
                .HasIndex(p => p.Nombre);

            // ── Datos semilla (Seed Data) ────────────────────────
            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Electrónica",   Descripcion = "Dispositivos electrónicos y accesorios" },
                new Categoria { Id = 2, Nombre = "Ropa",          Descripcion = "Prendas de vestir y complementos" },
                new Categoria { Id = 3, Nombre = "Alimentación",  Descripcion = "Productos alimenticios" },
                new Categoria { Id = 4, Nombre = "Hogar",         Descripcion = "Artículos para el hogar" }
            );

            modelBuilder.Entity<Proveedor>().HasData(
                new Proveedor { Id = 1, Nombre = "TechSupply SL",   Email = "info@techsupply.es",  Telefono = "960000001" },
                new Proveedor { Id = 2, Nombre = "Distribuciones García", Email = "ventas@garcia.es", Telefono = "960000002" }
            );

            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Teclado mecánico",  Precio = 79.99m,  Stock = 25, StockMinimo = 5, CategoriaId = 1, ProveedorId = 1, FechaAlta = new DateTime(2025, 1, 1) },
                new Producto { Id = 2, Nombre = "Ratón inalámbrico", Precio = 34.50m,  Stock = 3,  StockMinimo = 5, CategoriaId = 1, ProveedorId = 1, FechaAlta = new DateTime(2025, 1, 1) },
                new Producto { Id = 3, Nombre = "Camiseta básica",   Precio = 12.00m,  Stock = 50, StockMinimo = 10, CategoriaId = 2, ProveedorId = 2, FechaAlta = new DateTime(2025, 1, 1) }
            );
        }
    }
}
