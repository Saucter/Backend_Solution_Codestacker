using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace PDF_Reader_APIs.Server.Authentication
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IUserRepository UserAuthenticator;
        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, IUserRepository UserAuthenticator)
        : base(options, logger, encoder, clock) //Applies required parameters in order to utilize AuthenticationHandler 
        {
            this.UserAuthenticator = UserAuthenticator;
        }

        protected async override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var authorizationHeader = Request.Headers["Authorization"].ToString(); //Checks the authorization header
            if (authorizationHeader != null && authorizationHeader.StartsWith("basic", StringComparison.OrdinalIgnoreCase)) //Ensures that authentication is of type Basic
            {
                var token = authorizationHeader.Substring("Basic ".Length).Trim(); 
                var credentialsAsEncodedString = Encoding.UTF8.GetString(Convert.FromBase64String(token)); //Decode authentication username and PW
                var credentials = credentialsAsEncodedString.Split(':'); //Split the Basic authentication at the colon (username:password)
                if (await UserAuthenticator.Authenticate(credentials[0], credentials[1], Request.Method)) //Checks if the username and PW are valid
                {
                    var claims = new[] { new Claim("name", credentials[0]), new Claim(ClaimTypes.Role, "Admin") }; //Creates a role for the username and password
                    var identity = new ClaimsIdentity(claims, "Basic");
                    var claimsPrincipal = new ClaimsPrincipal(identity);
                    return await Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(claimsPrincipal, Scheme.Name))); //Returns successful authentication 'ticket'
                }
            }
            Response.StatusCode = 401; //Returns 401 if authentication failed
            Response.Headers.Add("WWW-Authenticate", "Basic realm=\"PDF_Reader_API\""); 
            return AuthenticateResult.Fail("Invalid Authorization Header"); 
        }
    }  
}