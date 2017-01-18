using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DoubleSourceBalance
{
    public enum BalanceSides
    {
        IN,
        OUT
    }

    struct BalanceComponent
    {
        private string sign;
        private BalanceSides side;
        private string source;
        private string channel;

        public BalanceComponent(string sign,BalanceSides side,string source,string channel)
        {
            this.sign = sign;
            this.side = side;
            this.source = source;
            this.channel = channel;
        }
        public string Sign
        {
            get
            {
                return sign;
            }
        }

        public BalanceSides Side
        {
            get
            {
                return side;
            }
        }

        public string Source
        {
            get
            {
                return source;
            }
        }

        public string Channel
        {
            get
            {
                return channel;
            }
        }
    }
}
