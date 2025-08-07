namespace CCXT.Simple.Models
{
    //public class WStates
    //{
    //    public WStates()
    //    {
    //        this.states = new List<WState>();
    //    }

    //    public string exchange
    //    {
    //        get;
    //        set;
    //    }

    //    public List<WState> states
    //    {
    //        get;
    //        set;
    //    }
    //}

    public class WState
    {
        public string baseName
        {
            get;
            set;
        }

        /// <summary>
        /// wallet state
        /// </summary>
        public bool active
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        /// <summary>
        /// travle-rule
        /// </summary>
        public bool travelRule
        {
            get;
            set;
        }

        /// <summary>
        /// block state
        /// </summary>
        public bool block
        {
            get;
            set;
        }

        /// <summary>
        /// block height
        /// </summary>
        public long height
        {
            get;
            set;
        }

        /// <summary>
        /// block updated at
        /// </summary>
        public DateTime updated
        {
            get;
            set;
        }

        /// <summary>
        /// block elapsed minutes
        /// </summary>
        public int elapsed
        {
            get;
            set;
        }

        /// <summary>
        /// transaction minimum fee withdraw
        /// </summary>
        public decimal makerFee
        {
            get;
            set;
        }

        /// <summary>
        /// transaction maximum fee withdraw
        /// </summary>
        public decimal takerFee
        {
            get;
            set;
        }

        public List<WNetwork> networks
        {
            get;
            set;
        }
    }

    public class WNetwork
    {
        /// <summary>
        /// chain name by exchange
        /// </summary>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// ETH, BSC, TRX, etc...
        /// </summary>
        public string network
        {
            get;
            set;
        }

        /// <summary>
        /// ERC20, BEP20, TRC20, etc...
        /// </summary>
        public string chain
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        public decimal withdrawFee
        {
            get;
            set;
        }

        public decimal depositFee
        {
            get;
            set;
        }

        public decimal sellFee
        {
            get;
            set;
        }

        public decimal buyFee
        {
            get;
            set;
        }

        public decimal minWithdrawal
        {
            get;
            set;
        }

        public decimal maxWithdrawal
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int minConfirm
        {
            get;
            set;
        }

        /// <summary>
        /// 
        /// </summary>
        public int arrivalTime
        {
            get;
            set;
        }
    }
}