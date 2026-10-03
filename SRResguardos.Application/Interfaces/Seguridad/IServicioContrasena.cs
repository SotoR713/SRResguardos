namespace SRResguardos.Application.Interfaces.Seguridad;

// Contrato para la contraseña única de acceso.
// La pantalla de login no sabe qué algoritmo se usa; eso vive en Infrastructure.
public interface IServicioContrasena
{
    bool EstaConfigurada { get; }
    string GenerarHash(string contrasena);
    bool Verificar(string contrasena);
}
