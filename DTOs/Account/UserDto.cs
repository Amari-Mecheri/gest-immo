namespace Gest_Immo_API.DTOs.Account
{
    public class UserDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string JWT { get; set; }
        public UserDto() { }    
    }
}
