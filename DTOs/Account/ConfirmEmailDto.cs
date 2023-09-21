using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class ConfirmEmailDto
    {
        [Required]
        public string Token { get; set; }

        [Required]
        [RegularExpression("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$", ErrorMessage = "Adresse email invalide")]
        public string Email { get; set; }

    }
}
