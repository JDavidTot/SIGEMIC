using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Models;

namespace SIGEMIC.Data
{
    public static class DbInitializer
    {
        public static async Task InicializarAsync(ApplicationDbContext context)
        {
            Console.WriteLine("Entrando a DbInitializer...");
            var hasher = new PasswordHasher<Usuario>();
            bool existeAdmin = await context.Usuarios.AnyAsync(u => u.Username == "admin");
            Console.WriteLine($"¿Existe admin?: {existeAdmin}");

            if (!existeAdmin)
            {
                var admin = new Usuario
                {
                    Nombres = "Administrador",
                    Apellidos = "SIGEMIC",
                    Username = "admin",
                    Rol = "ADMINISTRADOR",
                    Estado = "ACTIVO",
                    IntentosFallidos = 0,
                    FechaCreacion = DateTime.Now
                };

                admin.PasswordHash = hasher.HashPassword(admin,"Admin123*");
                context.Usuarios.Add(admin);
                await context.SaveChangesAsync();
                Console.WriteLine("Administrador creado correctamente.");
            }


            bool existeCajero = await context.Usuarios.AnyAsync(u => u.Username == "cajero01");
            Console.WriteLine($"¿Existe cajero?: {existeCajero}");
            if (!existeCajero)
            {
                var cajero = new Usuario
                {
                    Nombres = "Cajero",
                    Apellidos = "SIGEMIC",
                    Username = "cajero01",
                    Rol = "CAJERO",
                    Estado = "ACTIVO",
                    IntentosFallidos = 0,
                    FechaCreacion = DateTime.Now
                };

                cajero.PasswordHash = hasher.HashPassword(cajero,"Cajero123*");
                context.Usuarios.Add(cajero);
                await context.SaveChangesAsync();
                Console.WriteLine("Cajero creado correctamente.");
            }

            Console.WriteLine("DbInitializer finalizado."
            );
        }
    }
}