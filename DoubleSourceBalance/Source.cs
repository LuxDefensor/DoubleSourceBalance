using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleSourceBalance
{
    struct Source
    {
        private string server;
        private string database;
        private string user;
        private string password;
        private string id;

        public Source(string id,string server,string database,string user,string password)
        {
            this.id = id;
            this.server = server;
            this.database = database;
            this.user = user;
            this.password = password;
        }

        public string Server
        {
            get
            {
                return server;
            }
        }

        public string Database
        {
            get
            {
                return database;
            }
        }

        public string User
        {
            get
            {
                return user;
            }
        }

        public string Password
        {
            get
            {
                return password;
            }
        }

        public string Id
        {
            get
            {
                return id;
            }
        }
    }
}
