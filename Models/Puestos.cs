using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("puestos")]
    public class Puestos
    {
        [Key]
        [Column("id_puesto")]
        public int Id_puesto { get; set; }

        [Required]
        [Column("nombre_puesto")]
        public string Nombre_puesto { get; set; } = string.Empty;

        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}