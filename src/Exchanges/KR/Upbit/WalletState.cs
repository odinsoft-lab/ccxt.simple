namespace CCXT.Simple.Exchanges.Upbit 
{
    /// <summary>
    /// https://ccx.upbit.com/api/v1/status/wallet
    /// </summary>
    public class WalletState
    {
        public string currency
        {
            get;
            set;
        }

        public string wallet_state
        {
            get;
            set;
        }

        public string block_state
        {
            get;
            set;
        }

        public long? block_height
        {
            get;
            set;
        }

        public DateTime? block_updated_at
        {
            get;
            set;
        }

        public int? block_elapsed_minutes
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