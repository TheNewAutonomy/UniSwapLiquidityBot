using Nethereum.Web3;
using Nethereum.Contracts;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System;

public static class UniswapV3Helper
{
    private const string UNISWAP_V3_POOL_ABI = @"[
        {
            ""inputs"": [],
            ""name"": ""slot0"",
            ""outputs"": [
                { ""internalType"": ""uint160"", ""name"": ""sqrtPriceX96"", ""type"": ""uint160"" },
                { ""internalType"": ""int24"", ""name"": ""tick"", ""type"": ""int24"" },
                { ""internalType"": ""uint16"", ""name"": ""observationIndex"", ""type"": ""uint16"" },
                { ""internalType"": ""uint16"", ""name"": ""observationCardinality"", ""type"": ""uint16"" },
                { ""internalType"": ""uint16"", ""name"": ""observationCardinalityNext"", ""type"": ""uint16"" },
                { ""internalType"": ""uint8"", ""name"": ""feeProtocol"", ""type"": ""uint8"" },
                { ""internalType"": ""bool"", ""name"": ""unlocked"", ""type"": ""bool"" }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";

    // Address of the Uniswap V3 pool contract
    private const string POOL_CONTRACT_ADDRESS = "0xUNISWAP_V3_POOL_CONTRACT_ADDRESS";

    public static async Task<decimal> GetCurrentPoolPrice(Web3 web3)
    {
        var poolContract = web3.Eth.GetContract(UNISWAP_V3_POOL_ABI, POOL_CONTRACT_ADDRESS);
        var slot0Function = poolContract.GetFunction("slot0");

        var slot0 = await slot0Function.CallDeserializingToObjectAsync<Slot0>();

        // Calculate the price from sqrtPriceX96
        // The formula to convert sqrtPriceX96 to price is: (sqrtPriceX96 / 2^96)^2
        decimal sqrtPriceX96 = (decimal)slot0.SqrtPriceX96;
        decimal price = (sqrtPriceX96 / (decimal)Math.Pow(2, 96));
        price = price * price;

        return price;
    }

    public class Slot0
    {
        [Parameter("uint160", "sqrtPriceX96", 1)]
        public BigInteger SqrtPriceX96 { get; set; }

        [Parameter("int24", "tick", 2)]
        public int Tick { get; set; }

        [Parameter("uint16", "observationIndex", 3)]
        public ushort ObservationIndex { get; set; }

        [Parameter("uint16", "observationCardinality", 4)]
        public ushort ObservationCardinality { get; set; }

        [Parameter("uint16", "observationCardinalityNext", 5)]
        public ushort ObservationCardinalityNext { get; set; }

        [Parameter("uint8", "feeProtocol", 6)]
        public byte FeeProtocol { get; set; }

        [Parameter("bool", "unlocked", 7)]
        public bool Unlocked { get; set; }
    }
}
