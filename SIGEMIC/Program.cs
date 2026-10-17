using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using SIGEMIC.Data;
using SIGEMIC.Services;

var builder = WebApplication.CreateBuilder(args);



const string esquemaAutenticacion =
    "SIGEMICCookie";



builder.Services.AddControllersWithViews();


var connectionString =
    builder.Configuration
        .GetConnectionString("DefaultConnection");


if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'DefaultConnection'."
    );
}


builder.Services.AddDbContext<ApplicationDbContext>(
    options =>
        options.UseMySql(
            connectionString,
            ServerVersion.AutoDetect(connectionString)
        )
);


builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            esquemaAutenticacion;

        options.DefaultChallengeScheme =
            esquemaAutenticacion;

        options.DefaultSignInScheme =
            esquemaAutenticacion;

        options.DefaultSignOutScheme =
            esquemaAutenticacion;
    })
    .AddCookie(
        esquemaAutenticacion,
        options =>
        {


            options.Cookie.Name =
                "SIGEMICCookie";

            options.LoginPath =
                "/Account/Login";


            options.AccessDeniedPath =
                "/Account/AccessDenied";

            options.ExpireTimeSpan =
                TimeSpan.FromHours(8);


            options.SlidingExpiration =
                true;


            options.Cookie.HttpOnly =
                true;


            options.Cookie.SameSite =
                SameSiteMode.Lax;
        }
    );


builder.Services.AddAuthorization();

builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<PermisoService>();



var app =
    builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler(
        "/Home/Error"
    );


    app.UseHsts();
}

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern:
        "{controller=Home}/{action=Index}/{id?}"
);


using (
    var scope =
        app.Services.CreateScope()
)
{
    var services =
        scope.ServiceProvider;


    try
    {
        var context =
            services
                .GetRequiredService<
                    ApplicationDbContext
                >();


        await DbInitializer
            .InicializarAsync(
                context
            );
    }
    catch (Exception ex)
    {
        var logger =
            services
                .GetRequiredService<
                    ILogger<Program>
                >();


        logger.LogError(
            ex,
            "Ocurrió un error al inicializar la base de datos de SIGEMIC."
        );


        throw;
    }
}


app.Run();