using HearstWebService.Data.Model;
using System.Security;

namespace HearstWebService.Interfaces
{
    public interface IAuthenticationLogic
    {
        bool IsUserAuthenticated(User user);
        User Login(string domain, string username, string password);
        void Logout(string domain, string username);
        Credentials GetUserCredentials(string domain, string username);
        ISecureStringToStringMarshaller GetSecureStringToStringMarshaller(SecureString securePassword);
    }
}
