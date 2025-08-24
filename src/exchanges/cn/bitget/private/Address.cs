namespace CCXT.Simple.Exchanges.Bitget.Private
{
    /// <summary>
    /// GET /api/spot/v1/wallet/deposit-address?coin=USDT&amp;chain=trc20
    /// </summary>

    public class Address : RResult<AddressData>
    {
    }

    public class AddressData
    {
        public string address { get; set; }
        public string chain { get; set; }
        public string coin { get; set; }
        public string tag { get; set; }
        public string url { get; set; }
    }
}