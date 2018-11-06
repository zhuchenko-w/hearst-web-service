using Newtonsoft.Json;
using System;

namespace HearstWebService.Data.Model
{
    public class User
    {
        public string Domain { get; set; }
        public string Username { get; set; }
        public Guid Token { get; set; }

        public string FullDomainUsername => $@"{Domain}\{Username}";
        public string SerializedUser => JsonConvert.SerializeObject(new { Domain, Username, Token });

        public static User DeserializeUser(string json)
        {
            return JsonConvert.DeserializeObject<User>(json);
        }
    }
}
