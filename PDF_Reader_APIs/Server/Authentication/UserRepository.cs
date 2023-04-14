namespace PDF_Reader_APIs.Server.Authentication
{
     public class UserRepository: IUserRepository
    {
        private List<User> UsersList = new List<User>
        {
            new User 
            {
                Id = 1, Username = "peter", Password = "peter123"            
            },
            new User
            {
                Id = 2, Username = "joydip", Password = "joydip123"
            },
            new User
            {
                Id = 3, Username = "james", Password = "james123"
            }
        };
        public async Task<bool> Authenticate(string username, string password)
        {
            if(await Task.FromResult(UsersList.SingleOrDefault(x => x.Username == username && x.Password == password)) != null)
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