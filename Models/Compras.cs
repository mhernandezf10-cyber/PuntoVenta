using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("compras")]
    public class Compras
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_compra")]
        public int Id_compra { get; set; }

        [Required]
        [Column("id_proveedor")]
        public int Id_proveedor { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; } = DateTime.Now;

        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; } = 0.00m;
    }
}