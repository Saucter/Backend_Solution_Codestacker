using Microsoft.AspNetCore.Authentication;

namespace PDF_Reader_APIs.Server.Authentication
{
    //Users class simply used to create property for the UserRepository
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }  
}