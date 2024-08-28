using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;

namespace UniSwapTradingBot.Utilities
{
    public static class TickMath
    {
        public const int MIN_TICK = -887272;
        public const int MAX_TICK = 887272;

        public static BigInteger GetSqrtRatioAtTick(int tick)
        {
            TicksValidation.CheckTick(tick);
            // ReSharper disable once IntVariableOverflowInUncheckedContext
            var absTick = tick < 0 ? tick * -1 : tick;
            if (tick < MIN_TICK || tick > MAX_TICK)
                throw new ArgumentOutOfRangeException(nameof(tick), "Tick is out of range");

            var ratio = (absTick & 0x1) != 0
                ? "0xfffcb933bd6fad37aa2d162d1a594001".HexToBigInteger(false)
                : "0x100000000000000000000000000000000".HexToBigInteger(false);

            if ((absTick & 0x2) != 0) ratio = (ratio * "0xfff97272373d413259a46990580e213a".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x4) != 0) ratio = (ratio * "0xfff2e50f5f656932ef12357cf3c7fdcc".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x8) != 0) ratio = (ratio * "0xffe5caca7e10e4e61c3624eaa0941cd0".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x10) != 0) ratio = (ratio * "0xffcb9843d60f6159c9db58835c926644".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x20) != 0) ratio = (ratio * "0xff973b41fa98c081472e6896dfb254c0".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x40) != 0) ratio = (ratio * "0xff2ea16466c96a3843ec78b326b52861".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x80) != 0) ratio = (ratio * "0xfe5dee046a99a2a811c461f1969c3053".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x100) != 0) ratio = (ratio * "0xfcbe86c7900a88aedcffc83b479aa3a4".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x200) != 0) ratio = (ratio * "0xf987a7253ac413176f2b074cf7815e54".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x400) != 0) ratio = (ratio * "0xf3392b0822b70005940c7a398e4b70f3".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x800) != 0) ratio = (ratio * "0xe7159475a2c29b7443b29c7fa6e889d9".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x1000) != 0) ratio = (ratio * "0xd097f3bdfd2022b8845ad8f792aa5825".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x2000) != 0) ratio = (ratio * "0xa9f746462d870fdf8a65dc1f90e061e5".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x4000) != 0) ratio = (ratio * "0x70d869a156d2a1b890bb3df62baf32f7".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x8000) != 0) ratio = (ratio * "0x31be135f97d08fd981231505542fcfa6".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x10000) != 0) ratio = (ratio * "0x9aa508b5b7a84e1c677de54f3e99bc9".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x20000) != 0) ratio = (ratio * "0x5d6af8dedb81196699c329225ee604".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x40000) != 0) ratio = (ratio * "0x2216e584f5fa1ea926041bedfe98".HexToBigInteger(false)) >> 128;
            if ((absTick & 0x80000) != 0) ratio = (ratio * "0x48a170391f7dc42444e8fa2".HexToBigInteger(false)) >> 128;

            if (tick > 0) ratio = MathUtils.Uint256MaxValue / ratio;

            // this divides by 1<<32 rounding up to go from a Q128.128 to a Q128.96.
            // we then downcast because we know the result always fits within 160 bits due to our tick input constraint
            // we round up in the division so getTickAtSqrtRatio of the output price is always consistent

            return (ratio >> 32) + (ratio % (new BigInteger(1) << 32) == 0 ? 0 : 1);
        }

        public static int GetTickAtSqrtRatio(BigInteger sqrtPriceX96)
        {
            BigInteger MIN_SQRT_RATIO = "4295128739".HexToBigInteger(false);
            BigInteger MAX_SQRT_RATIO = "1461446703485210103287273052203988822378723970342".HexToBigInteger(false);

            if (sqrtPriceX96 < MIN_SQRT_RATIO || sqrtPriceX96 > MAX_SQRT_RATIO)
                throw new ArgumentOutOfRangeException(nameof(sqrtPriceX96), "Sqrt price is out of range");

            int msb = 0;
            BigInteger r = sqrtPriceX96;
            if (r >= "0x100000000000000000000000000000000".HexToBigInteger(false))
            {
                r >>= 128;
                msb += 128;
            }
            if (r >= "0x10000000000000000".HexToBigInteger(false))
            {
                r >>= 64;
                msb += 64;
            }
            if (r >= 0x100000000)
            {
                r >>= 32;
                msb += 32;
            }
            if (r >= 0x10000)
            {
                r >>= 16;
                msb += 16;
            }
            if (r >= 0x100)
            {
                r >>= 8;
                msb += 8;
            }
            if (r >= 0x10)
            {
                r >>= 4;
                msb += 4;
            }
            if (r >= 0x4)
            {
                r >>= 2;
                msb += 2;
            }
            if (r >= 0x2) msb += 1;

            BigInteger log2 = (msb - 128) << 64;
            r = (sqrtPriceX96 << (127 - msb)) / (r);
            log2 += (r * r * r * r * r * r * r * r * r * r * r * r * r * r * r * r) >> 128;

            int tickLow = (int)(((log2 - "3402992956809132418596140100660247210".HexToBigInteger(false)) * "1451370000000000000000000000000000000".HexToBigInteger(false)) >> 128);
            int tickHigh = (int)(((log2 + "291339464771989622907027621153398088495".HexToBigInteger(false)) * "1461446703485210103287273052203988822378723970342".HexToBigInteger(false)) >> 128);

            return tickLow == tickHigh ? tickLow : tickHigh;
        }
    }
}
