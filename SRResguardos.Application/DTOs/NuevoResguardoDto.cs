using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

// Datos que captura la pantalla de asignación.
// IValidatableObject permite reglas que involucran a más de un campo.
public class NuevoResguardoDto : IValidatableObject
{
    [Range(1, int.MaxValue, ErrorMessage = "Elige un colaborador activo.")]
    public int ColaboradorId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Elige quién entrega.")]
    public int EntregaId { get; set; }

    [Required(ErrorMessage = "Elige un bien disponible.")]
    [StringLength(20)]
    public string IdentificadorBien { get; set; } = string.Empty;

    public DateOnly Fecha { get; set; }

    [Required(ErrorMessage = "Las notas son obligatorias.")]
    [StringLength(500, ErrorMessage = "Las notas no pueden pasar de 500 caracteres.")]
    public string Notas { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ColaboradorId > 0 && ColaboradorId == EntregaId)
        {
            yield return new ValidationResult(
                "Quien entrega debe ser una persona distinta del colaborador.",
                [nameof(EntregaId)]);
        }
    }
}
