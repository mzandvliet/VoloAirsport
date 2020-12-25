using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RamjetAnvil.Volo {
    public class UserAccount {
        private readonly string _username;
        private readonly string _fullname;

        public UserAccount(string username, string fullname) {
            _username = username;
            _fullname = fullname;
        }

        public string Username {
            get { return _username; }
        }

        public string Fullname {
            get { return _fullname; }
        }
    }
}
