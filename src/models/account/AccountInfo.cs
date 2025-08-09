namespace CCXT.Simple.Models.Account
{
    public class AccountInfo
    {
        public string id { get; set; }
        public string type { get; set; }
        public Dictionary<string, BalanceInfo> balances { get; set; }
        public bool canTrade { get; set; }
        public bool canWithdraw { get; set; }
        public bool canDeposit { get; set; }
    }
}