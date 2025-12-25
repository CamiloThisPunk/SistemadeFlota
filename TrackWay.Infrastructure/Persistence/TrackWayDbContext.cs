using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Domain.Entities.Auth;
using TrackWay.Domain.Entities.SaaS;
using TrackWay.Domain.Enums;
using TrackWay.Application.Auth.Handlers;
using TrackWay.Application.SuperAdmin;

namespace TrackWay.Infrastructure.Persistence;

/// <summary>
/// DbContext unificado para TrackWay - Usa entidades DDD
/// </summary>
public class TrackWayDbContext : DbContext, IAuthDbContext, ISaaSDbContext
{
    public TrackWayDbContext(DbContextOptions<TrackWayDbContext> options) : base(options) { }

    // ============ Auth Entities ============
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<Role> Roles { get; set; } = null!;

    // ============ Fleet DDD Entities ============
    public DbSet<Vehicle> Vehicles { get; set; } = null!;
    public DbSet<Driver> Drivers { get; set; } = null!;
    public DbSet<MaintenanceOrder> MaintenanceOrders { get; set; } = null!;
    public DbSet<FuelLoad> FuelLoads { get; set; } = null!;
    public DbSet<VehicleDocument> VehicleDocuments { get; set; } = null!;

    // ============ Restaurante Entities ============
    public DbSet<Categoria> Categorias { get; set; } = null!;
    public DbSet<Producto> Productos { get; set; } = null!;
    public DbSet<Mesa> Mesas { get; set; } = null!;
    public DbSet<Empleado> Empleados { get; set; } = null!;
    public DbSet<Orden> Ordenes { get; set; } = null!;
    public DbSet<DetalleOrden> DetallesOrden { get; set; } = null!;
    public DbSet<Reservacion> Reservaciones { get; set; } = null!;

    // ============ SaaS Entities ============
    public DbSet<Tenant> Tenants { get; set; } = null!;
    public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; } = null!;
    public DbSet<TenantSubscription> TenantSubscriptions { get; set; } = null!;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        ConfigureAuthEntities(modelBuilder);
        ConfigureFleetEntities(modelBuilder);
        ConfigureRestauranteEntities(modelBuilder);
        ConfigureSaaSEntities(modelBuilder);
    }

    private void ConfigureAuthEntities(ModelBuilder modelBuilder)
    {
        // ============ Role ============
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles", "auth");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Descripcion).HasMaxLength(200);
            entity.Property(e => e.Permisos).HasMaxLength(2000);
        });

        // ============ User ============
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users", "auth");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Email).IsRequired().HasMaxLength(256);
            entity.HasIndex(e => e.Email).IsUnique();
            
            entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.Property(e => e.RefreshToken).HasMaxLength(500);
            
            entity.HasOne(e => e.Role)
                .WithMany(r => r.Usuarios)
                .HasForeignKey(e => e.RoleId)
                .OnDelete(DeleteBehavior.Restrict);
            
            // Ignorar propiedades calculadas
            entity.Ignore(e => e.EstaBloqueado);
        });
    }

    private void ConfigureFleetEntities(ModelBuilder modelBuilder)
    {
        // ============ Vehicle ============
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.ToTable("Vehicles", "fleet");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Placa).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Placa).IsUnique();
            
            entity.Property(e => e.VIN).HasMaxLength(50);
            entity.HasIndex(e => e.VIN).IsUnique().HasFilter("[VIN] IS NOT NULL AND [VIN] <> ''");
            
            entity.Property(e => e.Marca).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Modelo).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).HasMaxLength(30);
            
            entity.Property(e => e.Combustible).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.Estado).HasConversion<string>().HasMaxLength(30);
            
            entity.Property(e => e.KmActual).HasPrecision(12, 2);
            entity.Property(e => e.CapacidadTanque).HasPrecision(8, 2);
            
            entity.HasOne(e => e.ConductorAsignado)
                .WithOne(d => d.VehiculoAsignado)
                .HasForeignKey<Vehicle>(e => e.ConductorAsignadoId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.Ignore(e => e.DomainEvents);
            
            // Configurar que use el backing field para las colecciones de solo lectura
            entity.Navigation(e => e.OrdenesMantenimiento)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(e => e.CargasCombustible)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            entity.Navigation(e => e.Documentos)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });

        // ============ Driver ============
        modelBuilder.Entity<Driver>(entity =>
        {
            entity.ToTable("Drivers", "fleet");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Documento).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Documento).IsUnique();
            
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.FotoUrl).HasMaxLength(500);
            
            entity.Property(e => e.LicenciaNum).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.LicenciaNum).IsUnique();
            
            entity.Property(e => e.CategoriaLicencia).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ScoringSeguridad).HasPrecision(5, 2);
            entity.Property(e => e.KmTotalesRecorridos).HasPrecision(12, 2);
            
            entity.Ignore(e => e.DomainEvents);
            entity.Ignore(e => e.NombreCompleto);
            entity.Ignore(e => e.DiasParaVencimientoLicencia);
            entity.Ignore(e => e.LicenciaVigente);
        });

        // ============ MaintenanceOrder ============
        modelBuilder.Entity<MaintenanceOrder>(entity =>
        {
            entity.ToTable("MaintenanceOrders", "fleet");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Estado).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Observaciones).HasMaxLength(1000);
            entity.Property(e => e.Proveedor).HasMaxLength(200);
            
            entity.Property(e => e.KmAlMomento).HasPrecision(12, 2);
            entity.Property(e => e.KmProximoMantenimiento).HasPrecision(12, 2);
            entity.Property(e => e.CostoEstimado).HasPrecision(12, 2);
            entity.Property(e => e.CostoReal).HasPrecision(12, 2);
            
            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.OrdenesMantenimiento)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Ignore(e => e.EstaVencida);
            entity.Ignore(e => e.VariacionCosto);
        });

        // ============ FuelLoad ============
        modelBuilder.Entity<FuelLoad>(entity =>
        {
            entity.ToTable("FuelLoads", "fleet");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Galones).HasPrecision(10, 3);
            entity.Property(e => e.PrecioPorGalon).HasPrecision(10, 4);
            entity.Property(e => e.KmOdometro).HasPrecision(12, 2);
            entity.Property(e => e.Estacion).HasMaxLength(200);
            entity.Property(e => e.NumeroVoucher).HasMaxLength(50);
            
            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.CargasCombustible)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Conductor)
                .WithMany()
                .HasForeignKey(e => e.ConductorId)
                .OnDelete(DeleteBehavior.SetNull);
            
            entity.Ignore(e => e.Total);
        });

        // ============ VehicleDocument ============
        modelBuilder.Entity<VehicleDocument>(entity =>
        {
            entity.ToTable("VehicleDocuments", "fleet");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Tipo).HasConversion<string>().HasMaxLength(30);
            entity.Property(e => e.NumeroDocumento).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ArchivoUrl).HasMaxLength(500);
            entity.Property(e => e.Emisor).HasMaxLength(200);
            entity.Property(e => e.Costo).HasPrecision(12, 2);
            
            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.Documentos)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.Ignore(e => e.EstaVigente);
            entity.Ignore(e => e.EstaVencido);
            entity.Ignore(e => e.DiasParaVencimiento);
        });
    }

    private void ConfigureRestauranteEntities(ModelBuilder modelBuilder)
    {
        // ============ Categoria ============
        modelBuilder.Entity<Categoria>(entity =>
        {
            entity.ToTable("Categorias", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Icono).HasMaxLength(50);
        });

        // ============ Producto ============
        modelBuilder.Entity<Producto>(entity =>
        {
            entity.ToTable("Productos", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.Precio).HasPrecision(10, 2);
            entity.Property(e => e.PrecioDescuento).HasPrecision(10, 2);
            entity.Property(e => e.ImagenUrl).HasMaxLength(500);

            entity.HasOne(e => e.Categoria)
                .WithMany(c => c.Productos)
                .HasForeignKey(e => e.CategoriaId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ Mesa ============
        modelBuilder.Entity<Mesa>(entity =>
        {
            entity.ToTable("Mesas", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Numero).IsRequired().HasMaxLength(10);
            entity.HasIndex(e => e.Numero).IsUnique();
            entity.Property(e => e.Ubicacion).HasMaxLength(50);
            entity.Property(e => e.Estado).HasMaxLength(20);
        });

        // ============ Empleado ============
        modelBuilder.Entity<Empleado>(entity =>
        {
            entity.ToTable("Empleados", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Documento).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.Documento).IsUnique();
            entity.Property(e => e.Cargo).HasMaxLength(50);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.SalarioBase).HasPrecision(10, 2);
            entity.Property(e => e.Estado).HasMaxLength(20);
        });

        // ============ Orden ============
        modelBuilder.Entity<Orden>(entity =>
        {
            entity.ToTable("Ordenes", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NumeroOrden).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.NumeroOrden).IsUnique();
            entity.Property(e => e.TipoServicio).HasMaxLength(30);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.MetodoPago).HasMaxLength(30);
            entity.Property(e => e.NombreCliente).HasMaxLength(100);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.Impuestos).HasPrecision(10, 2);
            entity.Property(e => e.Descuento).HasPrecision(10, 2);
            entity.Property(e => e.Total).HasPrecision(10, 2);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Mesa)
                .WithMany(m => m.Ordenes)
                .HasForeignKey(e => e.MesaId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.Empleado)
                .WithMany(emp => emp.OrdenesAtendidas)
                .HasForeignKey(e => e.EmpleadoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ DetalleOrden ============
        modelBuilder.Entity<DetalleOrden>(entity =>
        {
            entity.ToTable("DetallesOrden", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PrecioUnitario).HasPrecision(10, 2);
            entity.Property(e => e.Subtotal).HasPrecision(10, 2);
            entity.Property(e => e.Notas).HasMaxLength(200);

            entity.HasOne(e => e.Orden)
                .WithMany(o => o.Detalles)
                .HasForeignKey(e => e.OrdenId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Producto)
                .WithMany(p => p.DetallesOrden)
                .HasForeignKey(e => e.ProductoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // ============ Reservacion ============
        modelBuilder.Entity<Reservacion>(entity =>
        {
            entity.ToTable("Reservaciones", "restaurante");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Codigo).IsRequired().HasMaxLength(30);
            entity.HasIndex(e => e.Codigo).IsUnique();
            entity.Property(e => e.NombreCliente).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(30);
            entity.Property(e => e.Observaciones).HasMaxLength(500);

            entity.HasOne(e => e.Mesa)
                .WithMany(m => m.Reservaciones)
                .HasForeignKey(e => e.MesaId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private void ConfigureSaaSEntities(ModelBuilder modelBuilder)
    {
        // ============ SubscriptionPlan ============
        modelBuilder.Entity<SubscriptionPlan>(entity =>
        {
            entity.ToTable("SubscriptionPlans", "saas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.Nombre).IsUnique();
            entity.Property(e => e.Descripcion).HasMaxLength(500);
            entity.Property(e => e.PrecioMensual).HasPrecision(10, 2);
            entity.Property(e => e.Tier).HasConversion<string>().HasMaxLength(20);
            entity.Property(e => e.ColorHex).HasMaxLength(10);
            
            entity.Navigation(e => e.Subscriptions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            // Seed data - Planes por defecto
            entity.HasData(
                new { Id = 1, Nombre = "Free", Descripcion = "Plan gratuito con funcionalidades básicas", PrecioMensual = 0m, MaxUsuarios = 3, MaxVehiculos = 5, Tier = PlanTier.Free, Activo = true, ColorHex = "#95a5a6" },
                new { Id = 2, Nombre = "Pro", Descripcion = "Plan profesional para empresas en crecimiento", PrecioMensual = 49m, MaxUsuarios = 15, MaxVehiculos = 30, Tier = PlanTier.Pro, Activo = true, ColorHex = "#6c5ce7" },
                new { Id = 3, Nombre = "Enterprise", Descripcion = "Plan empresarial sin límites", PrecioMensual = 199m, MaxUsuarios = 100, MaxVehiculos = 500, Tier = PlanTier.Enterprise, Activo = true, ColorHex = "#00d4aa" }
            );
        });

        // ============ Tenant ============
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants", "saas");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(200);
            entity.Property(e => e.RUC).IsRequired().HasMaxLength(20);
            entity.HasIndex(e => e.RUC).IsUnique();
            
            entity.Property(e => e.EmailContacto).IsRequired().HasMaxLength(256);
            entity.Property(e => e.Telefono).HasMaxLength(20);
            entity.Property(e => e.Direccion).HasMaxLength(500);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            
            entity.Navigation(e => e.Subscriptions)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
            
            entity.Ignore(e => e.SuscripcionActiva);
            entity.Ignore(e => e.DomainEvents);
            
            // Seed data - Empresas de ejemplo
            entity.HasData(
                new { Id = 1, Nombre = "Transportes Rápidos SAC", RUC = "20123456789", EmailContacto = "contacto@transportesrapidos.com", Telefono = "01-2345678", Activo = true, FechaCreacion = new DateTime(2024, 1, 15), TotalUsuarios = 15, TotalVehiculos = 30, TotalMantenimientos = 45 },
                new { Id = 2, Nombre = "Logística Express EIRL", RUC = "20234567890", EmailContacto = "admin@logisticaexpress.pe", Telefono = "01-3456789", Activo = true, FechaCreacion = new DateTime(2024, 3, 22), TotalUsuarios = 5, TotalVehiculos = 10, TotalMantenimientos = 12 },
                new { Id = 3, Nombre = "Carga Pesada Corp", RUC = "20345678901", EmailContacto = "operaciones@cargapesada.com", Activo = true, FechaCreacion = new DateTime(2024, 6, 10), TotalUsuarios = 3, TotalVehiculos = 5, TotalMantenimientos = 8 },
                new { Id = 4, Nombre = "Distribuidora Lima Norte", RUC = "20456789012", EmailContacto = "ventas@limanorte.com", Telefono = "01-4567890", Activo = false, FechaCreacion = new DateTime(2024, 2, 28), FechaDesactivacion = new DateTime(2024, 11, 15), TotalUsuarios = 8, TotalVehiculos = 15, TotalMantenimientos = 20 },
                new { Id = 5, Nombre = "Mudanzas Perú SRL", RUC = "20567890123", EmailContacto = "info@mudanzasperu.pe", Activo = true, FechaCreacion = new DateTime(2024, 8, 5), TotalUsuarios = 100, TotalVehiculos = 450, TotalMantenimientos = 320 }
            );
        });

        // ============ TenantSubscription ============
        modelBuilder.Entity<TenantSubscription>(entity =>
        {
            entity.ToTable("TenantSubscriptions", "saas");
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Tenant)
                .WithMany(t => t.Subscriptions)
                .HasForeignKey(e => e.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
            
            entity.HasOne(e => e.Plan)
                .WithMany(p => p.Subscriptions)
                .HasForeignKey(e => e.SubscriptionPlanId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.Ignore(e => e.EstaActiva);
            
            // Seed data - Suscripciones de ejemplo
            entity.HasData(
                new { Id = 1, TenantId = 1, SubscriptionPlanId = 2, FechaInicio = new DateTime(2024, 1, 15) }, // Transportes Rápidos -> Pro
                new { Id = 2, TenantId = 2, SubscriptionPlanId = 2, FechaInicio = new DateTime(2024, 3, 22) }, // Logística Express -> Pro
                new { Id = 3, TenantId = 3, SubscriptionPlanId = 1, FechaInicio = new DateTime(2024, 6, 10) }, // Carga Pesada -> Free
                new { Id = 4, TenantId = 4, SubscriptionPlanId = 2, FechaInicio = new DateTime(2024, 2, 28), FechaFin = new DateTime(2024, 11, 15) }, // Distribuidora (inactiva)
                new { Id = 5, TenantId = 5, SubscriptionPlanId = 3, FechaInicio = new DateTime(2024, 8, 5) } // Mudanzas Perú -> Enterprise
            );
        });
    }
}
