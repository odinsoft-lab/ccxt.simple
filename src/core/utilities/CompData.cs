using System;

namespace CCXT.Simple.Core.Utilities
{
    public class CompItem
    {
        public string baseName
        {
            get;
            set;
        }

        public string compName
        {
            get;
            set;
        }
    
        public string dispName
        {
            get;
            set;
        }
    }

    public class CompData
    {
        public string exchange
        {
            get;
            set;
        }

        public List<CompItem> items
        {
            get;
            set;
        }
    }
}