using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("compras_detalle")]
    public class Compras_detalle
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_detalle")]
        public int Id_detalle { get; set; }

        [Required]
        [Column("id_compra")]
        public int Id_compra { get; set; }

        [Required]
        [Column("id_producto")]
        public int Id_producto { get; set; }

        [Required]
        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Required]
        [Column("precio", TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }

        [Required]
        [Column("subtotal", TypeName = "decimal(10,2)")]
        public decimal Subtotal { get; set; }
    }
}