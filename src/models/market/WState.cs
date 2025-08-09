namespace CCXT.Simple.Models.Market
{
    /// <summary>
    /// Represents wallet/coin state information including network status and limits
    /// </summary>
    public class WState
    {
        /// <summary>
        /// Base currency name (e.g., "BTC", "ETH")
        /// </summary>
        public string baseName
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether the wallet/currency is active for operations
        /// </summary>
        public bool active
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether withdrawals are enabled for this currency
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether deposits are enabled for this currency
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether travel rule compliance is required (for regulatory compliance)
        /// </summary>
        public bool travelRule
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether blockchain operations are currently blocked
        /// </summary>
        public bool block
        {
            get;
            set;
        }

        /// <summary>
        /// Current blockchain height for this currency
        /// </summary>
        public long height
        {
            get;
            set;
        }

        /// <summary>
        /// Last time the blockchain state was updated
        /// </summary>
        public DateTime updated
        {
            get;
            set;
        }

        /// <summary>
        /// Minutes elapsed since last blockchain update
        /// </summary>
        public int elapsed
        {
            get;
            set;
        }

        /// <summary>
        /// Maker fee rate for trading (as a decimal, e.g., 0.001 = 0.1%)
        /// </summary>
        public decimal makerFee
        {
            get;
            set;
        }

        /// <summary>
        /// Taker fee rate for trading (as a decimal, e.g., 0.002 = 0.2%)
        /// </summary>
        public decimal takerFee
        {
            get;
            set;
        }

        /// <summary>
        /// List of supported blockchain networks for this currency
        /// </summary>
        public List<WNetwork> networks
        {
            get;
            set;
        }
    }

    /// <summary>
    /// Represents a specific blockchain network configuration for a currency
    /// </summary>
    public class WNetwork
    {
        /// <summary>
        /// Network name as defined by the exchange
        /// </summary>
        public string name
        {
            get;
            set;
        }

        /// <summary>
        /// Blockchain network identifier (e.g., "ETH", "BSC", "TRX", "MATIC")
        /// </summary>
        public string network
        {
            get;
            set;
        }

        /// <summary>
        /// Token standard or chain type (e.g., "ERC20", "BEP20", "TRC20", "SPL")
        /// </summary>
        public string chain
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether withdrawals are enabled on this network
        /// </summary>
        public bool withdraw
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether deposits are enabled on this network
        /// </summary>
        public bool deposit
        {
            get;
            set;
        }

        /// <summary>
        /// Withdrawal fee for this network
        /// </summary>
        public decimal withdrawFee
        {
            get;
            set;
        }

        /// <summary>
        /// Deposit fee for this network (usually 0)
        /// </summary>
        public decimal depositFee
        {
            get;
            set;
        }

        /// <summary>
        /// Sell trading fee for this network
        /// </summary>
        public decimal sellFee
        {
            get;
            set;
        }

        /// <summary>
        /// Buy trading fee for this network
        /// </summary>
        public decimal buyFee
        {
            get;
            set;
        }

        /// <summary>
        /// Minimum withdrawal amount for this network
        /// </summary>
        public decimal minWithdrawal
        {
            get;
            set;
        }

        /// <summary>
        /// Maximum withdrawal amount for this network
        /// </summary>
        public decimal maxWithdrawal
        {
            get;
            set;
        }

        /// <summary>
        /// Minimum number of blockchain confirmations required
        /// </summary>
        public int minConfirm
        {
            get;
            set;
        }

        /// <summary>
        /// Estimated arrival time in minutes for deposits
        /// </summary>
        public int arrivalTime
        {
            get;
            set;
        }
    }
}