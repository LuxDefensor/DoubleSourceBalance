using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Data;
using System.Data.SqlClient;

namespace DoubleSourceBalance
{
    class Controller
    {
        private List<Source> sources;
        private List<Balance> balances;
        private const string fileName = "Balances.xml";


        public Controller()
        {
            sources = Parser.GetSources(fileName);
            balances = Parser.GetBalances(fileName);
        }

        #region Properties
        public List<Source> Sources
        {
            get
            {
                return new List<Source>(sources);
            }
        }

        public List<Balance> Balances
        {
            get
            {
                return new List<Balance>(balances);
            }
        }        

        #endregion
    }
}
