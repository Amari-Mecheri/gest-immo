using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class RegisterDto
    {
        [Required]
        [StringLength(15,MinimumLength = 3, ErrorMessage ="Le prénom doit contenir entre {2} et {1} caractères")]
        public string FirstName { get; set; }
        [Required]
        [StringLength(15,MinimumLength = 3, ErrorMessage ="Le nom doit contenir entre {2} et {1} caractères")]
        public string LastName { get; set; }
        [Required]
        [RegularExpression("^\\w+@[a-zA-Z_]+?\\.[a-zA-Z]{2,3}$", ErrorMessage ="Adresse email invalide")]
        public string Email { get; set; }
        [Required]
        [StringLength(15,MinimumLength = 8, ErrorMessage ="Le mot de passe doit contenir entre {2} et {1} caractères")]
        public string Password { get; set; }

    }
}
