using Microsoft.AspNetCore.Authentication;

namespace PDF_Reader_APIs.Server.Authentication
{
    public class User : AuthenticationSchemeOptions
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }  
}