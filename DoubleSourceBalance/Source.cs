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
        private string sql;

        public Source(string id, string server, string database, string user, string password, string sql)
        {
            this.id = id;
            this.server = server;
            this.database = database;
            this.user = user;
            this.password = password;
            this.sql = sql;
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

        public string SQL
        {
        get
            {
                return sql;
            }
        }

        public override string ToString()
        {
            return string.Format("server={0}; database={1}; user={2}", server, database, user);
        }
    }
}
