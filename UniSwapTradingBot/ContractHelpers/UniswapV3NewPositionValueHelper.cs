using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniSwapTradingBot.ContractHelpers
{
    public static class UniswapV3NewPositionValueHelper
    {
        public static async Task<(decimal amount0, decimal amount1)> CalculateAmountsForNewPosition(
            Web3 web3,
            decimal currentPrice,
            decimal lowerPrice,
            decimal upperPrice,
            decimal availableToken0,
            decimal availableToken1)
        {
            // Calculate sqrt prices
            var sqrtPriceCurrent = (decimal)Math.Sqrt((double)currentPrice);
            var sqrtPriceLower = (decimal)Math.Sqrt((double)lowerPrice);
            var sqrtPriceUpper = (decimal)Math.Sqrt((double)upperPrice);

            // Calculate liquidity amounts
            var liquidityAmount0 = availableToken0 * sqrtPriceCurrent * sqrtPriceUpper / (sqrtPriceUpper - sqrtPriceLower);
            var liquidityAmount1 = availableToken1 / (sqrtPriceUpper - sqrtPriceLower);

            // Determine the limiting factor (minimum liquidity that can be provided by token0 or token1)
            var liquidity = Math.Min(liquidityAmount0, liquidityAmount1);

            // Calculate the final amounts based on the liquidity
            var amount0 = liquidity * (sqrtPriceUpper - sqrtPriceLower) / (sqrtPriceCurrent * sqrtPriceUpper);
            var amount1 = liquidity * (sqrtPriceUpper - sqrtPriceLower);

            return (amount0, amount1);
        }
    }
}
