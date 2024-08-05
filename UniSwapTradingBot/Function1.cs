using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using System.Net.Http;
using System.Threading;
using Nethereum.Web3;
using Nethereum.ABI.FunctionEncoding.Attributes;
using Nethereum.Contracts.CQS;
using Nethereum.Util;
using Nethereum.Web3.Accounts;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Contracts;
using Nethereum.Contracts.Extensions;
using System.Numerics;

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
                // Implement trading bot logic here

                // Get token (wBTC) price
                // Call pool to check minimum and maximum band
                // if lower than minimum band, or higher than maximum band, sell x% of pool (start with 100%)
                // Calculate a new minimum and maximum band based on some logic
                // Calculate the amount of token to buy to maximise position
                // Buy token
                // Submit transaction to pool

                // Test code
                GetAccountBalance().Wait();

                //

                log.LogInformation("Executing trade at: " + DateTime.UtcNow);
                // Your trading logic goes here
            }

            static async Task GetAccountBalance()
            {
                var web3 = new Web3("https://mainnet.infura.io/v3/7238211010344719ad14a89db874158c");
                var balance = await web3.Eth.GetBalance.SendRequestAsync("0xde0b295669a9fd93d5f28d9ec85e40f4cb697bae");
                Console.WriteLine($"Balance in Wei: {balance.Value}");

                var etherAmount = Web3.Convert.FromWei(balance.Value);
                Console.WriteLine($"Balance in Ether: {etherAmount}");
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

                return new OkObjectResult(new
                {
                    instanceId,
                    statusQueryGetUri = starter.CreateHttpManagementPayload(instanceId).StatusQueryGetUri,
                    sendEventPostUri = starter.CreateHttpManagementPayload(instanceId).SendEventPostUri,
                    terminatePostUri = starter.CreateHttpManagementPayload(instanceId).TerminatePostUri,
                    restartPostUri = starter.CreateHttpManagementPayload(instanceId).RestartPostUri
                });
            }
        }
    }
}
