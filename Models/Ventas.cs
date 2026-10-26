
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("ventas")]
    public class Ventas
    {
        [Key]
        [Column("id_venta")]
        public int Id_venta { get; set; }

        [Column("fecha")]
        public DateTime Fecha { get; set; }

        [Column("id_cliente")]
        public uint Id_cliente { get; set; }

        [Column("total", TypeName = "decimal(10,2)")]
        public decimal Total { get; set; }
    }
}
