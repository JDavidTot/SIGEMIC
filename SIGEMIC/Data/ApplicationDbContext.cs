using Microsoft.EntityFrameworkCore;
using SIGEMIC.Models;

namespace SIGEMIC.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<PlanCredito> PlanesCredito { get; set; }
        public DbSet<Credito> Creditos { get; set; }
        public DbSet<Cuota> Cuotas { get; set; }
        public DbSet<FormaPago> FormasPago { get; set; }
        public DbSet<Pago> Pagos { get; set; }
        public DbSet<DocumentoCliente> DocumentosCliente { get; set; }
        public DbSet<Auditoria> Auditorias { get; set; }
        public DbSet<Banco> Bancos { get; set; }
        public DbSet<SolicitudPago> SolicitudesPago { get; set; }
        public DbSet<Permiso> Permisos { get; set; }
        public DbSet<UsuarioPermiso> UsuarioPermisos { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Usuario>()
                .HasOne(u => u.Cliente)
                .WithOne(c => c.Usuario)
                .HasForeignKey<Cliente>(c => c.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Banco>(entity =>
            {
                entity.HasKey(e => e.IdBanco);

                entity.HasIndex(e => new
                {
                    e.Nombre,
                    e.NumeroCuenta
                })
                .IsUnique();
            });


            modelBuilder.Entity<SolicitudPago>(entity =>
            {
                entity.HasKey(e => e.IdSolicitud);

                entity.Property(e => e.Monto)
                    .HasPrecision(14, 2);

                entity.HasIndex(e => new
                {
                    e.IdBanco,
                    e.NumeroBoleta
                })
                .IsUnique();

                entity.HasIndex(e => e.IdPago)
                .IsUnique();

                entity.HasOne(e => e.Cliente)
                    .WithMany()
                    .HasForeignKey(e => e.IdCliente)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Credito)
                    .WithMany()
                    .HasForeignKey(e => e.IdCredito)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Cuota)
                    .WithMany()
                    .HasForeignKey(e => e.IdCuota)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Banco)
                    .WithMany(e => e.SolicitudesPago)
                    .HasForeignKey(e => e.IdBanco)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.UsuarioRevision)
                    .WithMany()
                    .HasForeignKey(e => e.IdUsuarioRevision)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Pago)
                    .WithOne()
                    .HasForeignKey<SolicitudPago>(e => e.IdPago)
                    .OnDelete(DeleteBehavior.Restrict);
            });

                        modelBuilder.Entity<Permiso>(entity =>
                {
                    entity.ToTable("permisos");
                    entity.HasKey(e => e.IdPermiso);

                    entity.HasIndex(e => e.Codigo)
                        .IsUnique();

                    entity.Property(e => e.Estado)
                        .HasColumnType("enum('ACTIVO','INACTIVO')")
                        .HasDefaultValue("ACTIVO");

                    entity.Property(e => e.FechaCreacion)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");
                });

                modelBuilder.Entity<UsuarioPermiso>(entity =>
                {
                    entity.ToTable("usuario_permisos");

                    entity.HasKey(e => new
                    {
                        e.IdUsuario,
                        e.IdPermiso
                    });

                    entity.Property(e => e.FechaAsignacion)
                        .HasDefaultValueSql("CURRENT_TIMESTAMP");

                    entity.HasOne(e => e.Usuario)
                        .WithMany()
                        .HasForeignKey(e => e.IdUsuario)
                        .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(e => e.Permiso)
                        .WithMany()
                        .HasForeignKey(e => e.IdPermiso)
                        .OnDelete(DeleteBehavior.Restrict);
                });

        }
    }
}