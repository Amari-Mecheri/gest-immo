using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class RegisterDto
    {
        [Required(ErrorMessage ="Le prénom est requis")]
        [StringLength(15,MinimumLength = 3, ErrorMessage ="Le prénom doit contenir entre {2} et {1} caractères")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Le nom est requis")]
        [StringLength(15,MinimumLength = 3, ErrorMessage ="Le nom doit contenir entre {2} et {1} caractères")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Une adresse email est requise")]
        [RegularExpression("^[a-zA-Z0-9+_.-]+@[a-zA-Z0-9.-]+$", ErrorMessage ="Adresse email invalide")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Un mot de passe est requis")]
        [StringLength(15,MinimumLength = 8, ErrorMessage ="Le mot de passe doit contenir entre {2} et {1} caractères")]
        public string Password { get; set; }

    }
}
