namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit wallet status response model for deposit/withdrawal availability
    /// </summary>
    /// <remarks>
    /// <para>API Endpoint: GET /api/v1/status/wallet (CCX API)</para>
    /// <para>Provides real-time wallet status for each currency including blockchain state</para>
    /// </remarks>
    public class WalletState
    {
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH")
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Wallet operational state
        /// </summary>
        /// <remarks>
        /// Possible values:
        /// <list type="bullet">
        ///   <item>working: Both deposit and withdrawal available</item>
        ///   <item>paused: Both deposit and withdrawal suspended</item>
        ///   <item>withdraw_only: Only withdrawal available</item>
        ///   <item>deposit_only: Only deposit available</item>
        ///   <item>unsupported: Currency not supported</item>
        /// </list>
        /// </remarks>
        public string wallet_state { get; set; }

        /// <summary>
        /// Blockchain synchronization state
        /// </summary>
        /// <remarks>
        /// Possible values:
        /// <list type="bullet">
        ///   <item>normal: Blockchain synced and working normally</item>
        ///   <item>delayed: Blockchain sync is delayed</item>
        ///   <item>inactive: Blockchain is not active</item>
        /// </list>
        /// </remarks>
        public string block_state { get; set; }

        /// <summary>
        /// Current blockchain height (block number)
        /// </summary>
        /// <remarks>
        /// Null if blockchain height is not available
        /// </remarks>
        public long? block_height { get; set; }

        /// <summary>
        /// Timestamp when block information was last updated
        /// </summary>
        /// <remarks>
        /// Null if block update time is not available
        /// </remarks>
        public DateTime? block_updated_at { get; set; }

        /// <summary>
        /// Minutes elapsed since last block update
        /// </summary>
        /// <remarks>
        /// Used to detect blockchain delays. High values may indicate network issues.
        /// Null if not available.
        /// </remarks>
        public int? block_elapsed_minutes { get; set; }

        /// <summary>
        /// Network type for multi-chain currencies (e.g., "ERC20", "BEP20", "TRC20")
        /// </summary>
        public string net_type { get; set; }

        /// <summary>
        /// Human-readable network name (e.g., "Ethereum", "Binance Smart Chain")
        /// </summary>
        public string network_name { get; set; }
    }
}
