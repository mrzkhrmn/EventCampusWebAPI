using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventCampusAPI.Models
{
    public class LoginResponseModel
    {
        public string Token { get; set; }
        public UserInfoModel UserInfo { get; set; }
    }
}