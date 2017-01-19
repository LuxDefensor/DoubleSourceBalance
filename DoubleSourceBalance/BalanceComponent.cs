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

    public enum CalculateMethods
    {
        interval,
        integral
    }

    struct BalanceComponent
    {
        private string sign;
        private BalanceSides side;
        private Source source;
        private string channel;
        private CalculateMethods method;

        public BalanceComponent(string sign, BalanceSides side, 
            Source source, string channel, CalculateMethods method)
        {
            this.sign = sign;
            this.side = side;
            this.source = source;
            this.channel = channel;
            this.method = method;
        }

        public CalculateMethods Method
        {
        get
            {
                return method;
            }
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

        public Source Source
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

        public string Name
        {
        get
            {
                return DataProvider.ChannelName(source, channel);
            }
        }
    }
}
