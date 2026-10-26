namespace ApiEmpresa.Models;
using System.ComponentModel.DataAnnotations;

public class Clientes

{
    [Key]
    public Int32 Id_clientes { get; 
    set; }

    public string? Cui { get; set; }
    public string? NIT { get; set; }
    public string? Nombres { get; set; }
    public string? Apellidos { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public DateTime? Fecha_nacimiento { get; set; }
}