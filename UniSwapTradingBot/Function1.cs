using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Nethereum.Web3;
using Nethereum.Web3.Accounts;
using System.Threading;
using System.Numerics;
using UniSwapTradingBot.ContractHelpers;
using System.Collections.Generic;

namespace UniSwapTradingBot
{
    public static class Function1
    {
        public static class TradingBotOrchestration
        {
            [FunctionName("TradingBotOrchestration")]
            public static async Task RunOrchestrator(
                [OrchestrationTrigger] IDurableOrchestrationContext context)
            {
                // Call the trading bot logic function
                await context.CallActivityAsync("TradingBot_ExecuteTrade", null);

                // Recur with a delay (e.g., 1 minute)
                var nextRun = context.CurrentUtcDateTime.AddMinutes(1);
                await context.CreateTimer(nextRun, CancellationToken.None);
            }

            [FunctionName("TradingBot_ExecuteTrade")]
            public static async Task ExecuteTrade([ActivityTrigger] object input, ILogger log)
            {
                var url = Environment.GetEnvironmentVariable("ETH_NODE_URL");
                var privateKey = Environment.GetEnvironmentVariable("ETH_PRIVATE_KEY");
                var walletAddress = Environment.GetEnvironmentVariable("WALLET_ADDRESS");
                decimal upperTickerPercent = 10;
                decimal lowerTickerPercent = 10;
                decimal.TryParse(Environment.GetEnvironmentVariable("UPPER_TICKER_PERCENT"), out upperTickerPercent);
                decimal.TryParse(Environment.GetEnvironmentVariable("LOWER_TICKER_PERCENT"), out lowerTickerPercent);

                BigInteger positionId = BigInteger.Parse(Environment.GetEnvironmentVariable("INITIAL_POSITION_ID"));
                var account = new Account(privateKey);
                var web3 = new Web3(account, url);

                // Placeholder: Fetch current pool price and calculate thresholds
                var currentPrice = await UniswapV3PriceHelper.GetCurrentPoolPrice(web3);
                var minPriceThreshold = currentPrice - (currentPrice * upperTickerPercent);
                var maxPriceThreshold = currentPrice + (currentPrice * upperTickerPercent);

                // Retrieve position
                var position = await UniswapV3PositionHelper.GetPosition(web3, positionId);

                if (position.TickLower <= minPriceThreshold || position.TickUpper >= maxPriceThreshold)
                {
                    // 1. Remove all liquidity from the position. We will have to wait until the transaction is confirmed.
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;

                    await UniswapV3LiquidityHelper.RemoveLiquidity(web3, positionId, cancellationToken, log);

                    // 2. Calculate new position range.
                    var newTickLower = (int)(currentPrice - (currentPrice * lowerTickerPercent));
                    var newTickUpper = (int)(currentPrice + (currentPrice * upperTickerPercent));

                    // 3. Calculate the new liquidity amount based on the new tick range. This will be the amount of token 1 and token 2 to buy accounting for what I have in my wallet.
                    var token0Address = Environment.GetEnvironmentVariable("TOKEN0_ADDRESS");
                    var token1Address = Environment.GetEnvironmentVariable("TOKEN0_ADDRESS");
                    decimal availableToken0 = await TokenHelper.GetAvailableToken(web3, walletAddress, token0Address);
                    decimal availableToken1 = await TokenHelper.GetAvailableToken(web3, walletAddress, token1Address);
                    var (amount0, amount1) = await UniswapV3NewPositionValueHelper.CalculateAmountsForNewPosition(
                        web3, currentPrice, newTickLower, newTickUpper, availableToken0, availableToken1);

                    log.LogInformation($"Amount of Token0 to buy: {amount0}, Amount of Token1 to buy: {amount1}");

                    // 4. Buy token 1 and token 2 in preparation to fulfil the new position.
                    await ExecuteBuyTrade(web3, token0Address, amount0, log);
                    await ExecuteBuyTrade(web3, token1Address, amount1, log);

                    // 5. Create a new position with the new tick range using the Uniswap V3 pool contract.
                }

                log.LogInformation("Executed trade at: " + DateTime.UtcNow);

            }

            static async Task ExecuteBuyTrade(Web3 web3, string tokenAddress, decimal amount, ILogger log)
            {
                // Placeholder logic to interact with Uniswap V3 to perform the swap
                // This requires integrating with the Uniswap V3 router contract

                var routerAddress = Environment.GetEnvironmentVariable("UNISWAP_V3_ROUTER_ADDRESS");
                var account = web3.TransactionManager.Account.Address;

                var swapRouterService = new SwapRouterService(web3, routerAddress);

                // Define the swap parameters
                ulong deadline = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600; // 10 minutes from now
                var amountIn = Web3.Convert.ToWei(amount);
                var path = new List<string> { tokenAddress, Environment.GetEnvironmentVariable("WETH_ADDRESS") }; // Adjust the path as needed
                var to = account;

                // Swap function call
                var swapTxn = await swapRouterService.ExactInputSingle(new ExactInputSingleParams
                {
                    TokenIn = Environment.GetEnvironmentVariable("WETH_ADDRESS"),
                    TokenOut = tokenAddress,
                    Fee = 3000, // pool fee
                    Recipient = to,
                    Deadline = deadline,
                    AmountIn = (decimal)amountIn,
                    AmountOutMinimum = 1, // Setting this to 1 for simplicity, should be calculated based on slippage tolerance
                    SqrtPriceLimitX96 = 0 // No price limit
                });

                log.LogInformation($"Executed swap for {amount} of token at address {tokenAddress} with txn: {swapTxn}");
            }

            [FunctionName("TradingBot_HttpStart")]
            public static async Task<IActionResult> HttpStart(
                [HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req,
                [DurableClient] IDurableOrchestrationClient starter,
                ILogger log)
            {
                // Function input comes from the request content.
                string instanceId = await starter.StartNewAsync("TradingBotOrchestration", null);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                var payload = starter.CreateHttpManagementPayload(instanceId);
                return new OkObjectResult(new
                {
                    instanceId,
                    statusQueryGetUri = payload.StatusQueryGetUri,
                    sendEventPostUri = payload.SendEventPostUri,
                    terminatePostUri = payload.TerminatePostUri,
                    restartPostUri = payload.RestartPostUri
                });
            }
        }
    }
}
