using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using System;
using System.Numerics;
using System.Threading.Tasks;
using Nethereum.Util;
using Nethereum.Model;
using UniSwapTradingBot.ContractHelpers;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.Standards.ERC20.TokenList;

public class UniSwapV3SwapRouter
{
    private readonly Web3 _web3;
    private readonly string _routerAddress;

    private const string SwapRouterAbi = @"[
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

    public UniSwapV3SwapRouter(Web3 web3, string routerAddress)
    {
        _web3 = web3;
        _routerAddress = routerAddress;
    }

    public async Task<string> SwapExactInputSingleAsync(
    string tokenIn, string tokenOut, int fee, BigInteger amountIn, BigInteger amountOutMinimum,
    string recipient, BigInteger deadline)
    {
        string transactionHash = string.Empty;

        try
        {
            // Load the contract using ABI and Router Address
            var contract = _web3.Eth.GetContract(SwapRouterAbi, _routerAddress);
            var exactInputSingleFunction = contract.GetFunction("exactInputSingle");

            // Create input data for the exactInputSingle function
            var parameters = new ExactInputSingleParams
            {
                TokenIn = tokenIn,
                TokenOut = tokenOut,
                Fee = (uint)fee,
                Recipient = recipient,
                Deadline = (ulong)deadline,
                AmountIn = amountIn,
                AmountOutMinimum = 0,
                SqrtPriceLimitX96 = BigInteger.Zero  // No price limit
            };

            // Encode the parameters into transaction input data
            var transactionInputData = exactInputSingleFunction.GetData(parameters);

            // Get the transaction count (nonce)
            var nonce = await _web3.Eth.Transactions.GetTransactionCount.SendRequestAsync(
                _web3.TransactionManager.Account.Address, BlockParameter.CreatePending());

            // Fetch the current gas price dynamically
            var gasPrice = await _web3.Eth.GasPrice.SendRequestAsync();

            // Try estimating gas for the transaction
            HexBigInteger gasEstimate;
            try
            {
                // Check current allowance of the token
                var allowance = await TokenHelper.GetAllowance(_web3, tokenIn, _web3.TransactionManager.Account.Address, _routerAddress);
                if (allowance < amountIn)
                {
                    // Approve the router to spend the required amount
                    await TokenHelper.ApproveToken(_web3, tokenIn, _routerAddress, amountIn);
                }

                gasEstimate = await exactInputSingleFunction.EstimateGasAsync(transactionInputData);
            }
            catch (Exception ex)
            {
                // Fallback to a manual gas estimate if dynamic estimation fails
                Console.WriteLine($"Gas estimation failed: {ex.Message}. Using fallback gas limit.");
                gasEstimate = new HexBigInteger(500000);  // Set a fallback gas limit (adjust as needed)
            }

            // Create a transaction object
            var transaction = new TransactionInput
            {
                From = _web3.TransactionManager.Account.Address,
                To = _routerAddress,
                Data = transactionInputData,
                Gas = gasEstimate,
                GasPrice = gasPrice,
                Nonce = new HexBigInteger(nonce),
                Value = new HexBigInteger(0)  // Adjust if ETH is involved
            };

            // Sign the transaction
            var signedTransaction = await _web3.TransactionManager.Account.TransactionManager.SignTransactionAsync(transaction);

            // Send the signed transaction
            transactionHash = await _web3.Eth.Transactions.SendRawTransaction.SendRequestAsync(signedTransaction);

            Console.WriteLine($"Transaction sent successfully. Hash: {transactionHash}");
        }
        catch (Exception ex)
        {
            // Improved error logging
            Console.WriteLine($"Error during SwapExactInputSingleAsync: {ex.Message}");
        }

        return transactionHash;
    }

}

public class ExactInputSingleParams
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
    public BigInteger Deadline { get; set; }

    [Parameter("uint256", "amountIn", 6)]
    public BigInteger AmountIn { get; set; }

    [Parameter("uint256", "amountOutMinimum", 7)]
    public BigInteger AmountOutMinimum { get; set; }

    [Parameter("uint160", "sqrtPriceLimitX96", 8)]
    public BigInteger SqrtPriceLimitX96 { get; set; }
}
