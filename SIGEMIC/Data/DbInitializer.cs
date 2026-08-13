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

            bool existeAdmin = await context.Usuarios
                .AnyAsync(u => u.Username == "admin");

            Console.WriteLine($"¿Existe admin?: {existeAdmin}");

            if (existeAdmin)
            {
                return;
            }

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

            var hasher = new PasswordHasher<Usuario>();

            admin.PasswordHash =
                hasher.HashPassword(admin, "Admin123*");

            context.Usuarios.Add(admin);

            await context.SaveChangesAsync();

            Console.WriteLine("Administrador creado correctamente.");
        }
    }
}