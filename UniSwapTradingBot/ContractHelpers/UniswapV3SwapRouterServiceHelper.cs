﻿using System;
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
                // Fetch token decimals
                var tokenInDecimalPlaces = await TokenHelper.GetTokenDecimals(_web3, swapParams.TokenIn);
                var tokenOutDecimalPlaces = await TokenHelper.GetTokenDecimals(_web3, swapParams.TokenOut);

                // Convert amounts to wei using BigInteger
                var amountInWei = Web3.Convert.ToWei(swapParams.AmountIn, tokenInDecimalPlaces);
                var amountOutMinimumWei = Web3.Convert.ToWei(swapParams.AmountOutMinimum, tokenOutDecimalPlaces);

                // Check and set allowance
                var allowance = await TokenHelper.GetAllowance(_web3, swapParams.TokenIn, _web3.TransactionManager.Account.Address, _routerAddress);
                if (allowance < amountInWei)
                {
                    await TokenHelper.ApproveToken(_web3, swapParams.TokenIn, _routerAddress, amountInWei);
                }

                // Prepare function parameters without subtracting the fee
                var function = new ExactInputSingleFunction
                {
                    TokenIn = swapParams.TokenIn,
                    TokenOut = swapParams.TokenOut,
                    Fee = swapParams.Fee, // Pool's fee tier
                    Recipient = swapParams.Recipient,
                    Deadline = swapParams.Deadline,
                    AmountIn = amountInWei, // Use the full amount
                    AmountOutMinimum = amountOutMinimumWei,
                    SqrtPriceLimitX96 = BigInteger.Zero // If no price limit is needed
                };

                // Get the contract and function
                var contract = _web3.Eth.GetContract(Abi, _routerAddress);
                var exactInputSingleFunction = contract.GetFunction<ExactInputSingleFunction>();

                // Estimate gas
                var gasEstimate = await exactInputSingleFunction.EstimateGasAsync(function);

                // Send the transaction
                var transactionReceipt = await exactInputSingleFunction.SendTransactionAndWaitForReceiptAsync(function, new TransactionInput
                {
                    From = _web3.TransactionManager.Account.Address,
                    Gas = gasEstimate
                });

                return transactionReceipt.TransactionHash;
            }
            catch (SmartContractRevertException revertEx)
            {
                Console.WriteLine($"Transaction reverted: {revertEx.RevertMessage}");
                throw;
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
        public BigInteger AmountIn { get; set; }

        [Parameter("uint256", "amountOutMinimum", 7)]
        public BigInteger AmountOutMinimum { get; set; }

        [Parameter("uint160", "sqrtPriceLimitX96", 8)]
        public BigInteger SqrtPriceLimitX96 { get; set; }
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
