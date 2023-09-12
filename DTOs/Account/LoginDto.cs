using System.ComponentModel.DataAnnotations;

namespace Gest_Immo_API.DTOs.Account
{
    public class LoginDto
    {
        [Required(ErrorMessage ="Une adresse email est requise")]
        public string UserName { get; set; }
        [Required(ErrorMessage ="Un mot de passe est requis")]
        public string Password { get; set; }
    }
}
