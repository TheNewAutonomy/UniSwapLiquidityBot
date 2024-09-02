using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System.Threading;
using System.Numerics;
using Nethereum.Model;

namespace UniSwapTradingBot.ContractHelpers
{
    public class SwapRouterService
    {
        private readonly Web3 _web3;
        private readonly string _routerAddress;

        public SwapRouterService(Web3 web3, string routerAddress)
        {
            _web3 = web3;
            _routerAddress = routerAddress;
        }

        public async Task<string> ExactInputSingle(ExactInputSingleParams swapParams)
        {
            try
            {
                var tokenInDecimalPlaces = await TokenHelper.GetTokenDecimals(_web3, swapParams.TokenIn);
                var tokenOutDecimalPlaces = await TokenHelper.GetTokenDecimals(_web3, swapParams.TokenOut);

                // Correct conversion from decimal to Wei using appropriate precision
                var amountInWei = Web3.Convert.ToWei(swapParams.AmountIn, tokenInDecimalPlaces);
                var amountOutMinimumWei = Web3.Convert.ToWei(swapParams.AmountOutMinimum, tokenOutDecimalPlaces);
                var sqrtPriceLimitX96Wei = new BigInteger(swapParams.SqrtPriceLimitX96);

                // Debug information to ensure values are correct
                Console.WriteLine($"AmountIn (Wei): {amountInWei}");
                Console.WriteLine($"AmountOutMinimum (Wei): {amountOutMinimumWei}");
                Console.WriteLine($"SqrtPriceLimitX96 (Wei): {sqrtPriceLimitX96Wei}");

                var function = new ExactInputSingleFunction
                {
                    TokenIn = swapParams.TokenIn,
                    TokenOut = swapParams.TokenOut,
                    Fee = swapParams.Fee,
                    Recipient = swapParams.Recipient,
                    Deadline = swapParams.Deadline,
                    AmountIn = (ulong)swapParams.AmountIn,
                    AmountOutMinimum = (ulong)swapParams.AmountOutMinimum,
                    SqrtPriceLimitX96 = new HexBigInteger(sqrtPriceLimitX96Wei)
                };

                var contract = _web3.Eth.GetContract(Abi, _routerAddress);
                var exactInputSingleFunction = contract.GetFunction<ExactInputSingleFunction>();

                string transactionHash = null;
                try
                {
                    var gas = await exactInputSingleFunction.EstimateGasAsync(function, _web3.TransactionManager.Account.Address, null, null);
                    var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

                    var receipt = await exactInputSingleFunction.SendTransactionAndWaitForReceiptAsync(
                        function,
                        new TransactionInput
                        {
                            From = _web3.TransactionManager.Account.Address,
                            To = _routerAddress,
                            Gas = gas,
                            GasPrice = gasPrice,
                            Value = new HexBigInteger(0) // Assuming no ETH is being sent
                        }
                    );

                    transactionHash = receipt.TransactionHash;

                }
                catch (Exception ex)
                {
                    var x = ex.Message;
                    throw;
                }
                return transactionHash;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing ExactInputSingle: {ex.Message}");
                throw;
            }
        }

        // Uniswap V3 Router ABI - Partial ABI containing only the necessary parts for ExactInputSingle
        private const string Abi = @"[
            {
                ""inputs"": [
                    { ""internalType"": ""address"", ""name"": ""_factory"", ""type"": ""address"" },
                    { ""internalType"": ""address"", ""name"": ""_WETH9"", ""type"": ""address"" }
                ],
                ""stateMutability"": ""nonpayable"",
                ""type"": ""constructor""
            },
            {
                ""inputs"": [],
                ""name"": ""WETH9"",
                ""outputs"": [
                    { ""internalType"": ""address"", ""name"": """", ""type"": ""address"" }
                ],
                ""stateMutability"": ""view"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""components"": [
                            { ""internalType"": ""bytes"", ""name"": ""path"", ""type"": ""bytes"" },
                            { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
                            { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountIn"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountOutMinimum"", ""type"": ""uint256"" }
                        ],
                        ""internalType"": ""struct ISwapRouter.ExactInputParams"",
                        ""name"": ""params"",
                        ""type"": ""tuple""
                    }
                ],
                ""name"": ""exactInput"",
                ""outputs"": [
                    { ""internalType"": ""uint256"", ""name"": ""amountOut"", ""type"": ""uint256"" }
                ],
                ""stateMutability"": ""payable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""components"": [
                            { ""internalType"": ""address"", ""name"": ""tokenIn"", ""type"": ""address"" },
                            { ""internalType"": ""address"", ""name"": ""tokenOut"", ""type"": ""address"" },
                            { ""internalType"": ""uint24"", ""name"": ""fee"", ""type"": ""uint24"" },
                            { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
                            { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountIn"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountOutMinimum"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint160"", ""name"": ""sqrtPriceLimitX96"", ""type"": ""uint160"" }
                        ],
                        ""internalType"": ""struct ISwapRouter.ExactInputSingleParams"",
                        ""name"": ""params"",
                        ""type"": ""tuple""
                    }
                ],
                ""name"": ""exactInputSingle"",
                ""outputs"": [
                    { ""internalType"": ""uint256"", ""name"": ""amountOut"", ""type"": ""uint256"" }
                ],
                ""stateMutability"": ""payable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""components"": [
                            { ""internalType"": ""bytes"", ""name"": ""path"", ""type"": ""bytes"" },
                            { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
                            { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountOut"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountInMaximum"", ""type"": ""uint256"" }
                        ],
                        ""internalType"": ""struct ISwapRouter.ExactOutputParams"",
                        ""name"": ""params"",
                        ""type"": ""tuple""
                    }
                ],
                ""name"": ""exactOutput"",
                ""outputs"": [
                    { ""internalType"": ""uint256"", ""name"": ""amountIn"", ""type"": ""uint256"" }
                ],
                ""stateMutability"": ""payable"",
                ""type"": ""function""
            },
            {
                ""inputs"": [
                    {
                        ""components"": [
                            { ""internalType"": ""address"", ""name"": ""tokenIn"", ""type"": ""address"" },
                            { ""internalType"": ""address"", ""name"": ""tokenOut"", ""type"": ""address"" },
                            { ""internalType"": ""uint24"", ""name"": ""fee"", ""type"": ""uint24"" },
                            { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
                            { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountOut"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint256"", ""name"": ""amountInMaximum"", ""type"": ""uint256"" },
                            { ""internalType"": ""uint160"", ""name"": ""sqrtPriceLimitX96"", ""type"": ""uint160"" }
                        ],
                        ""internalType"": ""struct ISwapRouter.ExactOutputSingleParams"",
                        ""name"": ""params"",
                        ""type"": ""tuple""
                    }
                ],
                ""name"": ""exactOutputSingle"",
                ""outputs"": [
                    { ""internalType"": ""uint256"", ""name"": ""amountIn"", ""type"": ""uint256"" }
                ],
                ""stateMutability"": ""payable"",
                ""type"": ""function""
            }
            // Add other functions in the same format here...
        ]";
    }

    [Function("exactInputSingle", "uint256")]
    public class ExactInputSingleFunction : FunctionMessage
    {
        [Parameter("address", "tokenIn", 1)]
        public string TokenIn { get; set; }

        [Parameter("address", "tokenOut", 2)]
        public string TokenOut { get; set; }

        [Parameter("uint24", "fee", 3)]
        public uint Fee { get; set; }

        [Parameter("address", "recipient", 4)]
        public string Recipient { get; set; }

        [Parameter("uint256", "deadline", 5)]
        public ulong Deadline { get; set; }

        [Parameter("uint256", "amountIn", 6)]
        public ulong AmountIn { get; set; }

        [Parameter("uint256", "amountOutMinimum", 7)]
        public ulong AmountOutMinimum { get; set; }

        [Parameter("uint160", "sqrtPriceLimitX96", 8)]
        public HexBigInteger SqrtPriceLimitX96 { get; set; }
    }

    public class ExactInputSingleParams
    {
        public string TokenIn { get; set; }
        public string TokenOut { get; set; }
        public uint Fee { get; set; }
        public string Recipient { get; set; }
        public ulong Deadline { get; set; }
        public decimal AmountIn { get; set; }
        public decimal AmountOutMinimum { get; set; }
        public decimal SqrtPriceLimitX96 { get; set; }
    }
}
