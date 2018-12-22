using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nexplorer.Web.Cookies
{
    public class UserSettingsCookieData
    {
        public bool DismissedCookiePolicy { get; set; }

        public UserSettingsCookieData()
        {
            DismissedCookiePolicy = false;
        }
    }
}
