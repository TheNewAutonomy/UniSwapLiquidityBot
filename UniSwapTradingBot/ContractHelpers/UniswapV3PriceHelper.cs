using Nethereum.Web3;
using Nethereum.Contracts;
using System.Threading.Tasks;
using System.Numerics;
using Nethereum.ABI.FunctionEncoding.Attributes;

public static class UniswapV3PriceHelper
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

    private const string UNISWAP_V3_POOL_FACTORY_ABI = @"[
    {
        ""inputs"": [],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""constructor""
    },
    {
        ""anonymous"": false,
        ""inputs"": [
            {
                ""indexed"": true,
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
            },
            {
                ""indexed"": true,
                ""internalType"": ""int24"",
                ""name"": ""tickSpacing"",
                ""type"": ""int24""
            }
        ],
        ""name"": ""FeeAmountEnabled"",
        ""type"": ""event""
    },
    {
        ""anonymous"": false,
        ""inputs"": [
            {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""oldOwner"",
                ""type"": ""address""
            },
            {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""newOwner"",
                ""type"": ""address""
            }
        ],
        ""name"": ""OwnerChanged"",
        ""type"": ""event""
    },
    {
        ""anonymous"": false,
        ""inputs"": [
            {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""token0"",
                ""type"": ""address""
            },
            {
                ""indexed"": true,
                ""internalType"": ""address"",
                ""name"": ""token1"",
                ""type"": ""address""
            },
            {
                ""indexed"": true,
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
            },
            {
                ""indexed"": false,
                ""internalType"": ""int24"",
                ""name"": ""tickSpacing"",
                ""type"": ""int24""
            },
            {
                ""indexed"": false,
                ""internalType"": ""address"",
                ""name"": ""pool"",
                ""type"": ""address""
            }
        ],
        ""name"": ""PoolCreated"",
        ""type"": ""event""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""address"",
                ""name"": ""tokenA"",
                ""type"": ""address""
            },
            {
                ""internalType"": ""address"",
                ""name"": ""tokenB"",
                ""type"": ""address""
            },
            {
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
            }
        ],
        ""name"": ""createPool"",
        ""outputs"": [
            {
                ""internalType"": ""address"",
                ""name"": ""pool"",
                ""type"": ""address""
            }
        ],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
            },
            {
                ""internalType"": ""int24"",
                ""name"": ""tickSpacing"",
                ""type"": ""int24""
            }
        ],
        ""name"": ""enableFeeAmount"",
        ""outputs"": [],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""uint24"",
                ""name"": """",
                ""type"": ""uint24""
            }
        ],
        ""name"": ""feeAmountTickSpacing"",
        ""outputs"": [
            {
                ""internalType"": ""int24"",
                ""name"": """",
                ""type"": ""int24""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
            },
            {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
            },
            {
                ""internalType"": ""uint24"",
                ""name"": """",
                ""type"": ""uint24""
            }
        ],
        ""name"": ""getPool"",
        ""outputs"": [
            {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""inputs"": [],
        ""name"": ""owner"",
        ""outputs"": [
            {
                ""internalType"": ""address"",
                ""name"": """",
                ""type"": ""address""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""inputs"": [],
        ""name"": ""parameters"",
        ""outputs"": [
            {
                ""internalType"": ""address"",
                ""name"": ""factory"",
                ""type"": ""address""
            },
            {
                ""internalType"": ""address"",
                ""name"": ""token0"",
                ""type"": ""address""
            },
            {
                ""internalType"": ""address"",
                ""name"": ""token1"",
                ""type"": ""address""
            },
            {
                ""internalType"": ""uint24"",
                ""name"": ""fee"",
                ""type"": ""uint24""
            },
            {
                ""internalType"": ""int24"",
                ""name"": ""tickSpacing"",
                ""type"": ""int24""
            }
        ],
        ""stateMutability"": ""view"",
        ""type"": ""function""
    },
    {
        ""inputs"": [
            {
                ""internalType"": ""address"",
                ""name"": ""_owner"",
                ""type"": ""address""
            }
        ],
        ""name"": ""setOwner"",
        ""outputs"": [],
        ""stateMutability"": ""nonpayable"",
        ""type"": ""function""
    }
]";

    // Address of the Uniswap V3 pool contract
    private const string POOL_CONTRACT_ADDRESS = "0xeEF1A9507B3D505f0062f2be9453981255b503c8";

    // Address of the Uniswap V3 pool factory contract
    private const string POOL_FACTORY_CONTRACT_ADDRESS = "0x1F98431c8aD98523631AE4a59f267346ea31F984";

    public static async Task<decimal> GetCurrentPoolPrice(Web3 web3)
    {
        var poolContract = web3.Eth.GetContract(UNISWAP_V3_POOL_ABI, POOL_CONTRACT_ADDRESS);
        var slot0Function = poolContract.GetFunction("slot0");

        var slot0 = await slot0Function.CallDeserializingToObjectAsync<Slot0>();

        // Calculate the price from sqrtPriceX96
        // The formula to convert sqrtPriceX96 to price is: (sqrtPriceX96 / 2^96)^2

        // BigInteger approach to maintain precision
        BigInteger sqrtPriceX96 = slot0.SqrtPriceX96;

        // Calculate sqrtPriceX96^2 as BigInteger
        BigInteger priceX192 = BigInteger.Multiply(sqrtPriceX96, sqrtPriceX96);

        // Shift right by 192 bits to get the final price in decimal form
        BigInteger denominator = BigInteger.Pow(2, 192);

        // Perform the division to get the final price as a decimal
        decimal price = (decimal)(priceX192 * 1_000_000 / denominator) / 10_000;

        return price;
    }

    [FunctionOutputAttribute]
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

    public static async Task<decimal> GetPriceOfTokenPair(Web3 web3, string token0Address, string token1Address)
    {
        // Assuming you have the ABI and pool address for the Uniswap V3 pool contract
        string poolAddress = await GetPoolAddress(web3, token0Address, token1Address);
        var poolContract = web3.Eth.GetContract(UniswapV3PoolABI, poolAddress);

        // Fetch the slot0 which contains the current price (sqrtPriceX96)
        var slot0Function = poolContract.GetFunction("slot0");
        var slot0 = await slot0Function.CallDeserializingToObjectAsync<Slot0Output>();

        // Calculate the price from sqrtPriceX96
        BigInteger sqrtPriceX96 = slot0.SqrtPriceX96;

        // Convert BigInteger to decimal for the calculation
        decimal sqrtPriceX96Decimal = (decimal)sqrtPriceX96;
        decimal price = (sqrtPriceX96Decimal * sqrtPriceX96Decimal) / (decimal)(BigInteger.Pow(2, 192));

        return price;
    }

    // This method assumes a function that retrieves the pool address based on the token pair
    private static async Task<string> GetPoolAddress(Web3 web3, string token0Address, string token1Address)
    {
        var factoryContract = web3.Eth.GetContract(UNISWAP_V3_POOL_FACTORY_ABI, POOL_FACTORY_CONTRACT_ADDRESS);
        var getPoolFunction = factoryContract.GetFunction("getPool");
        var poolAddress = await getPoolFunction.CallAsync<string>(token0Address, token1Address, 3000); // assuming fee tier of 0.3%
        return poolAddress;
    }

    // Placeholder for the ABI of Uniswap V3 pool and factory contracts
    private const string UniswapV3PoolABI = @"[...]"; // Replace with the actual ABI
    private const string UniswapV3FactoryABI = @"[...]"; // Replace with the actual ABI
    private const string UniswapV3FactoryAddress = "0x..."; // Replace with the actual Uniswap V3 factory address

    // Class to represent the Slot0 output
    public class Slot0Output
    {
        public BigInteger SqrtPriceX96 { get; set; }
        public int Tick { get; set; }
        // other fields as needed...
    }
}
