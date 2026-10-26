using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("empleados")]
    public class Empleados
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int idempleados { get; set; }

        [Required]
        [StringLength(20)]
        public string Codigo { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string nombres { get; set; } = string.Empty;

        [Required]
        [StringLength(60)]
        public string apellidos { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string direccion { get; set; } = string.Empty;

        [StringLength(45)]
        public string? telefono { get; set; }

        public DateTime? fecha_nacimiento { get; set; }

        [Required]
        [StringLength(20)]
        public string activo { get; set; } = string.Empty;

        [Required]
        public DateTime fecha_ingreso { get; set; }

        [Required]
        public int id_puesto { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Sueldo { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? Bonificacion { get; set; }
    }
}