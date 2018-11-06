using System.Security;

namespace HearstWebService.Data.Model
{
    public class Credentials
    {
        public User User { get; set; }
        public SecureString Password { get; set; }
    }
}
