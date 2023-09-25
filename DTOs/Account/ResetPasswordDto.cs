using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class ResetPasswordDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$", ErrorMessage = "Adresse email invalide")]
        public string Email { get; set; }

        [Required]
        [StringLength(15, MinimumLength = 8, ErrorMessage = "Le nouveau mot de passe doit contenir entre {2} et {1} caractères")]
        public string NewPassword { get; set; }
    }
}
