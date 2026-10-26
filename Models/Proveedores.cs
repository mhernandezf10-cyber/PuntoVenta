using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("proveedores")]
    public class Proveedores
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Column("id_proveedor")]
        public int Id_proveedor { get; set; }

        [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
        [StringLength(100)]
        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [StringLength(20)]
        [Column("nit")]
        public string? Nit { get; set; }

        [StringLength(150)]
        [Column("direccion")]
        public string? Direccion { get; set; }

        [StringLength(20)]
        [Column("telefono")]
        public string? Telefono { get; set; }

        [StringLength(100)]
        [EmailAddress(ErrorMessage = "Ingrese un correo válido")]
        [Column("correo")]
        public string? Correo { get; set; }
    }
}