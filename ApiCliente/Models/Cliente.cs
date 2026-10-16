using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiCliente.Models
{
    public class Cliente
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id_cliente { get; set; }

        [Required]
        [MaxLength(13)]
        public string CUI { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string NIT { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Nombres { get; set; } = string.Empty;

        [Required]
        [MaxLength(60)]
        public string Apellidos { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Direccion { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [Required]
        public DateTime Fecha_Nacimiento { get; set; }
    }
}