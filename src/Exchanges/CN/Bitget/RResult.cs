namespace CCXT.Simple.Exchanges.Bitget.RA
{
    public class RResult<T>
    {
        public int code { get; set; }
        public string msg { get; set; }
        public long requestTime { get; set; }
        public T data { get; set; }
#if DEBUG
        public string json { get; set; }
#endif
    }
}