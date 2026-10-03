using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using SRResguardos.Application.Interfaces.Persistence;
using SRResguardos.Application.Interfaces.Seguridad;
using SRResguardos.Infrastructure.Persistence.Repositories;
using SRResguardos.Infrastructure.Seguridad;
using SRResguardos.Web.Components;

namespace SRResguardos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddRazorComponents()
                .AddInteractiveServerComponents();

            // Acceso con una sola contraseña: al entrar se emite una cookie de sesión.
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(opciones =>
                {
                    opciones.LoginPath = "/login";
                    opciones.ExpireTimeSpan = TimeSpan.FromHours(8);
                    opciones.SlidingExpiration = true;
                });
            builder.Services.AddAuthorization();
            builder.Services.AddCascadingAuthenticationState();

            // El hash de la contraseña viene de la configuración (secretos de usuario en desarrollo),
            // nunca del appsettings.json que se sube al repositorio.
            var hashContrasena = builder.Configuration["Acceso:HashContrasena"] ?? string.Empty;
            builder.Services.AddSingleton<IServicioContrasena>(new ServicioContrasena(hashContrasena));

            var cadena = builder.Configuration.GetConnectionString("SRResguardosBD")
                ?? throw new InvalidOperationException("Falta la cadena de conexión SRResguardosBD.");

            builder.Services.AddScoped<IResguardoRepository>(_ => new ResguardoRepository(cadena));
            builder.Services.AddScoped<ICatalogoRepository>(_ => new CatalogoRepository(cadena));
            builder.Services.AddScoped<IEmpleadoRepository>(_ => new EmpleadoRepository(cadena));
            builder.Services.AddScoped<IBienRepository>(_ => new BienRepository(cadena));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseAntiforgery();

            app.MapStaticAssets();
            app.MapRazorComponents<App>()
                .AddInteractiveServerRenderMode();

            app.MapPost("/logout", async (HttpContext contexto) =>
            {
                await contexto.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return Results.Redirect("/login");
            });

            app.Run();
        }
    }
}
