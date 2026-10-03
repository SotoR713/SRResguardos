using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoLaptopDto
{
    [Required(ErrorMessage = "Escribe la marca.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el modelo.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el número de serie.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Serie { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Ram { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string DiscoDuro { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Procesador { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica { get; set; } = string.Empty;
}
