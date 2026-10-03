using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoOtroBienDto
{
    [Required(ErrorMessage = "Escribe la descripción.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica1 { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica2 { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica3 { get; set; } = string.Empty;
}
