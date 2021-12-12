using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    class Data_banned
    {
        public int id;
        public string ip;
        public string mac;
        public DateTime bandate;
        public DateTime unbandate;
        public string bannedby;
        public string banreason;
        public int active;
    }
}
