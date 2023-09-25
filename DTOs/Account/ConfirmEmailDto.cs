using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$", ErrorMessage = "Adresse email invalide")]
        public string Email { get; set; }

    }
}
