namespace PDF_Reader_APIs.Server.Authentication
{
    public interface IUserRepository
    {
        Task<bool> Authenticate(string username, string password, string Method);
    }
}
