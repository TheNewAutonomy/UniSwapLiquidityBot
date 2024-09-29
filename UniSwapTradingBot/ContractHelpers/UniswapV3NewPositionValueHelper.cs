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

        public static Task<(string tokenToSell, string tokenToBuy, BigInteger amountToSell, BigInteger amountToBuy)> CalculateOptimalSwapForNewPosition(
    Web3 web3,
    decimal currentPrice,
    int newTickLower,
    int newTickUpper,
    decimal availableToken0,
    decimal availableToken1,
    int token0Decimals, // Decimals for Token0 (e.g., BTC)
    int token1Decimals, // Decimals for Token1 (e.g., USDC)
    string token0Address,
    string token1Address
)
        {
            // Step 1: Calculate the dollar value of each token amount
            decimal valueToken0InUSD = availableToken0 * currentPrice; // Value of Token0 (e.g., Bitcoin) in USD
            decimal valueToken1InUSD = availableToken1; // Value of Token1 (USDC) in USD (1:1)

            // Step 2: Calculate total value and target value for each token to achieve 50/50 balance
            decimal totalValueInUSD = valueToken0InUSD + valueToken1InUSD;
            decimal targetValuePerTokenInUSD = totalValueInUSD / 2;

            string tokenToSell = string.Empty;
            string tokenToBuy = string.Empty;
            BigInteger amountToSell = 0;
            BigInteger amountToBuy = 0;
            BigInteger optimalAmount0 = ConvertToBigInteger(availableToken0, token0Decimals);
            BigInteger optimalAmount1 = ConvertToBigInteger(availableToken1, token1Decimals);

            // Step 3: Determine which token needs to be swapped and by how much to achieve the target balance
            if (valueToken0InUSD > targetValuePerTokenInUSD)
            {
                // We have more value in Token0 (Bitcoin) than desired, so we need to sell some of it
                tokenToSell = token0Address;
                tokenToBuy = token1Address;

                // Calculate how much Token0 (Bitcoin) to sell to reach target value
                decimal excessValueInUSD = valueToken0InUSD - targetValuePerTokenInUSD;
                decimal amountToSellDecimal = excessValueInUSD / currentPrice; // Amount of Token0 to sell

                // Convert the amount to sell into the smallest units (BigInteger)
                amountToSell = ConvertToBigInteger(amountToSellDecimal, token0Decimals);

                // Recalculate the resulting amounts after the swap
                optimalAmount0 = ConvertToBigInteger(availableToken0 - amountToSellDecimal, token0Decimals);
                optimalAmount1 = ConvertToBigInteger(availableToken1 + excessValueInUSD, token1Decimals); // Receiving this amount in USDC

                // Ensure non-negative amounts
                optimalAmount0 = BigInteger.Max(optimalAmount0, 0);
                optimalAmount1 = BigInteger.Max(optimalAmount1, 0);

                // Calculate amount to buy
                amountToBuy = optimalAmount1 - ConvertToBigInteger(availableToken1, token1Decimals);
            }
            else if (valueToken1InUSD > targetValuePerTokenInUSD)
            {
                // We have more value in Token1 (USDC) than desired, so we need to sell some of it
                tokenToSell = token1Address;
                tokenToBuy = token0Address;

                // Calculate how much Token1 (USDC) to sell to reach target value
                decimal excessValueInUSD = valueToken1InUSD - targetValuePerTokenInUSD;
                amountToSell = ConvertToBigInteger(excessValueInUSD, token1Decimals); // Amount of Token1 to sell (1:1 value)

                // Recalculate the resulting amounts after the swap
                optimalAmount0 = ConvertToBigInteger(availableToken0 + (excessValueInUSD / currentPrice), token0Decimals); // Receiving this amount in Token0
                optimalAmount1 = ConvertToBigInteger(availableToken1 - excessValueInUSD, token1Decimals);

                // Ensure non-negative amounts
                optimalAmount0 = BigInteger.Max(optimalAmount0, 0);
                optimalAmount1 = BigInteger.Max(optimalAmount1, 0);

                // Calculate amount to buy
                amountToBuy = optimalAmount0 - ConvertToBigInteger(availableToken0, token0Decimals);
            }
            else
            {
                // Already balanced, no swap needed
                tokenToSell = "0x0000000000000000000000000000000000000000\r\n";
                amountToSell = 0;
            }

            return Task.FromResult<(string tokenToSell, string tokenToBuy, BigInteger amountToSell, BigInteger amountToBuy)>((tokenToSell, tokenToBuy, amountToSell, amountToBuy)); // Assuming 2 decimals for total USD value
        }

        // Helper method to convert decimal to BigInteger based on token decimals
        private static BigInteger ConvertToBigInteger(decimal value, int decimals)
        {
            decimal scaleFactor = (decimal)Math.Pow(10, decimals);
            return new BigInteger(value * scaleFactor);
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
