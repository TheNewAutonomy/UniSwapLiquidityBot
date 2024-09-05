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
using Google.Protobuf.WellKnownTypes;
using Nethereum.ABI.FunctionEncoding;

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

                var amountInWei = Web3.Convert.ToWei(swapParams.AmountIn, tokenInDecimalPlaces);
                var amountOutMinimumWei = Web3.Convert.ToWei(swapParams.AmountOutMinimum, tokenOutDecimalPlaces);
                var sqrtPriceLimitX96Wei = new BigInteger(swapParams.SqrtPriceLimitX96);

                var tokenAddressFromProxy = TokenHelper.getTokenFromProxy(swapParams.TokenIn);

                // Check current allowance of the token
                var allowance = await TokenHelper.GetAllowance(_web3, tokenAddressFromProxy, _web3.TransactionManager.Account.Address, _routerAddress);
                if (allowance < amountInWei)
                {
                    Console.WriteLine("Allowance is insufficient, approving the router...");
                    // Approve the router to spend the required amount
                    await TokenHelper.ApproveToken(_web3, tokenAddressFromProxy, _routerAddress, amountInWei);
                }

                var function = new ExactInputSingleFunction
                {
                    TokenIn = swapParams.TokenIn,
                    TokenOut = swapParams.TokenOut,
                    Fee = swapParams.Fee,
                    Recipient = swapParams.Recipient,
                    Deadline = swapParams.Deadline,
                    AmountIn = (ulong)swapParams.AmountIn,
                    AmountOutMinimum = (ulong)swapParams.AmountOutMinimum,
                    SqrtPriceLimitX96 = (ulong)sqrtPriceLimitX96Wei
                };

                var contract = _web3.Eth.GetContract(Abi, _routerAddress);
                var exactInputSingleFunction = contract.GetFunction<ExactInputSingleFunction>();

                // Estimating the gas required for the transaction
                var gasEstimate = await exactInputSingleFunction.EstimateGasAsync(
                    function,
                    new CallInput
                    {
                        From = _web3.TransactionManager.Account.Address,
                        To = _routerAddress,
                        Value = new HexBigInteger(0),
                        Data = exactInputSingleFunction.GetData(function)
                    }
                );

                // Adding a buffer to gas estimate to ensure transaction execution
                var gasBuffer = new HexBigInteger(gasEstimate.Value * 110 / 100); // 10% buffer

                // Sending the transaction
                var transactionHash = await exactInputSingleFunction.SendTransactionAndWaitForReceiptAsync(
                    function,
                    new TransactionInput
                    {
                        From = _web3.TransactionManager.Account.Address,
                        To = _routerAddress,
                        Gas = gasBuffer, // Using estimated gas with buffer
                        Value = null
                    }
                );

                return transactionHash.TransactionHash;
            }
            catch (SmartContractRevertException revertEx)
            {
                // This exception type is specific to reverts and might provide revert reason
                Console.WriteLine($"Transaction reverted: {revertEx.RevertMessage}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error executing ExactInputSingle: {ex.Message}");
                // More details can be logged here if needed
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
        public ulong SqrtPriceLimitX96 { get; set; }
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
