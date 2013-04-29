using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Owin.Security.Twitter.Messages
{
    public class AccessToken : RequestToken
    {
        public string UserId
        {
            get;
            set;
        }

        public string ScreenName
        {
            get;
            set;
        }
    }
}
