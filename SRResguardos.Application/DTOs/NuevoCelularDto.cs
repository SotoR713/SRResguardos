using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoCelularDto
{
    [Required(ErrorMessage = "Escribe la marca.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el modelo.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el IMEI.")]
    [RegularExpression(@"^\d{15}$", ErrorMessage = "El IMEI son exactamente 15 dígitos.")]
    public string Imei { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Numero { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Ram { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Memoria { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica { get; set; } = string.Empty;
}
