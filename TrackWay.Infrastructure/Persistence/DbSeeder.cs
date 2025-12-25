using TrackWay.Domain.Entities.Auth;
using TrackWay.Domain.Entities.Fleet;
using TrackWay.Domain.Entities.Restaurante;
using TrackWay.Domain.Entities.SaaS;
using TrackWay.Domain.Enums;
using TrackWay.Application.Auth.Handlers;

namespace TrackWay.Infrastructure.Persistence;

/// <summary>
/// Seeder para poblar la base de datos con datos de prueba
/// </summary>
public static class DbSeeder
{
    public static async Task SeedAsync(TrackWayDbContext context, IPasswordHasher passwordHasher)
    {
        await SeedAuthDataAsync(context, passwordHasher);
        await SeedFleetDataAsync(context);
        await SeedRestauranteDataAsync(context);
        await SeedSaaSDataAsync(context);
    }

    private static async Task SeedAuthDataAsync(TrackWayDbContext context, IPasswordHasher passwordHasher)
    {
        // Seed Roles con permisos específicos
        if (!context.Roles.Any())
        {
            var roles = new[]
            {
                // SuperAdmin - Acceso total al sistema SaaS (gestión de tenants)
                Role.Create("SuperAdmin", "Super Administrador con acceso total al SaaS", 
                    "[\"dashboard:read\",\"vehicles:read\",\"vehicles:write\",\"vehicles:delete\",\"drivers:read\",\"drivers:write\",\"drivers:delete\",\"maintenance:read\",\"maintenance:write\",\"maintenance:delete\",\"alerts:read\",\"alerts:write\",\"reports:read\",\"reports:export\",\"users:read\",\"users:write\",\"users:delete\",\"settings:read\",\"settings:write\",\"superadmin:read\",\"superadmin:write\",\"tenants:read\",\"tenants:write\",\"tenants:delete\"]"),
                
                // Admin - Acceso total a una empresa
                Role.Create("Admin", "Administrador del sistema con acceso completo", 
                    "[\"dashboard:read\",\"vehicles:read\",\"vehicles:write\",\"vehicles:delete\",\"drivers:read\",\"drivers:write\",\"drivers:delete\",\"maintenance:read\",\"maintenance:write\",\"maintenance:delete\",\"alerts:read\",\"alerts:write\",\"reports:read\",\"reports:export\",\"users:read\",\"users:write\",\"users:delete\",\"settings:read\",\"settings:write\"]"),
                
                // Gerente - Gestión sin eliminar usuarios
                Role.Create("Gerente", "Gerente con acceso a reportes y gestión", 
                    "[\"dashboard:read\",\"vehicles:read\",\"vehicles:write\",\"drivers:read\",\"drivers:write\",\"maintenance:read\",\"maintenance:write\",\"alerts:read\",\"alerts:write\",\"reports:read\",\"reports:export\",\"users:read\"]"),
                
                // Asistente - Solo lectura y algunas escrituras
                Role.Create("Asistente", "Asistente con acceso básico", 
                    "[\"dashboard:read\",\"vehicles:read\",\"drivers:read\",\"maintenance:read\",\"alerts:read\",\"reports:read\"]"),
                
                // Chofer - Acceso limitado a su información
                Role.Create("Chofer", "Conductor con acceso a sus rutas y vehículo asignado", 
                    "[\"dashboard:read\",\"vehicles:read\",\"maintenance:read\",\"alerts:read\"]"),
            };
            context.Roles.AddRange(roles);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Roles creados: SuperAdmin, Admin, Gerente, Asistente, Chofer");
        }
        else
        {
            // Verificar si existe el rol SuperAdmin, si no, agregarlo
            if (!context.Roles.Any(r => r.Nombre == "SuperAdmin"))
            {
                var superAdminRole = Role.Create("SuperAdmin", "Super Administrador con acceso total al SaaS", 
                    "[\"dashboard:read\",\"vehicles:read\",\"vehicles:write\",\"vehicles:delete\",\"drivers:read\",\"drivers:write\",\"drivers:delete\",\"maintenance:read\",\"maintenance:write\",\"maintenance:delete\",\"alerts:read\",\"alerts:write\",\"reports:read\",\"reports:export\",\"users:read\",\"users:write\",\"users:delete\",\"settings:read\",\"settings:write\",\"superadmin:read\",\"superadmin:write\",\"tenants:read\",\"tenants:write\",\"tenants:delete\"]");
                context.Roles.Add(superAdminRole);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Rol SuperAdmin agregado");
            }
            
            // Verificar si existe el rol Chofer, si no, agregarlo
            if (!context.Roles.Any(r => r.Nombre == "Chofer"))
            {
                var choferRole = Role.Create("Chofer", "Conductor con acceso a sus rutas y vehículo asignado", 
                    "[\"dashboard:read\",\"vehicles:read\",\"maintenance:read\",\"alerts:read\"]");
                context.Roles.Add(choferRole);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Rol Chofer agregado");
            }
        }

        // Seed Users
        if (!context.Users.Any())
        {
            var superAdminRole = context.Roles.FirstOrDefault(r => r.Nombre == "SuperAdmin");
            var adminRole = context.Roles.FirstOrDefault(r => r.Nombre == "Admin");
            var gerenteRole = context.Roles.FirstOrDefault(r => r.Nombre == "Gerente");
            var asistenteRole = context.Roles.FirstOrDefault(r => r.Nombre == "Asistente");
            var choferRole = context.Roles.FirstOrDefault(r => r.Nombre == "Chofer");

            var users = new List<User>();

            if (superAdminRole != null)
            {
                users.Add(User.Create("superadmin@trackway.com", "Super Administrador", passwordHasher.HashPassword("SuperAdmin123!"), superAdminRole.Id));
            }

            if (adminRole != null && gerenteRole != null && asistenteRole != null)
            {
                users.Add(User.Create("admin@trackway.com", "Administrador Sistema", passwordHasher.HashPassword("Admin123!"), adminRole.Id));
                users.Add(User.Create("gerente@trackway.com", "Gerente General", passwordHasher.HashPassword("Gerente123!"), gerenteRole.Id));
                users.Add(User.Create("asistente@trackway.com", "Asistente Operaciones", passwordHasher.HashPassword("Asistente123!"), asistenteRole.Id));
            }

            if (choferRole != null)
            {
                users.Add(User.Create("chofer@trackway.com", "Carlos García (Chofer)", passwordHasher.HashPassword("Chofer123!"), choferRole.Id));
            }

            if (users.Any())
            {
                context.Users.AddRange(users);
                await context.SaveChangesAsync();
                Console.WriteLine("✅ Usuarios de prueba creados:");
                Console.WriteLine("   - superadmin@trackway.com / SuperAdmin123! (SuperAdmin)");
                Console.WriteLine("   - admin@trackway.com / Admin123! (Admin)");
                Console.WriteLine("   - gerente@trackway.com / Gerente123! (Gerente)");
                Console.WriteLine("   - asistente@trackway.com / Asistente123! (Asistente)");
                Console.WriteLine("   - chofer@trackway.com / Chofer123! (Chofer)");
            }
        }
        else
        {
            // Verificar si existe usuario superadmin
            if (!context.Users.Any(u => u.Email == "superadmin@trackway.com"))
            {
                var superAdminRole = context.Roles.FirstOrDefault(r => r.Nombre == "SuperAdmin");
                if (superAdminRole != null)
                {
                    var superAdminUser = User.Create("superadmin@trackway.com", "Super Administrador", 
                        passwordHasher.HashPassword("SuperAdmin123!"), superAdminRole.Id);
                    context.Users.Add(superAdminUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuario SuperAdmin agregado: superadmin@trackway.com / SuperAdmin123!");
                }
            }
            
            // Verificar si existe usuario chofer
            if (!context.Users.Any(u => u.Email == "chofer@trackway.com"))
            {
                var choferRole = context.Roles.FirstOrDefault(r => r.Nombre == "Chofer");
                if (choferRole != null)
                {
                    var choferUser = User.Create("chofer@trackway.com", "Carlos García (Chofer)", 
                        passwordHasher.HashPassword("Chofer123!"), choferRole.Id);
                    context.Users.Add(choferUser);
                    await context.SaveChangesAsync();
                    Console.WriteLine("✅ Usuario chofer agregado: chofer@trackway.com / Chofer123!");
                }
            }
            else
            {
                Console.WriteLine("ℹ️ Usuarios ya existen en la base de datos");
            }
        }
    }

    private static async Task SeedFleetDataAsync(TrackWayDbContext context)
    {
        // Seed Drivers
        if (!context.Drivers.Any())
        {
            var drivers = new[]
            {
                Driver.Create("Carlos", "García López", "12345678", "A-12345", CategoriaLicencia.AIIIb, DateTime.UtcNow.AddYears(2)),
                Driver.Create("María", "Rodríguez Pérez", "23456789", "B-23456", CategoriaLicencia.AIIa, DateTime.UtcNow.AddMonths(6)),
                Driver.Create("José", "Martínez Silva", "34567890", "C-34567", CategoriaLicencia.AIIIa, DateTime.UtcNow.AddDays(15)), // Próximo a vencer
            };

            // Asignar IDs y propiedades para seed (EF asignará los IDs reales)
            context.Drivers.AddRange(drivers);
            await context.SaveChangesAsync();
        }

        // Seed Vehicles
        if (!context.Vehicles.Any())
        {
            var vehicles = new[]
            {
                Vehicle.Create("ABC-123", "1HGBH41JXMN109186", "Toyota", "Hilux", 2022, TipoCombustible.Diesel, 45000),
                Vehicle.Create("DEF-456", "2GTEK19B571123456", "Nissan", "Frontier", 2021, TipoCombustible.Diesel, 62000),
                Vehicle.Create("GHI-789", "3VWDX7AJ8DM012345", "Hyundai", "H1", 2023, TipoCombustible.Gasolina, 18000),
                Vehicle.Create("JKL-012", "5YFBURHE4LP012345", "Kia", "Bongo", 2020, TipoCombustible.Diesel, 95000),
            };

            context.Vehicles.AddRange(vehicles);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedRestauranteDataAsync(TrackWayDbContext context)
    {
        // Seed Categorías
        if (!context.Categorias.Any())
        {
            var categorias = new[]
            {
                new Categoria { Nombre = "Entradas", Descripcion = "Platos de entrada", Icono = "🥗", Orden = 1 },
                new Categoria { Nombre = "Platos Principales", Descripcion = "Platos de fondo", Icono = "🍽️", Orden = 2 },
                new Categoria { Nombre = "Postres", Descripcion = "Dulces y postres", Icono = "🍰", Orden = 3 },
                new Categoria { Nombre = "Bebidas", Descripcion = "Bebidas y refrescos", Icono = "🥤", Orden = 4 },
                new Categoria { Nombre = "Sopas", Descripcion = "Sopas y caldos", Icono = "🍲", Orden = 5 },
                new Categoria { Nombre = "Especialidades", Descripcion = "Platos especiales", Icono = "⭐", Orden = 6 },
            };
            context.Categorias.AddRange(categorias);
            await context.SaveChangesAsync();
        }

        // Seed Productos
        if (!context.Productos.Any())
        {
            var categorias = context.Categorias.ToList();
            var productos = new[]
            {
                new Producto { Codigo = "ENT001", Nombre = "Ceviche Clásico", Descripcion = "Pescado fresco marinado en limón", Precio = 35.00m, CategoriaId = categorias[0].Id, Disponible = true, TiempoPreparacionMinutos = 10 },
                new Producto { Codigo = "ENT002", Nombre = "Causa Limeña", Descripcion = "Papa amarilla con pollo", Precio = 25.00m, CategoriaId = categorias[0].Id, Disponible = true, TiempoPreparacionMinutos = 15 },
                new Producto { Codigo = "PRI001", Nombre = "Lomo Saltado", Descripcion = "Carne salteada con papas fritas", Precio = 38.00m, CategoriaId = categorias[1].Id, Disponible = true, TiempoPreparacionMinutos = 20 },
                new Producto { Codigo = "PRI002", Nombre = "Arroz con Pollo", Descripcion = "Clásico peruano", Precio = 28.00m, CategoriaId = categorias[1].Id, Disponible = true, TiempoPreparacionMinutos = 25 },
                new Producto { Codigo = "POS001", Nombre = "Suspiro Limeño", Descripcion = "Postre tradicional", Precio = 15.00m, CategoriaId = categorias[2].Id, Disponible = true, TiempoPreparacionMinutos = 5 },
                new Producto { Codigo = "BEB001", Nombre = "Chicha Morada", Descripcion = "Bebida de maíz morado", Precio = 8.00m, CategoriaId = categorias[3].Id, Disponible = true, TiempoPreparacionMinutos = 2 },
                new Producto { Codigo = "BEB002", Nombre = "Pisco Sour", Descripcion = "Cóctel peruano", Precio = 22.00m, CategoriaId = categorias[3].Id, Disponible = true, TiempoPreparacionMinutos = 5 },
                new Producto { Codigo = "SOP001", Nombre = "Sopa Criolla", Descripcion = "Sopa con fideos y carne", Precio = 18.00m, CategoriaId = categorias[4].Id, Disponible = true, TiempoPreparacionMinutos = 15 },
            };
            context.Productos.AddRange(productos);
            await context.SaveChangesAsync();
        }

        // Seed Mesas
        if (!context.Mesas.Any())
        {
            var mesas = new[]
            {
                new Mesa { Numero = "M01", Capacidad = 4, Ubicacion = "Interior", Estado = "Disponible" },
                new Mesa { Numero = "M02", Capacidad = 4, Ubicacion = "Interior", Estado = "Disponible" },
                new Mesa { Numero = "M03", Capacidad = 6, Ubicacion = "Interior", Estado = "Ocupada" },
                new Mesa { Numero = "M04", Capacidad = 2, Ubicacion = "Interior", Estado = "Disponible" },
                new Mesa { Numero = "T01", Capacidad = 4, Ubicacion = "Terraza", Estado = "Disponible" },
                new Mesa { Numero = "T02", Capacidad = 6, Ubicacion = "Terraza", Estado = "Reservada" },
                new Mesa { Numero = "T03", Capacidad = 8, Ubicacion = "Terraza", Estado = "Disponible" },
                new Mesa { Numero = "V01", Capacidad = 8, Ubicacion = "VIP", Estado = "Disponible" },
                new Mesa { Numero = "V02", Capacidad = 10, Ubicacion = "VIP", Estado = "Ocupada" },
                new Mesa { Numero = "B01", Capacidad = 4, Ubicacion = "Interior", Estado = "Disponible" },
            };
            context.Mesas.AddRange(mesas);
            await context.SaveChangesAsync();
        }

        // Seed Empleados
        if (!context.Empleados.Any())
        {
            var empleados = new[]
            {
                new Empleado { Codigo = "EMP001", Nombres = "Ana", Apellidos = "Torres", Documento = "45678901", Cargo = "Mesero", Telefono = "987654321", Email = "ana@trackway.com", SalarioBase = 1500, Estado = "Activo", FechaContratacion = DateTime.UtcNow.AddMonths(-6) },
                new Empleado { Codigo = "EMP002", Nombres = "Luis", Apellidos = "Vargas", Documento = "56789012", Cargo = "Mesero", Telefono = "987654322", Email = "luis@trackway.com", SalarioBase = 1500, Estado = "Activo", FechaContratacion = DateTime.UtcNow.AddMonths(-12) },
                new Empleado { Codigo = "EMP003", Nombres = "Pedro", Apellidos = "Chef", Documento = "67890123", Cargo = "Cocinero", Telefono = "987654323", Email = "pedro@trackway.com", SalarioBase = 2500, Estado = "Activo", FechaContratacion = DateTime.UtcNow.AddYears(-2) },
                new Empleado { Codigo = "EMP004", Nombres = "Rosa", Apellidos = "Caja", Documento = "78901234", Cargo = "Cajero", Telefono = "987654324", Email = "rosa@trackway.com", SalarioBase = 1800, Estado = "Activo", FechaContratacion = DateTime.UtcNow.AddMonths(-3) },
                new Empleado { Codigo = "EMP005", Nombres = "Juan", Apellidos = "Admin", Documento = "89012345", Cargo = "Administrador", Telefono = "987654325", Email = "juan@trackway.com", SalarioBase = 3500, Estado = "Activo", FechaContratacion = DateTime.UtcNow.AddYears(-3) },
            };
            context.Empleados.AddRange(empleados);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedSaaSDataAsync(TrackWayDbContext context)
    {
        // Seed Subscription Plans
        if (!context.SubscriptionPlans.Any())
        {
            var plans = new[]
            {
                SubscriptionPlan.Create("Free", "Plan gratuito con funciones básicas", 0, 5, 5, PlanTier.Free, "#4CAF50"),
                SubscriptionPlan.Create("Pro", "Plan profesional para empresas medianas", 49, 30, 50, PlanTier.Pro, "#2196F3"),
                SubscriptionPlan.Create("Enterprise", "Plan empresarial con todas las funciones", 199, 100, 500, PlanTier.Enterprise, "#FF9800"),
            };
            context.SubscriptionPlans.AddRange(plans);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Planes de suscripción creados: Free, Pro, Enterprise");
        }

        // Seed Tenants
        if (!context.Tenants.Any())
        {
            var freePlan = context.SubscriptionPlans.First(p => p.Nombre == "Free");
            var proPlan = context.SubscriptionPlans.First(p => p.Nombre == "Pro");
            var enterprisePlan = context.SubscriptionPlans.First(p => p.Nombre == "Enterprise");

            var tenants = new[]
            {
                Tenant.Create("Mudanzas Perú SRL", "20567891023", "info@mudanzasperu.pe", "01-555-1234", "Av. Principal 123, Lima"),
                Tenant.Create("Carga Pesada Corp", "20345678901", "operaciones@cargapesada.com", "01-555-5678", "Jr. Industrial 456, Callao"),
                Tenant.Create("Logística Express EIRL", "20234567890", "admin@logisticaexpress.pe", "01-555-9012", "Calle Comercio 789, Miraflores"),
                Tenant.Create("Distribuidora Lima Norte", "20123456789", "ventas@dislima.com", "01-555-3456", "Av. Panamericana Norte 1500"),
                Tenant.Create("TransCargo SAC", "20678901234", "contacto@transcargo.pe", "01-555-7890", "Av. Colonial 2000, Lima"),
            };

            // Configurar datos simulados de actividad
            tenants[0].SimularActividad(100, 450, 320);
            tenants[1].SimularActividad(3, 5, 8);
            tenants[2].SimularActividad(15, 10, 12);
            tenants[3].SimularActividad(8, 25, 45);
            tenants[4].SimularActividad(5, 20, 20);

            context.Tenants.AddRange(tenants);
            await context.SaveChangesAsync();

            // Crear suscripciones
            var subscriptions = new[]
            {
                TenantSubscription.Create(tenants[0].Id, enterprisePlan.Id),
                TenantSubscription.Create(tenants[1].Id, freePlan.Id),
                TenantSubscription.Create(tenants[2].Id, proPlan.Id),
                TenantSubscription.Create(tenants[3].Id, proPlan.Id),
                TenantSubscription.Create(tenants[4].Id, freePlan.Id),
            };

            context.TenantSubscriptions.AddRange(subscriptions);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Empresas de prueba creadas: 5 tenants con suscripciones");
        }
    }
}
