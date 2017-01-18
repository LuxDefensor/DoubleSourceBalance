using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleSourceBalance
{
    struct Balance
    {
        private string name;
        private List<BalanceComponent> components;

        public Balance(string name,List<BalanceComponent> components)
        {
            this.name = name;
            this.components = new List<BalanceComponent>(components);
        }
        public string Name
        {
            get
            {
                return name;
            }
        }

        internal List<BalanceComponent> Components
        {
            get
            {
                return new List<BalanceComponent>(components);
            }
        }
    }
}
