using CCXT.Simple.Core.Converters;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Upbit
{
    /// <summary>
    /// Upbit funds/account state response model
    /// </summary>
    /// <remarks>
    /// API Endpoint: GET /api/v1/funds (CCX API)
    /// Contains member level, currencies, accounts, and unit currency information
    /// </remarks>
    public class CoinState
    {
        /// <summary>
        /// Member level and verification information
        /// </summary>
        public MemberLevel member_level { get; set; }

        /// <summary>
        /// List of available currencies with withdrawal fees and wallet states
        /// </summary>
        public List<Currency> currencies { get; set; }

        /// <summary>
        /// List of account balances for each currency
        /// </summary>
        public List<Account> accounts { get; set; }

        /// <summary>
        /// Base unit currency for the account (e.g., "KRW")
        /// </summary>
        public string unit_currency { get; set; }
    }

    /// <summary>
    /// Member level and security verification status
    /// </summary>
    public class MemberLevel
    {
        /// <summary>
        /// Unique user identifier (UUID format)
        /// </summary>
        public string uuid { get; set; }

        /// <summary>
        /// Whether the account is activated
        /// </summary>
        public bool activated { get; set; }

        /// <summary>
        /// Account type classification
        /// </summary>
        public string type { get; set; }

        /// <summary>
        /// Security level (1-4, higher is more secure)
        /// </summary>
        public int security_level { get; set; }

        /// <summary>
        /// Withdrawal permission level
        /// </summary>
        public int withdraw_level { get; set; }

        /// <summary>
        /// Trading fee level (affects fee discount)
        /// </summary>
        public int fee_level { get; set; }

        /// <summary>
        /// Whether email verification is completed
        /// </summary>
        public bool email_verified { get; set; }

        /// <summary>
        /// Whether identity authentication (KYC) is verified
        /// </summary>
        public bool identity_auth_verified { get; set; }

        /// <summary>
        /// Whether bank account is verified for KRW deposits/withdrawals
        /// </summary>
        public bool bank_account_verified { get; set; }

        /// <summary>
        /// Whether Kakao Pay authentication is verified
        /// </summary>
        public bool kakao_pay_auth_verified { get; set; }

        /// <summary>
        /// Whether two-factor authentication (2FA) is enabled
        /// </summary>
        public bool two_factor_auth_verified { get; set; }

        /// <summary>
        /// Whether the account is locked (trading disabled)
        /// </summary>
        public bool locked { get; set; }

        /// <summary>
        /// Whether the wallet is locked (withdrawals disabled)
        /// </summary>
        public bool wallet_locked { get; set; }

        /// <summary>
        /// Whether fiat (KRW) withdrawals are disabled
        /// </summary>
        public bool withdraw_fiat_disabled { get; set; }
    }

    /// <summary>
    /// Currency information including withdrawal fees and wallet support
    /// </summary>
    public class Currency
    {
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH", "KRW")
        /// </summary>
        public string code { get; set; }

        /// <summary>
        /// External withdrawal fee amount
        /// </summary>
        [JsonConverter(typeof(XDecimalNullConverter))]
        public decimal withdraw_fee { get; set; }

        /// <summary>
        /// Internal transfer fee (between Upbit accounts)
        /// </summary>
        public decimal withdraw_internal_fee { get; set; }

        /// <summary>
        /// Whether this is a cryptocurrency (true) or fiat (false)
        /// </summary>
        public bool is_coin { get; set; }

        /// <summary>
        /// Wallet state: working, paused, withdraw_only, deposit_only, unsupported
        /// </summary>
        public string wallet_state { get; set; }

        /// <summary>
        /// List of supported networks for this currency
        /// </summary>
        public List<string> wallet_support { get; set; }

        /// <summary>
        /// Network type for deposits/withdrawals (e.g., "ERC20", "BEP20")
        /// </summary>
        public string net_type { get; set; }

        /// <summary>
        /// Deposit notice title displayed to users
        /// </summary>
        public string deposit_title { get; set; }

        /// <summary>
        /// Deposit notice body messages (list of strings)
        /// </summary>
        public List<string> deposit_body { get; set; }

        /// <summary>
        /// Withdrawal notice title displayed to users
        /// </summary>
        public string withdraw_title { get; set; }

        /// <summary>
        /// Withdrawal notice body messages (list of strings)
        /// </summary>
        public List<string> withdraw_body { get; set; }
    }

    /// <summary>
    /// Account balance information for a specific currency
    /// </summary>
    public class Account
    {
        /// <summary>
        /// Currency code (e.g., "BTC", "ETH")
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Available balance that can be used for trading or withdrawal
        /// </summary>
        public decimal balance { get; set; }

        /// <summary>
        /// Balance locked in open orders or pending operations
        /// </summary>
        public decimal locked { get; set; }

        /// <summary>
        /// Average KRW buy price for this currency
        /// </summary>
        public decimal avg_krw_buy_price { get; set; }

        /// <summary>
        /// Whether the average buy price was manually modified
        /// </summary>
        public bool modified { get; set; }

        /// <summary>
        /// Average buy price in the unit currency
        /// </summary>
        public decimal avg_buy_price { get; set; }

        /// <summary>
        /// Whether the average buy price was modified by user
        /// </summary>
        public bool avg_buy_price_modified { get; set; }

        /// <summary>
        /// Unit currency for average buy price calculation (e.g., "KRW")
        /// </summary>
        public string unit_currency { get; set; }
    }
}
