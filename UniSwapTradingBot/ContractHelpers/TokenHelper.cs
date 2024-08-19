using Nethereum.Web3;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System;

public static class TokenHelper
{
    private static string ERC20_ABI = @"[
        {
            ""inputs"": [
                {
                    ""internalType"": ""address"",
                    ""name"": ""account"",
                    ""type"": ""address""
                }
            ],
            ""name"": ""balanceOf"",
            ""outputs"": [
                {
                    ""internalType"": ""uint256"",
                    ""name"": """",
                    ""type"": ""uint256""
                }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        },
{
            ""inputs"": [],
            ""name"": ""decimals"",
            ""outputs"": [
                {
                    ""internalType"": ""uint8"",
                    ""name"": """",
                    ""type"": ""uint8""
                }
            ],
            ""stateMutability"": ""view"",
            ""type"": ""function""
        }
    ]";

    private static string PROXY_ABI = @"[
    {
        ""constant"": true,
        ""inputs"": [],
        ""name"": ""decimals"",
        ""outputs"": [{""name"": """", ""type"": ""uint8""}],
        ""type"": ""function""
    }
]";

    public static async Task<int> GetTokenDecimals(Web3 web3, string tokenAddress)
    {
        var contract = web3.Eth.GetContract(ERC20_ABI, tokenAddress);
        var decimalsFunction = contract.GetFunction("decimals");
        var decimals = await decimalsFunction.CallAsync<int>();
        return decimals;
    }

    public static async Task<decimal> GetAvailableTokenBalance(Web3 web3, string tokenAddress, string proxyAddress, string walletAddress)
    {
        var decimals = await GetTokenDecimals(web3, tokenAddress);

        var contract = web3.Eth.GetContract(ERC20_ABI, tokenAddress);
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var balance = await balanceOfFunction.CallAsync<BigInteger>(walletAddress);

        // Convert the balance using the retrieved decimals
        return Web3.Convert.FromWei(balance, decimals);
    }
}
