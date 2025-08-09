namespace CCXT.Simple.Core
{
    /// <summary>
    /// api call result class
    /// </summary>
    public class ApiResult
    {
        /// <summary>
        /// api call result class
        /// </summary>
        public ApiResult(bool success = false)
        {
            this.success = success;
            this.supported = true;
        }

        /// <summary>
        /// is success calling
        /// </summary>
        public virtual bool success
        {
            get;
            set;
        }

        /// <summary>
        /// error or success message
        /// </summary>
        public virtual string message
        {
            get;
            set;
        }

        /// <summary>
        /// status, error code
        /// </summary>
        public virtual int statusCode
        {
            get;
            set;
        }

        /// <summary>
        /// check implemented
        /// </summary>
        public virtual bool supported
        {
            get;
            set;
        }
    }
}