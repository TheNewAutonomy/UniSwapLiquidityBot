using Nethereum.Web3;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using UniSwapTradingBot.Utilities;

namespace UniSwapTradingBot.ContractHelpers
{
    public static class UniswapV3NewPositionValueHelper
    {
        // Function to convert decimal to BigInteger scaled by 2^96
        static BigInteger ConvertDecimalToBigIntegerWithScaling(decimal value, int scale)
        {
            // Separate the integral and fractional parts
            int[] bits = decimal.GetBits(value);
            ulong low = (uint)bits[0];
            ulong mid = (uint)bits[1];
            ulong high = (uint)bits[2];
            int exponent = (bits[3] >> 16) & 0xFF;
            bool isNegative = (bits[3] & 0x80000000) != 0;

            // Compute the integral part as BigInteger
            BigInteger integral = new BigInteger((high << 32 | mid) << 32 | low);
            if (isNegative)
                integral = -integral;

            // Scale the integral part by 2^96
            integral = integral * BigInteger.Pow(2, scale);

            // Compute the fractional part
            decimal fractional = value - decimal.Truncate(value);

            // Convert the fractional part to BigInteger manually without overflow
            BigInteger fractionalBigInt = 0;
            for (int i = 0; i < scale; i++)
            {
                fractional *= 2;
                if (fractional >= 1)
                {
                    fractionalBigInt += BigInteger.One << (scale - 1 - i);
                    fractional -= 1;
                }
            }

            // Combine integral and fractional parts
            return integral + fractionalBigInt;
        }

        public static async Task<(string tokenToSwap, decimal amountToSwap, decimal resultingAmount0, decimal resultingAmount1)> CalculateOptimalSwapForNewPosition(
    Web3 web3,
    decimal currentPrice,
    int newTickLower,
    int newTickUpper,
    decimal availableToken0,
    decimal availableToken1)
        {
            // Step 1: Calculate the dollar value of each token amount
            decimal valueToken0InUSD = availableToken0 * currentPrice; // Value of Bitcoin in USD
            decimal valueToken1InUSD = availableToken1; // Value of USDC in USD (1:1)

            // Step 2: Calculate total value and target value for each token to achieve 50/50 balance
            decimal totalValueInUSD = valueToken0InUSD + valueToken1InUSD;
            decimal targetValuePerTokenInUSD = totalValueInUSD / 2;

            string tokenToSwap = string.Empty;
            decimal amountToSwap = 0m;
            decimal optimalAmount0Decimal = availableToken0;
            decimal optimalAmount1Decimal = availableToken1;

            // Step 3: Determine which token needs to be swapped and by how much to achieve the target balance
            if (valueToken0InUSD > targetValuePerTokenInUSD)
            {
                // We have more value in Token0 (Bitcoin) than desired, so we need to sell some of it
                tokenToSwap = "Token0";

                // Calculate how much Token0 (Bitcoin) to sell to reach target value
                decimal excessValueInUSD = valueToken0InUSD - targetValuePerTokenInUSD;
                amountToSwap = excessValueInUSD / currentPrice; // Amount of Token0 (Bitcoin) to sell

                // Recalculate the resulting amounts after the swap
                optimalAmount0Decimal = availableToken0 - amountToSwap;
                optimalAmount1Decimal = availableToken1 + excessValueInUSD; // Receiving this amount in USD worth of Token1 (USDC)
            }
            else if (valueToken1InUSD > targetValuePerTokenInUSD)
            {
                // We have more value in Token1 (USDC) than desired, so we need to sell some of it
                tokenToSwap = "Token1";

                // Calculate how much Token1 (USDC) to sell to reach target value
                decimal excessValueInUSD = valueToken1InUSD - targetValuePerTokenInUSD;
                amountToSwap = excessValueInUSD; // Amount of Token1 (USDC) to sell (1:1 value)

                // Recalculate the resulting amounts after the swap
                optimalAmount0Decimal = availableToken0 + (excessValueInUSD / currentPrice); // Receiving this amount in Token0 (Bitcoin)
                optimalAmount1Decimal = availableToken1 - amountToSwap;
            }
            else
            {
                // Already balanced, no swap needed
                tokenToSwap = "None";
                amountToSwap = 0m;
            }

            // Ensure non-negative amounts
            optimalAmount0Decimal = Math.Max(optimalAmount0Decimal, 0);
            optimalAmount1Decimal = Math.Max(optimalAmount1Decimal, 0);

            return (tokenToSwap, amountToSwap, optimalAmount0Decimal, optimalAmount1Decimal);
        }


        public static (decimal amountToBuy, string tokenToBuy) DetermineTokenPurchase(
    decimal requiredAmount0, decimal requiredAmount1,
    decimal availableAmount0, decimal availableAmount1)
        {
            decimal deficit0 = requiredAmount0 - availableAmount0;
            decimal deficit1 = requiredAmount1 - availableAmount1;

            if (deficit0 > 0 && deficit1 <= 0)
            {
                return (deficit0, "token0");
            }
            else if (deficit1 > 0 && deficit0 <= 0)
            {
                return (deficit1, "token1");
            }
            else if (deficit0 > 0 && deficit1 > 0)
            {
                // You need to buy both tokens
                return (deficit0, "token0");  // or deficit1, "token1", depending on your strategy
            }
            else
            {
                // No need to buy anything, or consider selling excess if needed
                return (0, "none");
            }
        }
    }
}
