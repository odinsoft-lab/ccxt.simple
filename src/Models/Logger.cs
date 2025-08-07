namespace CCXT.Simple.Models
{
    public class XLogger
    {
        public DateTime time
        {
            get;
            set;
        }

        public string level
        {
            get;
            set;
        }

        public int error_no
        {
            get;
            set;
        }

        public string exchange
        {
            get;
            set;
        }

        public string message
        {
            get;
            set;
        }
    }
}