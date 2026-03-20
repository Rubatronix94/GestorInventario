using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GestorInventario.Models;

namespace GestorInventario.Data
{
    // Cambiamos IdentityDbContext a IdentityDbContext<ApplicationUser>
    // para usar nuestro usuario personalizado con EmpresaId
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // ── DbSets ──────────────────────────────────────────────
        public DbSet<Empresa> Empresas { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<MovimientoStock> MovimientosStock { get; set; }

        // ── Configuración Fluent API ─────────────────────────────
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ── Relaciones Empresa ───────────────────────────────

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Categorias)
                .WithOne(c => c.Empresa)
                .HasForeignKey(c => c.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Proveedores)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Productos)
                .WithOne(p => p.Empresa)
                .HasForeignKey(p => p.EmpresaId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Relaciones Producto ──────────────────────────────

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(p => p.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Producto>()
                .HasOne(p => p.Proveedor)
                .WithMany(pr => pr.Productos)
                .HasForeignKey(p => p.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<MovimientoStock>()
                .HasOne(m => m.Producto)
                .WithMany(p => p.Movimientos)
                .HasForeignKey(m => m.ProductoId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Índices ──────────────────────────────────────────

            // Índice compuesto: búsquedas rápidas por empresa
            modelBuilder.Entity<Producto>()
                .HasIndex(p => new { p.EmpresaId, p.Nombre });

            modelBuilder.Entity<Categoria>()
                .HasIndex(c => c.EmpresaId);

            modelBuilder.Entity<Proveedor>()
                .HasIndex(p => p.EmpresaId);

            // ── Seed Data ────────────────────────────────────────
            // NOTA: El seed ahora requiere EmpresaId.
            // Creamos una empresa de demo para desarrollo.
            modelBuilder.Entity<Empresa>().HasData(
                new Empresa
                {
                    Id = 1,
                    Nombre = "Empresa Demo",
                    Cif = "B00000000",
                    Email = "demo@gestorinventario.com",
                    FechaRegistro = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                    Activa = true
                }
            );

            modelBuilder.Entity<Categoria>().HasData(
                new Categoria { Id = 1, Nombre = "Electrónica", Descripcion = "Dispositivos electrónicos y accesorios", EmpresaId = 1 },
                new Categoria { Id = 2, Nombre = "Ropa", Descripcion = "Prendas de vestir y complementos", EmpresaId = 1 },
                new Categoria { Id = 3, Nombre = "Alimentación", Descripcion = "Productos alimenticios", EmpresaId = 1 },
                new Categoria { Id = 4, Nombre = "Hogar", Descripcion = "Artículos para el hogar", EmpresaId = 1 }
            );

            modelBuilder.Entity<Proveedor>().HasData(
                new Proveedor { Id = 1, Nombre = "TechSupply SL", Email = "info@techsupply.es", Telefono = "960000001", EmpresaId = 1 },
                new Proveedor { Id = 2, Nombre = "Distribuciones García", Email = "ventas@garcia.es", Telefono = "960000002", EmpresaId = 1 }
            );

            modelBuilder.Entity<Producto>().HasData(
                new Producto { Id = 1, Nombre = "Teclado mecánico", Precio = 79.99m, Stock = 25, StockMinimo = 5, CategoriaId = 1, ProveedorId = 1, EmpresaId = 1, FechaAlta = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Producto { Id = 2, Nombre = "Ratón inalámbrico", Precio = 34.50m, Stock = 3, StockMinimo = 5, CategoriaId = 1, ProveedorId = 1, EmpresaId = 1, FechaAlta = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Producto { Id = 3, Nombre = "Camiseta básica", Precio = 12.00m, Stock = 50, StockMinimo = 10, CategoriaId = 2, ProveedorId = 2, EmpresaId = 1, FechaAlta = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        }
    }
}