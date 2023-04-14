namespace PDF_Reader_APIs.Server.Authentication
{
     public class UserRepository: IUserRepository
    {
        private List<User> UsersList = new List<User>
        {
            new User 
            {
                Id = 1, Username = "Mohammed", Password = "DoesThisWork123?"            
            },
            new User
            {
                Id = 2, Username = "RihalTeam", Password = "Your_PW:)"
            },
        };
        public async Task<bool> Authenticate(string username, string password)
        {
            if(UsersList.Any(x => x.Username == username && x.Password == password))
            {
                return true;
            }
            return false;
        }
        public async Task<List<string>> GetUsernames()
        {
            List<User> Users = new List<User>();
            foreach(var User in UsersList)
            {
                Users.Add(User);
            }
            return await Task.FromResult(Users.Select(u => u.Username).ToList());
        }
    }   
}