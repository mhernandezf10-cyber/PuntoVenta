
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ApiEmpresa.Models
{
    [Table("usuarios")]
    public class Usuarios
    {
        [Key]
        [Column("id_usuario")]
        public int Id_usuario { get; set; }

        [Required]
        [StringLength(50)]
        [Column("nombre_usuario")]
        public string Nombre_usuario { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("password_hash")]
        public string Password_hash { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        [Column("nombre_completo")]
        public string Nombre_completo { get; set; } = string.Empty;

        [Column("activo")]
        public bool Activo { get; set; } = true;
    }
}
