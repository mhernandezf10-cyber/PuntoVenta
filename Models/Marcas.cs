using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("marcas")]
    public class Marcas
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_marca")]
        public int Id_marca { get; set; }

        [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
        [StringLength(60, ErrorMessage = "El nombre no puede superar los 60 caracteres.")]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "La descripción no puede superar los 150 caracteres.")]
        [Column("descripcion")]
        public string? Descripcion { get; set; }
    }
}
