using CCXT.Simple.Core.Utilities;
using Xunit;

namespace CCXT.Simple.Tests.Utilities
{
    public class ChainNetworkTests
    {
        [Fact]
        public void ChainNetwork_Properties_SetCorrectly()
        {
            // Arrange & Act
            var network = new ChainNetwork
            {
                network = "ETH",
                chain = "ERC20"
            };

            // Assert
            Assert.Equal("ETH", network.network);
            Assert.Equal("ERC20", network.chain);
        }
    }

    public class ChainItemTests
    {
        [Fact]
        public void ChainItem_Properties_SetCorrectly()
        {
            // Arrange & Act
            var item = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" },
                    new ChainNetwork { network = "TRX", chain = "TRC20" }
                }
            };

            // Assert
            Assert.Equal("USDT", item.baseName);
            Assert.Equal(2, item.networks.Count);
        }

        [Fact]
        public void ChainItem_Equals_SameNetwork_ReturnsTrue()
        {
            // Arrange
            var item1 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            var item2 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" },
                    new ChainNetwork { network = "TRX", chain = "TRC20" }
                }
            };

            // Act
            var result = item1.Equals(item2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ChainItem_Equals_DifferentNetwork_ReturnsFalse()
        {
            // Arrange
            var item1 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            var item2 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "TRX", chain = "TRC20" }
                }
            };

            // Act
            var result = item1.Equals(item2);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ChainItem_Equals_EmptyNetworks_ReturnsTrue()
        {
            // Arrange
            var item1 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>()
            };

            var item2 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            // Act - Empty networks means we accept any match
            var result = item1.Equals(item2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ChainItem_Equals_Null_ReturnsFalse()
        {
            // Arrange
            var item = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            // Act
            var result = item.Equals((ChainItem)null!);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ChainItem_EqualsObject_SameType_ReturnsCorrectResult()
        {
            // Arrange
            var item1 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            var item2 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            // Act
            var result = item1.Equals((object)item2);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void ChainItem_EqualsObject_DifferentType_ReturnsFalse()
        {
            // Arrange
            var item = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>()
            };

            // Act
            var result = item.Equals("not a ChainItem");

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void ChainItem_GetHashCode_ReturnsSameForSameBaseName()
        {
            // Arrange
            var item1 = new ChainItem { baseName = "USDT", networks = new List<ChainNetwork>() };
            var item2 = new ChainItem { baseName = "USDT", networks = new List<ChainNetwork>() };

            // Act & Assert
            Assert.Equal(item1.GetHashCode(), item2.GetHashCode());
        }

        [Fact]
        public void ChainItem_OperatorEquals_BothNull_ReturnsTrue()
        {
            // Arrange
            ChainItem? item1 = null;
            ChainItem? item2 = null;

            // Act & Assert
            Assert.True(item1 == item2);
        }

        [Fact]
        public void ChainItem_OperatorEquals_OneNull_ReturnsFalse()
        {
            // Arrange
            ChainItem? item1 = new ChainItem { baseName = "USDT", networks = new List<ChainNetwork>() };
            ChainItem? item2 = null;

            // Act & Assert
            Assert.False(item1 == item2);
        }

        [Fact]
        public void ChainItem_OperatorNotEquals_DifferentItems_ReturnsTrue()
        {
            // Arrange
            var item1 = new ChainItem
            {
                baseName = "USDT",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "ETH", chain = "ERC20" }
                }
            };

            var item2 = new ChainItem
            {
                baseName = "BTC",
                networks = new List<ChainNetwork>
                {
                    new ChainNetwork { network = "BTC", chain = "BTC" }
                }
            };

            // Act & Assert
            Assert.True(item1 != item2);
        }
    }

    public class ChainDataTests
    {
        [Fact]
        public void ChainData_Properties_SetCorrectly()
        {
            // Arrange & Act
            var chainData = new ChainData
            {
                exchange = "binance",
                items = new List<ChainItem>
                {
                    new ChainItem
                    {
                        baseName = "USDT",
                        networks = new List<ChainNetwork>
                        {
                            new ChainNetwork { network = "ETH", chain = "ERC20" }
                        }
                    }
                }
            };

            // Assert
            Assert.Equal("binance", chainData.exchange);
            Assert.Single(chainData.items);
            Assert.Equal("USDT", chainData.items[0].baseName);
        }

        [Fact]
        public void ChainData_EmptyItems_IsValid()
        {
            // Arrange & Act
            var chainData = new ChainData
            {
                exchange = "kraken",
                items = new List<ChainItem>()
            };

            // Assert
            Assert.Equal("kraken", chainData.exchange);
            Assert.Empty(chainData.items);
        }

        [Fact]
        public void ChainData_MultipleItems_ContainsAll()
        {
            // Arrange & Act
            var chainData = new ChainData
            {
                exchange = "bybit",
                items = new List<ChainItem>
                {
                    new ChainItem { baseName = "BTC", networks = new List<ChainNetwork>() },
                    new ChainItem { baseName = "ETH", networks = new List<ChainNetwork>() },
                    new ChainItem { baseName = "USDT", networks = new List<ChainNetwork>() }
                }
            };

            // Assert
            Assert.Equal(3, chainData.items.Count);
            Assert.Contains(chainData.items, x => x.baseName == "BTC");
            Assert.Contains(chainData.items, x => x.baseName == "ETH");
            Assert.Contains(chainData.items, x => x.baseName == "USDT");
        }
    }
}
