using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace UniSwapTradingBot.Utilities
{
    public static class MathUtils
    {
        public static BigInteger Uint256MaxValue =
            BigInteger.Parse("115792089237316195423570985008687907853269984665640564039457584007913129639935");

        public static async Task<BigInteger> ConvertDecimalToBigInteger(Web3 web3, string tokenAddress, decimal value)
        {
            var scale = await TokenHelper.GetTokenDecimals(web3, tokenAddress);
            return Web3.Convert.ToWei(value, scale);
        }
    }
}
