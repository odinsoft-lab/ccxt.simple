using CCXT.Simple.Core;
using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bithumb
{
    /// <summary>
    /// Represents a response to a place order request. Inherits ApiResult to standardize success handling.
    /// </summary>
    public class PlaceOrders : ApiResult
    {
    /// <summary>
    /// Result status code (success: 0000, otherwise see error code reference)
    /// </summary>
        [JsonProperty(PropertyName = "status")]
        public override int statusCode
        {
            get => base.statusCode;
            set
            {
                base.statusCode = value;

                if (statusCode == 0)
                {
                    message = "success";
                    success = true;
                }
            }
        }

    /// <summary>
    /// Order identifier
    /// </summary>
        [JsonProperty(PropertyName = "order_id")]
        public string orderId
        {
            get;
            set;
        }
    }
}