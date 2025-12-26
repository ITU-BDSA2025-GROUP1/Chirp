namespace Chirp.Core.DTOs;

// DTO used by Razor views. Only primitive/string fields.
public record AuthorDTO(int Id, string Name, string? Email);
