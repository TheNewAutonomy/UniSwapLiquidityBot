using Nethereum.Web3;
using Nethereum.Contracts;
using System.Numerics;
using System.Threading.Tasks;
using System;

public static class TokenHelper
{
    private static string ERC20_ABI = @"[
        {
            ""constant"": true,
            ""inputs"": [{""name"": ""_owner"", ""type"": ""address""}],
            ""name"": ""balanceOf"",
            ""outputs"": [{""name"": ""balance"", ""type"": ""uint256""}],
            ""type"": ""function""
        }
    ]";

    public static async Task<decimal> GetAvailableTokenBalance(Web3 web3, string tokenAddress, string walletAddress)
    {
        var contract = web3.Eth.GetContract(ERC20_ABI, tokenAddress);
        var balanceOfFunction = contract.GetFunction("balanceOf");
        var balance = await balanceOfFunction.CallAsync<BigInteger>(walletAddress);

        // Assuming the token has 18 decimals, adjust according to the actual token decimals
        return Web3.Convert.FromWei(balance);
    }

    public static async Task<decimal> GetAvailableToken(Web3 web3, string walletAddress, string tokenAddress)
    {
        return await GetAvailableTokenBalance(web3, tokenAddress, walletAddress);
    }
}
