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
        /*
        Steps to Calculate the Amount of Tokens
        Get the Current Reserves: Get the current reserves of token0 and token1 in the pool.
        Calculate the Price Bounds: Determine the lower and upper price bounds for the new position.
        Calculate the Amounts of Token0 and Token1: Use the Uniswap V3 formulas to calculate the required amounts of each token to provide liquidity within the given price range.

        Uniswap V3 Liquidity Math
        The key formulas to use are:

        Liquidity (L):

        L = amount0 * √P current * √P upper
            --------------------------------
                 √P upper - √ P lower


        L =       amount1
             -------------------
             √P upper - √P lower

        Where:
        P current is the current price.
        P upper and P lower are the upper and lower price bounds.
        √P is the square root of the price
         */
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
