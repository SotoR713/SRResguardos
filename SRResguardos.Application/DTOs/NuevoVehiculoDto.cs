using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoVehiculoDto
{
    [Required(ErrorMessage = "Escribe el nombre del equipo.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe la marca.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Marca { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el modelo.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Modelo { get; set; } = string.Empty;

    [Required(ErrorMessage = "Escribe el número de serie.")]
    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string NumeroSerie { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Seguro { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Máximo 50 caracteres.")]
    public string Caracteristica { get; set; } = string.Empty;
}
