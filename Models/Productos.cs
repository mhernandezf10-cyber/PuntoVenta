using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("productos")]
    public class Productos
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_producto")]
        public int Id_producto { get; set; }

        [Required(ErrorMessage = "El código es obligatorio.")]
        [StringLength(30)]
        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(200)]
        [Column("descripcion")]
        public string? Descripcion { get; set; }

        [Column("precio")]
        public decimal Precio { get; set; }

        [Column("existencia")]
        public int Existencia { get; set; }

        [Column("id_marca")]
        public int? Id_marca { get; set; }
    }
}