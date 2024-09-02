using GraphQL.Client.Serializer.Newtonsoft;
using GraphQL;
using NBitcoin.Secp256k1;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;
using static UniswapV3PositionHelper;
using GraphQL.Client.Http;
using Nethereum.Contracts.QueryHandlers.MultiCall;

namespace UniSwapTradingBot.Utilities
{
    public static class Subgraph
    {
        public static async Task<List<Position>> GetPositions(string walletAddress, string Token0ProxyAddress, string Token1ProxyAddress, string TheGraphKey)
        {
            var positions = new List<Position>();

            // Create a GraphQL client instance
            var client = new GraphQLHttpClient("https://gateway.thegraph.com/api/" + TheGraphKey + "/subgraphs/id/EsLGwxyeMMeJuhqWvuLmJEiDKXJ4Z6YsoJreUnyeozco", new NewtonsoftJsonSerializer());

            // Define your GraphQL query as a string
            var query = new GraphQLRequest
            {
                Query = $@"
                {{
                    positions(where: {{ owner: ""{walletAddress}"", liquidity_gt: ""0"", token0_: {{ id: ""{Token0ProxyAddress}"" }}, token1_: {{ id: ""{Token1ProxyAddress}"" }}}}) {{
                        id
                        owner
                        liquidity
                        tickLower {{
                          tickIdx
                        }}
                        tickUpper {{
                          tickIdx
                        }}
                        token0 {{
                          id
                          symbol
                        }}
                        token1 {{
                          id
                          symbol
                        }}
                    }}
                }}"
            };

            try
            {
                // Send the request
                var response = await client.SendQueryAsync<dynamic>(query);

                // Handle the response
                if (response.Errors != null && response.Errors.Length > 0)
                {
                    Console.WriteLine("Errors: " + response.Errors[0].Message);
                }
                else if (response.Data != null && response.Data.Positions != null)
                {
                    // Deserialize into List of Positions
                    positions = response.Data.Positions;

                    // Print or process the positions
                    foreach (var position in positions)
                    {
                        Console.WriteLine($"Position ID: {position.Id}, Owner: {position.Operator}, Liquidity: {position.Liquidity}");
                    }
                }
                else
                {
                    Console.WriteLine("No positions found.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }

            return positions;
        }
    }

    public class GraphQLResponse
    {
        public Data Data { get; set; }
    }

    public class Data
    {
        public List<Position> Positions { get; set; }
    }
}
