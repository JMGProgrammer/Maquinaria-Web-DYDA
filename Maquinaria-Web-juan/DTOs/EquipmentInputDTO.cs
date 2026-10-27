using System.ComponentModel.DataAnnotations;

namespace Maquinaria_Web_juan.DTOs;

// Campos que el form envía a la API (sin Id: lo genera la API en el POST).
// Las mismas validaciones dejan al EditForm validar en el navegador antes del HTTP.
public abstract class EquipmentInputDTO
{
    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required]
    public string Location { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Operativo";
}
