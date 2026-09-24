using System.ComponentModel.DataAnnotations;

namespace SRResguardos.Application.DTOs;

public class NuevoEmpleadoDto
{
    [Required(ErrorMessage = "Escribe el nombre.")]
    [StringLength(50, ErrorMessage = "El nombre no puede pasar de 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Elige un puesto.")]
    public int PuestoId { get; set; }
}