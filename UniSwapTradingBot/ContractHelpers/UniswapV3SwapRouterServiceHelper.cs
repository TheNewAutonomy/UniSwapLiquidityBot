using System;
using System.Threading.Tasks;
using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System.Threading;

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
            var function = new ExactInputSingleFunction
            {
                TokenIn = swapParams.TokenIn,
                TokenOut = swapParams.TokenOut,
                Fee = swapParams.Fee,
                Recipient = swapParams.Recipient,
                Deadline = swapParams.Deadline,
                AmountIn = new HexBigInteger(Web3.Convert.ToWei(swapParams.AmountIn)),
                AmountOutMinimum = new HexBigInteger(Web3.Convert.ToWei(swapParams.AmountOutMinimum)),
                SqrtPriceLimitX96 = new HexBigInteger(Web3.Convert.ToWei(swapParams.SqrtPriceLimitX96))
            };

            var contract = _web3.Eth.GetContract(Abi, _routerAddress);
            var exactInputSingleFunction = contract.GetFunction<ExactInputSingleFunction>();

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

                return receipt.TransactionHash;
            }
            catch (Exception ex)
            {
                // Log the exception or handle it as needed
                Console.WriteLine($"Error executing ExactInputSingle: {ex.Message}");
                throw;
            }
        }

        // Uniswap V3 Router ABI - Partial ABI containing only the necessary parts for ExactInputSingle
        private const string Abi = @"[
            {
                ""inputs"": [
                    { ""internalType"": ""address"", ""name"": ""tokenIn"", ""type"": ""address"" },
                    { ""internalType"": ""address"", ""name"": ""tokenOut"", ""type"": ""address"" },
                    { ""internalType"": ""uint24"", ""name"": ""fee"", ""type"": ""uint24"" },
                    { ""internalType"": ""address"", ""name"": ""recipient"", ""type"": ""address"" },
                    { ""internalType"": ""uint256"", ""name"": ""deadline"", ""type"": ""uint256"" },
                    { ""internalType"": ""uint256"", ""name"": ""amountIn"", ""type"": ""uint256"" },
                    { ""internalType"": ""uint256"", ""name"": ""amountOutMinimum"", ""type"": ""uint256"" },
                    { ""internalType"": ""uint160"", ""name"": ""sqrtPriceLimitX96"", ""type"": ""uint160"" }
                ],
                ""name"": ""exactInputSingle"",
                ""outputs"": [
                    { ""internalType"": ""uint256"", ""name"": ""amountOut"", ""type"": ""uint256"" }
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
        public HexBigInteger AmountIn { get; set; }

        [Parameter("uint256", "amountOutMinimum", 7)]
        public HexBigInteger AmountOutMinimum { get; set; }

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
