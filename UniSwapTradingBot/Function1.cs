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
using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json.Linq;
using static UniswapV3PositionHelper;
using Nethereum.Contracts.Standards.ERC20.TokenList;
using Azure.Core.GeoJson;
using UniSwapTradingBot.Utilities;
using Nethereum.Util;

namespace UniSwapTradingBot
{
    public static class TradingBotOrchestration
    {
        static decimal UpperTickerPercent = 10;
        static decimal LowerTickerPercent = 10;
        static string WalletAddress = string.Empty;
        static string Token0ProxyAddress = string.Empty;
        static string Token1ProxyAddress = string.Empty;
        static string UniswapV3RouterAddress = string.Empty;
        static string UniswapV3PositionManagerAddress = string.Empty;
        static string UniswapV3FactoryAddress = string.Empty;
        static string WethAddress = string.Empty;
        static string EthNodeUrl = string.Empty;
        static string PrivateKey = string.Empty;
        static string TheGraphKey = string.Empty;

        [FunctionName("TradingBotOrchestration")]
        public static async Task RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var envVariables = await context.CallActivityAsync<Dictionary<string, string>>("GetEnvironmentVariables", null);

            EthNodeUrl = envVariables["ETH_NODE_URL"];
            PrivateKey = envVariables["ETH_PRIVATE_KEY"];
            WalletAddress = envVariables["WALLET_ADDRESS"];
            Token0ProxyAddress = envVariables["TOKEN0_PROXY_ADDRESS"];
            Token1ProxyAddress = envVariables["TOKEN1_PROXY_ADDRESS"];
            WethAddress = envVariables["WETH_ADDRESS"];
            UniswapV3RouterAddress = envVariables["UNISWAP_V3_ROUTER_ADDRESS"];
            TheGraphKey = envVariables["THE_GRAPH_KEY"];
            decimal.TryParse(envVariables["UPPER_TICKER_PERCENT"], out UpperTickerPercent);
            decimal.TryParse(envVariables["LOWER_TICKER_PERCENT"], out LowerTickerPercent);

            // Call the trading bot logic function
            await context.CallActivityAsync("TradingBot_ExecuteTrade", null);

            // Recur with a delay (e.g., 1 minute)
            var nextRun = context.CurrentUtcDateTime.AddMinutes(1);
            await context.CreateTimer(nextRun, CancellationToken.None);
        }

        /*
         * For a given token pair, the trading bot will:
         * 1. Fetch the current pool price and calculate thresholds
         * 2. Get a list of all positions for the wallet address that have liquidity and use a given token pair
         * 3. Any positions that are outside the price thresholds will have their liquidity removed
         * 4. Using the liquidity in my wallet, calculate a new optimal position range
         * 5. Calculate the new liquidity amount based on the new tick range. This will be the amount of token 1 and token 2 to buy accounting for what I have in my wallet
         * 6. Buy the required token amount to create the position, if I have more than a minimum threshold of liquidity in my wallet, and then create a new position
         */
        [FunctionName("TradingBot_ExecuteTrade")]
        public static async Task ExecuteTrade([ActivityTrigger] object input, ILogger log)
        {
            var account = new Account(PrivateKey);
            var web3 = new Web3(account, EthNodeUrl);

            // 1. Fetch current pool price
            var currentPrice = await UniswapV3PriceHelper.GetCurrentPoolPrice(web3);

            // 2. Get a list of all positions for the wallet address that have liquidity and use a given token pair
            var positions = Subgraph.GetPositions(walletAddress: WalletAddress, Token0ProxyAddress, Token1ProxyAddress, TheGraphKey);

            // 3. Any positions that are outside the price thresholds will have their liquidity removed
            foreach (var position in positions.Result)
            {
                decimal lowerPrice = currentPrice * (1 - LowerTickerPercent / 100);
                decimal upperPrice = currentPrice * (1 + UpperTickerPercent / 100);

                var newTickRange = CalculateTicks(currentPrice, LowerTickerPercent, UpperTickerPercent);

                if (position.TickLower <= newTickRange.newTickLower || position.TickUpper >= newTickRange.newTickUpper)
                {
                    // Remove all liquidity from the position
                    var liquidityRemover = new LiquidityRemover(web3, log);
                    CancellationTokenSource cts = new CancellationTokenSource();

                    try
                    {
                        await liquidityRemover.RemoveLiquidityAsync(PrivateKey, account, position.Id, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        log.LogWarning("Operation was canceled.");
                    }
                    catch (Exception ex)
                    {
                        log.LogError($"Failed to remove liquidity: {ex.Message}");
                        throw;
                    }
                }
            }

            // 4. Calculate newTicks based on the current pool price and percentage range, get the amount of tokens in my wallet and their decimal places
            var newTicks = CalculateTicks(currentPrice, LowerTickerPercent, UpperTickerPercent);

            decimal availableToken0 = await TokenHelper.GetAvailableTokenBalance(web3, Token0ProxyAddress, Token0ProxyAddress, WalletAddress);
            decimal availableToken1 = await TokenHelper.GetAvailableTokenBalance(web3, Token1ProxyAddress, Token1ProxyAddress, WalletAddress);

            var token0Decimals = await TokenHelper.GetTokenDecimals(web3, Token0ProxyAddress);
            var token1Decimals = await TokenHelper.GetTokenDecimals(web3, Token1ProxyAddress);

            // 5. Calculate an optimal position and from that, how many tokens to swap to reach that position
            var result = await UniswapV3NewPositionValueHelper.CalculateOptimalSwapForNewPosition(web3, currentPrice, newTicks.newTickLower, newTicks.newTickUpper, availableToken0, availableToken1, token0Decimals, token1Decimals, Token0ProxyAddress, Token1ProxyAddress);

            if (result.amountToSell > 0)
            {
                try
                {
                    // 6. Buy the required token amount to create the position.
                    await ExecuteBuyTrade(web3, result.tokenToBuy, result.amountToBuy, result.tokenToSell, result.amountToSell, currentPrice, log);

                    // 7. Recalculate newTocks and ranges based on what's in my wallet since the trade may have introduced some small rounding errors.
                    availableToken0 = await TokenHelper.GetAvailableTokenBalance(web3, Token0ProxyAddress, Token0ProxyAddress, WalletAddress);
                    availableToken1 = await TokenHelper.GetAvailableTokenBalance(web3, Token1ProxyAddress, Token1ProxyAddress, WalletAddress);
                    currentPrice = await UniswapV3PriceHelper.GetCurrentPoolPrice(web3);
                    newTicks = CalculateTicks(currentPrice, LowerTickerPercent, UpperTickerPercent);

                    decimal token0ScaleFactor = (decimal)Math.Pow(10, token0Decimals);
                    var token0BigInteger = new BigInteger(availableToken0 * token0ScaleFactor);

                    decimal token1ScaleFactor = (decimal)Math.Pow(10, token1Decimals);
                    var token1BigInteger = new BigInteger(availableToken1 * token1ScaleFactor);

                    // 8. Create a new position
                    string tokenHash = await CreateNewPosition(web3, newTicks.newTickLower, newTicks.newTickUpper, token0BigInteger, token1BigInteger, log).ConfigureAwait(false);
                    log.LogInformation($"Created new position with Hash: {tokenHash}");
                }
                catch (OperationCanceledException)
                {
                    log.LogWarning("Operation was canceled.");
                }
                catch (Exception ex)
                {
                    log.LogError($"Failed to execute trade: {ex.Message}");
                    throw;
                }

                log.LogInformation("Executed trade at: " + DateTime.UtcNow);
            }
        }

        static int GetTickFromPrice(decimal price)
        {
            // Log base sqrt(1.0001) is approximately 0.0001 / 2
            double logBase = Math.Log(1.0001) / 2.0;
            return (int)(Math.Log((double)price) / logBase);
        }

        static (int newTickLower, int newTickUpper) CalculateTicks(decimal currentPrice, decimal lowerTickerPercent, decimal upperTickerPercent)
        {
            // Calculate the price range for the lower and upper bounds
            decimal lowerPrice = currentPrice * (1 - lowerTickerPercent / 100);
            decimal upperPrice = currentPrice * (1 + upperTickerPercent / 100);

            // Convert prices to tick values
            int newTickLower = GetTickFromPrice(lowerPrice);
            int newTickUpper = GetTickFromPrice(upperPrice);

            return (newTickLower, newTickUpper);
        }

        static async Task ExecuteBuyTrade(Web3 web3, string tokenToBuyAddress, BigInteger amountToBuy, string tokenToSellAddress, BigInteger amountToSell, decimal currentPrice, ILogger log)
        {
            try
            {
                var routerAddress = UniswapV3RouterAddress;
                var swapRouterService = new SwapRouterService(web3, routerAddress);

                // Define the swap parameters
                ulong deadline = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600; // 10 minutes from now

                // Execute the swap
                var swapRouter = new UniSwapV3SwapRouter(
                   web3: web3,
                   routerAddress: routerAddress
               );

                var txHash = await swapRouter.SwapExactInputSingleAsync(
                    tokenIn: tokenToSellAddress,
                    tokenOut: tokenToBuyAddress,
                    fee: 500, // Example fee tier
                    amountIn: amountToSell,
                    amountOutMinimum: amountToBuy,
                    recipient: web3.TransactionManager.Account.Address,
                    deadline: deadline // new BigInteger(DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600)  // Deadline 10 minutes from now
                );

                Console.WriteLine("Transaction Hash: " + txHash);
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to execute swap: {ex.Message}");
                throw;
            }
        }

        private static async Task<string> CreateNewPosition(Web3 web3, int tickLower, int tickUpper, BigInteger amount0, BigInteger amount1, ILogger log)
        {
            string tokenHash = string.Empty;

            try
            {
                var positionManagerService = new NonfungiblePositionManagerService(web3, UniswapV3RouterAddress);

                // Calculate slippage tolerance (e.g., 2% slippage for both tokens)
                BigInteger amount0Min = amount0 * 98 / 100; // Allow 2% slippage
                BigInteger amount1Min = amount1 * 98 / 100; // Allow 2% slippage

                tokenHash = await positionManagerService.MintPositionAsync(new MintPositionParams
                {
                    Token0 = Token0ProxyAddress,
                    Token1 = Token1ProxyAddress,
                    Fee = 500, // pool fee
                    TickLower = tickLower,
                    TickUpper = tickUpper,
                    Amount0Desired = amount0,
                    Amount1Desired = amount1,
                    Amount0Min = 0, // Will be set to amount0 * 0.98 to allow 2% slippage
                    Amount1Min = 0, // Will be set to amount1 * 0.98 to allow 2% slippage
                    Recipient = WalletAddress,
                    Deadline = (ulong)DateTimeOffset.UtcNow.ToUnixTimeSeconds() + 600 // 10 minutes from now
                }).ConfigureAwait(false);

                log.LogInformation($"Created new position with Token ID: {tokenHash}");
            }
            catch (Exception ex)
            {
                log.LogError($"Failed to create new position: {ex.Message}");
                throw;
            }

            return tokenHash;
        }

        [FunctionName("GetEnvironmentVariables")]
        public static Dictionary<string, string> GetEnvironmentVariables([ActivityTrigger] object input)
        {
            return new Dictionary<string, string>
                {
                    { "ETH_NODE_URL", Environment.GetEnvironmentVariable("ETH_NODE_URL") },
                    { "ETH_PRIVATE_KEY", Environment.GetEnvironmentVariable("ETH_PRIVATE_KEY") },
                    { "WALLET_ADDRESS", Environment.GetEnvironmentVariable("WALLET_ADDRESS") },
                    { "TOKEN0_PROXY_ADDRESS", Environment.GetEnvironmentVariable("TOKEN0_PROXY_ADDRESS") },
                    { "TOKEN1_PROXY_ADDRESS", Environment.GetEnvironmentVariable("TOKEN1_PROXY_ADDRESS") },
                    { "WETH_ADDRESS", Environment.GetEnvironmentVariable("WETH_ADDRESS") },
                    { "UNISWAP_V3_ROUTER_ADDRESS", Environment.GetEnvironmentVariable("UNISWAP_V3_ROUTER_ADDRESS") },
                    { "UPPER_TICKER_PERCENT", Environment.GetEnvironmentVariable("UPPER_TICKER_PERCENT") },
                    { "LOWER_TICKER_PERCENT", Environment.GetEnvironmentVariable("LOWER_TICKER_PERCENT") },
                    { "INITIAL_POSITION_ID", Environment.GetEnvironmentVariable("INITIAL_POSITION_ID") },
                    { "THE_GRAPH_KEY", Environment.GetEnvironmentVariable("THE_GRAPH_KEY") },
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
