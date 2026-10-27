using System.Net.Http;
using System.Net.Http.Json;
using Maquinaria_Web_juan.DTOs;
using Maquinaria_Web_juan.Models;

namespace Maquinaria_Web_juan.Services;

// Cliente tipado de la API: las páginas inyectan esta clase y llaman métodos,
// sin conocer URLs ni detalles de HTTP. La BaseAddress se fija en Program.cs.
public class EquipmentApiClient
{
    private readonly HttpClient _http;

    public EquipmentApiClient(HttpClient http)
    {
        _http = http;
    }

    // GET /api/equipment → lista completa (si la API cae, sube HttpRequestException).
    public async Task<List<Equipment>> GetAllAsync()
    {
        return await _http.GetFromJsonAsync<List<Equipment>>("equipment")
               ?? new List<Equipment>();
    }

    // GET /api/equipment/{id} → el equipo, o null si responde 404.
    // GetAsync crudo para poder mirar el status code antes de leer el body.
    public async Task<Equipment?> GetByIdAsync(string id)
    {
        var response = await _http.GetAsync($"equipment/{id}");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<Equipment>();
    }

    // POST /api/equipment → alta. 400 (regla inválida) → Exception con el mensaje de la API.
    public async Task<Equipment> CreateAsync(EquipmentCreateDTO input)
    {
        var response = await _http.PostAsJsonAsync("equipment", input);

        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error {(int)response.StatusCode}: {detalle}");
        }

        return await response.Content.ReadFromJsonAsync<Equipment>()!;
    }

    // PUT /api/equipment/{id} → actualización. Mismo manejo de errores que POST.
    public async Task<Equipment> UpdateAsync(string id, EquipmentUpdateDTO input)
    {
        var response = await _http.PutAsJsonAsync($"equipment/{id}", input);

        if (!response.IsSuccessStatusCode)
        {
            var detalle = await response.Content.ReadAsStringAsync();
            throw new Exception($"Error {(int)response.StatusCode}: {detalle}");
        }

        return await response.Content.ReadFromJsonAsync<Equipment>()!;
    }

    // DELETE /api/equipment/{id} → true si eliminó (200), false si no (404).
    public async Task<bool> DeleteAsync(string id)
    {
        var response = await _http.DeleteAsync($"equipment/{id}");

        return response.IsSuccessStatusCode;
    }
}
