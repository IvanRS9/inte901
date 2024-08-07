﻿using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Server.Models.Usuario.Server.Models.Usuario;

namespace Server.Models
{
    public class Proveedor
    {
        [Key]
        public int? Id { get; set; }
        [Required]
        [MaxLength(200)]
        public string? NombreEmpresa { get; set; }
        [Required]
        [MaxLength(200)]
        public string? DireccionEmpresa { get; set; }
        [Required]
        [MaxLength(13)]
        public string? TelefonoEmpresa { get; set; }
        [Required]
        [MaxLength(80)]
        public string? NombreEncargado { get; set; }
        [Required]
        public int? Estatus { get; set; } = 1;

        public DateTime? CreatedAt { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public DateTime? DeletedAt { get; set; }

        public int? IdUsuario { get; set; }
        [ForeignKey("IdUsuario")]

        public User? Usuario { get; set; }

        public ICollection<MateriaPrimaProveedor>? MateriaPrimaProveedores { get; set; }

    }
}
