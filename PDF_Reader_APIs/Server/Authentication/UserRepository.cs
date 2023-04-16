namespace PDF_Reader_APIs.Server.Authentication
{
     public class UserRepository: IUserRepository
    {
        private List<User> UsersList = new List<User> //List of all the applicable username + password combinations for basic authentication
        {
            new User 
            {
                Id = 1, Username = "Mohammed", Password = "DoesThisWork123?"            
            },
            new User
            {
                Id = 2, Username = "RihalTeam", Password = "Your_PW:)"
            },
            new User
            {
                Id = 3, Username = "ForDeleteOnly", Password = "PlsDoNotUse101" //Dedicated ID for deleting only
            }
        };

         //Authenticates if the inputted username password matches valud user's list
        public async Task<bool> Authenticate(string username, string password, string Method)
        {
            if(Method.ToUpper() == "DELETE" && UsersList.Any(x => x.Username == username && x.Password == password && x.Id != 3)) //Checks if the request is DELETE and compares it to ID 3
            {   
                return true;
            }
            else if(UsersList.Any(x => x.Username == username && x.Password == password)) //If it is not delete and the username and PW match
            {
                return true;
            }
            return false;
        }
    }   
}