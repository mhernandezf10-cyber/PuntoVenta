
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("ventas_detalle")]
    public class Ventas_detalle
    {
        [Key]
        [Column("id_detalle")]
        public int Id_detalle { get; set; }

        [Column("id_venta")]
        public int Id_venta { get; set; }

        [Column("id_producto")]
        public int Id_producto { get; set; }

        [Column("cantidad")]
        public int Cantidad { get; set; }

        [Column("precio", TypeName = "decimal(10,2)")]
        public decimal Precio { get; set; }
    }
}
