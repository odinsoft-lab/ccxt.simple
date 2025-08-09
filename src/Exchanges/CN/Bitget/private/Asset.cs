namespace CCXT.Simple.Exchanges.Bitget.Private
{
    /// <summary>
    /// POST /api/spot/v1/account/assets
    /// </summary>

    public class Asset : RResult<List<AssetData>>
    {
    }

    public class AssetData
    {
        public int coinId { get; set; }
        public string coinName { get; set; }
        public decimal available { get; set; }
        public decimal frozen { get; set; }
        public decimal @lock { get; set; }
        public long uTime { get; set; }
    }

    /// <summary>
    /// POST /api/spot/v1/account/sub-account-spot-assets
    /// </summary>

    public class SAsset : RResult<List<SAssetData>>
    {
    }

    public class SAssetData
    {
        public long userId { get; set; }
        public List<AssetData> spotAssetsList { get; set; }
    }
}