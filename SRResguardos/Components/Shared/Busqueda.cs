using System.Globalization;

namespace SRResguardos.Web.Components.Shared;

// Regla de búsqueda compartida por todas las listas.
public static class Busqueda
{
    // Ignora mayúsculas y acentos: "jose" encuentra a "José".
    // La base distingue acentos (CI_AS); el buscador no, para que sea más amable.
    private static readonly CompareInfo Comparador = CultureInfo.GetCultureInfo("es-MX").CompareInfo;
    private const CompareOptions SinAcentosNiMayusculas = CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace;

    // Verdadero si cada palabra escrita aparece en alguno de los textos, en cualquier orden:
    // "lenovo e16" encuentra la Lenovo Thinkpad E16.
    public static bool Coincide(string busqueda, params string?[] textos)
    {
        var palabras = busqueda.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (palabras.Length == 0)
            return true;

        var todo = string.Join(' ', textos.Where(t => !string.IsNullOrEmpty(t)));

        return palabras.All(p => Comparador.IndexOf(todo, p, SinAcentosNiMayusculas) >= 0);
    }
}
