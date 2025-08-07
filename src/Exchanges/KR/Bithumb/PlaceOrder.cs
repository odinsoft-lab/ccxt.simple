using Newtonsoft.Json;

namespace CCXT.Simple.Exchanges.Bithumb
{
    public class PlaceOrders : ApiResult
    {
        /// <summary>
        /// 결과 상태 코드 (정상 : 0000, 정상이외 코드는 에러 코드 참조)
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
        /// 주문번호
        /// </summary>
        [JsonProperty(PropertyName = "order_id")]
        public string orderId
        {
            get;
            set;
        }
    }
}