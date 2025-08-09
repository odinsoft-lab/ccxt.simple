namespace CCXT.Simple.Core.Utilities
{
    public class ChainNetwork
    {
        public string network
        {
            get;
            set;
        }

        public string chain
        {
            get;
            set;
        }
    }

    public class ChainItem : IEquatable<ChainItem>
    {
        public string baseName
        {
            get;
            set;
        }

        public List<ChainNetwork> networks
        {
            get;
            set;
        }

        public bool Equals(ChainItem other)
        {
            var _result = false;

            if (other != null)
            {
                _result = true;

                if (this.networks.Count > 0)
                {
                    _result = false;

                    foreach (var p in this.networks)
                    {
                        if (other.networks.Exists(x => x.network == p.network && x.chain == p.chain))
                        {
                            _result = true;
                            break;
                        }
                    }
                }
            }

            return _result;
        }

        public override bool Equals(Object obj)
        {
            var _result = false;

            if (obj != null)
            {
                var _chain_obj = obj as ChainItem;
                if (_chain_obj != null)
                    _result = Equals(_chain_obj);
            }

            return _result;
        }

        public override int GetHashCode()
        {
            return this.baseName.GetHashCode();
        }

        public static bool operator ==(ChainItem chain1, ChainItem chain2)
        {
            if (((object)chain1) == null || ((object)chain2) == null)
                return Object.Equals(chain1, chain2);

            return chain1.Equals(chain2);
        }

        public static bool operator !=(ChainItem chain1, ChainItem chain2)
        {
            if (((object)chain1) == null || ((object)chain2) == null)
                return !Object.Equals(chain1, chain2);

            return !chain1.Equals(chain2);
        }
    }

    public class ChainData
    {
        public string exchange
        {
            get;
            set;
        }

        public List<ChainItem> items
        {
            get;
            set;
        }
    }
}