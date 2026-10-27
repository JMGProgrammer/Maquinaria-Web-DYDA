namespace Maquinaria_Web_juan.Models;

// Espejo del EquipmentReadDTO de la API: mismas propiedades para que el JSON se deserialice.
public class Equipment
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Code { get; set; } = string.Empty;

    public string Location { get; set; } = string.Empty;

    public string Status { get; set; } = "Operativo";
}
