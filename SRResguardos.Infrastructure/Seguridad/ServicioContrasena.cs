using SRResguardos.Application.Interfaces.Seguridad;
using System.Security.Cryptography;

namespace SRResguardos.Infrastructure.Seguridad;

// La contraseña nunca se guarda: se guarda su hash PBKDF2 con una sal aleatoria.
// Formato: PBKDF2$iteraciones$sal$hash   (sal y hash en Base64)
public sealed class ServicioContrasena : IServicioContrasena
{
    private const int Iteraciones = 600_000;
    private const int TamanoSal = 16;
    private const int TamanoHash = 32;

    private readonly string _hashGuardado;

    public ServicioContrasena(string hashGuardado)
    {
        _hashGuardado = hashGuardado;
    }

    public bool EstaConfigurada => !string.IsNullOrWhiteSpace(_hashGuardado);

    public string GenerarHash(string contrasena)
    {
        var sal = RandomNumberGenerator.GetBytes(TamanoSal);
        var hash = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, Iteraciones, HashAlgorithmName.SHA256, TamanoHash);

        return $"PBKDF2${Iteraciones}${Convert.ToBase64String(sal)}${Convert.ToBase64String(hash)}";
    }

    public bool Verificar(string contrasena)
    {
        var partes = _hashGuardado.Split('$');

        if (partes.Length != 4 || partes[0] != "PBKDF2" || !int.TryParse(partes[1], out var iteraciones))
            return false;

        byte[] sal;
        byte[] esperado;

        try
        {
            sal = Convert.FromBase64String(partes[2]);
            esperado = Convert.FromBase64String(partes[3]);
        }
        catch (FormatException)
        {
            return false;
        }

        var calculado = Rfc2898DeriveBytes.Pbkdf2(contrasena, sal, iteraciones, HashAlgorithmName.SHA256, esperado.Length);

        // Comparación en tiempo constante: no revela cuántos bytes coincidieron.
        return CryptographicOperations.FixedTimeEquals(calculado, esperado);
    }
}
