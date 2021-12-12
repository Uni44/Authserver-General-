using System;
using System.Collections.Generic;
using System.Text;

namespace Authserver
{
    class Data_account
    {
        public int id = 0;
        public string username;
        public string pass;
        public string sessionkey;
        public string v;
        public string s;
        public string email;
        public DateTime joindate = System.DateTime.Now;
        public string last_ip;
        public string last_mac;
        public int failed_logins;
        public int locked;
        public DateTime last_login = DateTime.Parse("2001-01-01 00:00:00");
        public int online;
        public DateTime time_locked;
    }
}
