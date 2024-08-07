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
            static decimal UpperTickerPercent = 10;
            static decimal LowerTickerPercent = 10;
            static string WalletAddress = string.Empty;
            static string Token0Address = string.Empty;
            static string Token1Address = string.Empty;
            static string UniswapV3RouterAddress = string.Empty;
            static string UniswapV3PositionManagerAddress = string.Empty;
            static string UniswapV3FactoryAddress = string.Empty;
            static string WethAddress = string.Empty;
            static string EthNodeUrl = string.Empty;
            static string PrivateKey = string.Empty;
            static BigInteger PositionId = 0;
           
            [FunctionName("TradingBotOrchestration")]
            public static async Task RunOrchestrator(
                [OrchestrationTrigger] IDurableOrchestrationContext context)
            {
                var envVariables = await context.CallActivityAsync<Dictionary<string, string>>("GetEnvironmentVariables", null);

                EthNodeUrl = envVariables["ETH_NODE_URL"];
                PrivateKey = envVariables["ETH_PRIVATE_KEY"];
                WalletAddress = envVariables["WALLET_ADDRESS"];
                Token0Address = envVariables["TOKEN0_ADDRESS"];
                Token1Address = envVariables["TOKEN1_ADDRESS"];
                WethAddress = envVariables["WETH_ADDRESS"];
                UniswapV3RouterAddress = envVariables["UNISWAP_V3_ROUTER_ADDRESS"];
                decimal.TryParse(envVariables["UPPER_TICKER_PERCENT"], out UpperTickerPercent);
                decimal.TryParse(envVariables["LOWER_TICKER_PERCENT"], out LowerTickerPercent);
                PositionId = BigInteger.Parse(envVariables["INITIAL_POSITION_ID"]);

                // Call the trading bot logic function
                await context.CallActivityAsync("TradingBot_ExecuteTrade", null);

                // Recur with a delay (e.g., 1 minute)
                var nextRun = context.CurrentUtcDateTime.AddMinutes(1);
                await context.CreateTimer(nextRun, CancellationToken.None);
            }

            [FunctionName("TradingBot_ExecuteTrade")]
            public static async Task ExecuteTrade([ActivityTrigger] object input, ILogger log)
            {
                var account = new Account(PrivateKey);
                var web3 = new Web3(account, EthNodeUrl);

                // Placeholder: Fetch current pool price and calculate thresholds
                var currentPrice = await UniswapV3PriceHelper.GetCurrentPoolPrice(web3);
                var minPriceThreshold = currentPrice - (currentPrice * LowerTickerPercent);
                var maxPriceThreshold = currentPrice + (currentPrice * UpperTickerPercent);

                // Retrieve position
                var position = await UniswapV3PositionHelper.GetPosition(web3, PositionId);

                if (position.TickLower <= minPriceThreshold || position.TickUpper >= maxPriceThreshold)
                {
                    // 1. Remove all liquidity from the position. We will have to wait until the transaction is confirmed.
                    var cancellationTokenSource = new CancellationTokenSource();
                    var cancellationToken = cancellationTokenSource.Token;

                    await UniswapV3LiquidityHelper.RemoveLiquidity(web3, PositionId, cancellationToken, log);

                    // 2. Calculate new position range.
                    var newTickLower = (int)(currentPrice - (currentPrice * LowerTickerPercent));
                    var newTickUpper = (int)(currentPrice + (currentPrice * UpperTickerPercent));

                    // 3. Calculate the new liquidity amount based on the new tick range. This will be the amount of token 1 and token 2 to buy accounting for what I have in my wallet.
                    
                    decimal availableToken0 = await TokenHelper.GetAvailableToken(web3, WalletAddress, Token0Address);
                    decimal availableToken1 = await TokenHelper.GetAvailableToken(web3, WalletAddress, Token1Address);
                    var (amount0, amount1) = await UniswapV3NewPositionValueHelper.CalculateAmountsForNewPosition(
                        web3, currentPrice, newTickLower, newTickUpper, availableToken0, availableToken1);

                    log.LogInformation($"Amount of Token0 to buy: {amount0}, Amount of Token1 to buy: {amount1}");

                    // 4. Buy token 1 and token 2 in preparation to fulfil the new position.
                    await ExecuteBuyTrade(web3, Token0Address, amount0, log);
                    await ExecuteBuyTrade(web3, Token1Address, amount1, log);

                    // 5. Create a new position with the new tick range using the Uniswap V3 pool contract.
                    await CreateNewPosition(web3, newTickLower, newTickUpper, amount0, amount1, log).ConfigureAwait(false);
                }

                log.LogInformation("Executed trade at: " + DateTime.UtcNow);

            }

            static async Task ExecuteBuyTrade(Web3 web3, string tokenAddress, decimal amount, ILogger log)
            {
                // Placeholder logic to interact with Uniswap V3 to perform the swap
                // This requires integrating with the Uniswap V3 router contract

                var routerAddress = UniswapV3RouterAddress;
                var account = web3.TransactionManager.Account.Address;

                var swapRouterService = new SwapRouterService(web3, routerAddress);

                // Define the swap parameters
                ulong deadline = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600; // 10 minutes from now
                var amountIn = Web3.Convert.ToWei(amount);
                var path = new List<string> { tokenAddress, WethAddress }; // Adjust the path as needed
                var to = account;

                // Swap function call
                var swapTxn = await swapRouterService.ExactInputSingle(new ExactInputSingleParams
                {
                    TokenIn = WethAddress,
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

            private static async Task CreateNewPosition(Web3 web3, int tickLower, int tickUpper, decimal amount0, decimal amount1, ILogger log)
            {
                try
                {
                    var positionManagerService = new NonfungiblePositionManagerService(web3, UniswapV3RouterAddress);

                    var tokenId = await positionManagerService.MintPositionAsync(new MintPositionParams
                    {
                        Token0 = Token0Address,
                        Token1 = Token1Address,
                        Fee = 3000, // pool fee
                        TickLower = tickLower,
                        TickUpper = tickUpper,
                        Amount0Desired = Web3.Convert.ToWei(amount0),
                        Amount1Desired = Web3.Convert.ToWei(amount1),
                        Amount0Min = 1, // Setting this to 1 for simplicity, should be calculated based on slippage tolerance
                        Amount1Min = 1, // Setting this to 1 for simplicity, should be calculated based on slippage tolerance
                        Recipient = WalletAddress,
                        Deadline = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600 // 10 minutes from now
                    }).ConfigureAwait(false);

                    log.LogInformation($"Created new position with Token ID: {tokenId}");
                }
                catch (Exception ex)
                {
                    log.LogError($"Failed to create new position: {ex.Message}");
                    throw;
                }
            }

            [FunctionName("GetEnvironmentVariables")]
            public static Dictionary<string, string> GetEnvironmentVariables([ActivityTrigger] object input)
            {
                return new Dictionary<string, string>
                {
                    { "ETH_NODE_URL", Environment.GetEnvironmentVariable("ETH_NODE_URL") },
                    { "ETH_PRIVATE_KEY", Environment.GetEnvironmentVariable("ETH_PRIVATE_KEY") },
                    { "WALLET_ADDRESS", Environment.GetEnvironmentVariable("WALLET_ADDRESS") },
                    { "TOKEN0_ADDRESS", Environment.GetEnvironmentVariable("TOKEN0_ADDRESS") },
                    { "TOKEN1_ADDRESS", Environment.GetEnvironmentVariable("TOKEN1_ADDRESS") },
                    { "WETH_ADDRESS", Environment.GetEnvironmentVariable("WETH_ADDRESS") },
                    { "UNISWAP_V3_ROUTER_ADDRESS", Environment.GetEnvironmentVariable("UNISWAP_V3_ROUTER_ADDRESS") },
                    { "UPPER_TICKER_PERCENT", Environment.GetEnvironmentVariable("UPPER_TICKER_PERCENT") },
                    { "LOWER_TICKER_PERCENT", Environment.GetEnvironmentVariable("LOWER_TICKER_PERCENT") },
                    { "INITIAL_POSITION_ID", Environment.GetEnvironmentVariable("INITIAL_POSITION_ID") }
                };
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
