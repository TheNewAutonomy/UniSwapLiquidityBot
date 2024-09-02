using Nethereum.Web3;
using Nethereum.Contracts;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

public static class UniswapV3PositionHelper
{
    private const string NONFUNGIBLE_POSITION_MANAGER_ABI = @"[
        {
            ""inputs"": [
                { ""internalType"": ""uint256"", ""name"": ""tokenId"", ""type"": ""uint256"" }
            ],
            ""name"": ""positions"",
            ""outputs"": [
                { ""internalType"": ""uint96"", ""name"": ""nonce"", ""type"": ""uint96"" },
                { ""internalType"": ""address"", ""name"": ""operator"", ""type"": ""address"" },
                { ""internalType"": ""address"", ""name"": ""token0"", ""type"": ""address"" },
                { ""internalType"": ""address"", ""name"": ""token1"", ""type"": ""address"" },
                { ""internalType"": ""uint24"", ""name"": ""fee"", ""type"": ""uint24"" },
                { ""internalType"": ""int24"", ""name"": ""tickLower"", ""type"": ""int24"" },
                { ""internalType"": ""int24"", ""name"": ""tickUpper"", ""type"": ""int24"" },
                { ""internalType"": ""uint128"", ""name"": ""liquidity"", ""type"": ""uint128"" },
                { ""internalType"": ""uint256"", ""name"": ""feeGrowthInside0LastX128"", ""type"": ""uint256"" },
                { ""internalType"": ""uint256"", ""name"": ""feeGrowthInside1LastX128"", ""type"": ""uint256"" },
                { ""internalType"": ""uint128"", ""name"": ""tokensOwed0"", ""type"": ""uint128"" },
                { ""internalType"": ""uint128"", ""name"": ""tokensOwed1"", ""type"": ""uint128"" }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";

    private const string POSITION_MANAGER_ADDRESS = "0xC36442b4a4522E871399CD717aBDD847Ab11FE88";

    public static async Task<Position> GetPosition(Web3 web3, BigInteger tokenId)
    {
        var positionManager = web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, POSITION_MANAGER_ADDRESS);
        var positionsFunction = positionManager.GetFunction("positions");

        var position = await positionsFunction.CallDeserializingToObjectAsync<Position>(tokenId);
        return position;
    }

    [FunctionOutputAttribute]
    public class Position
    {
        [Parameter("uint96", "nonce", 1)]
        public ulong Nonce { get; set; }

        [Parameter("address", "operator", 2)]
        public string Operator { get; set; }

        [Parameter("uint96", "id", 1)]
        public ulong Id { get; set; }

        [Parameter("address", "token0", 3)]
        public string Token0 { get; set; }

        [Parameter("address", "token1", 4)]
        public string Token1 { get; set; }

        [Parameter("uint24", "fee", 5)]
        public uint Fee { get; set; }

        [Parameter("int24", "tickLower", 6)]
        public int TickLower { get; set; }

        [Parameter("int24", "tickUpper", 7)]
        public int TickUpper { get; set; }

        [Parameter("uint128", "liquidity", 8)]
        public BigInteger Liquidity { get; set; }

        [Parameter("uint256", "feeGrowthInside0LastX128", 9)]
        public BigInteger FeeGrowthInside0LastX128 { get; set; }

        [Parameter("uint256", "feeGrowthInside1LastX128", 10)]
        public BigInteger FeeGrowthInside1LastX128 { get; set; }

        [Parameter("uint128", "tokensOwed0", 11)]
        public BigInteger TokensOwed0 { get; set; }

        [Parameter("uint128", "tokensOwed1", 12)]
        public BigInteger TokensOwed1 { get; set; }
    }
}
