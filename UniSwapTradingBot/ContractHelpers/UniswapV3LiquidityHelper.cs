using Nethereum.Web3;
using Nethereum.Contracts;
using Nethereum.ABI.FunctionEncoding.Attributes;
using System.Numerics;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;

namespace UniSwapTradingBot.ContractHelpers
{
    public static class UniswapV3LiquidityHelper
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

        public static async Task RemoveLiquidity(Web3 web3, BigInteger positionId, CancellationToken cancellationToken, ILogger log)
        {
            const string UniswapV3NFTPositionManagerAddress = "0xC36442b4a4522E871399CD717aBDD847Ab11FE88";

            var contract = web3.Eth.GetContract(NONFUNGIBLE_POSITION_MANAGER_ABI, UniswapV3NFTPositionManagerAddress);
            var burnFunction = contract.GetFunction("burn");

            // Define the parameters for the burn function
            var parameters = new
            {
                tokenId = positionId
            };

            try
            {
                // Send the transaction to burn liquidity
                var receipt = await burnFunction.SendTransactionAndWaitForReceiptAsync(web3.TransactionManager.Account.Address, cancellationToken, parameters);

                if (receipt.Status.Value == 1)
                {
                    log.LogInformation($"Successfully removed liquidity for position {positionId}.");
                }
                else
                {
                    log.LogError($"Failed to remove liquidity for position {positionId}. Transaction status: {receipt.Status.Value}");
                }
            }
            catch (Exception ex)
            {
                log.LogError($"An error occurred while removing liquidity for position {positionId}: {ex.Message}");
            }
        }
    }
}
