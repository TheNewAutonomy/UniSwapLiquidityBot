using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UniSwapTradingBot.Utilities
{
    public static class TicksValidation
    {
        public static void CheckTick(int tick)
        {
            if (tick is not (>= TickMath.MIN_TICK and <= TickMath.MAX_TICK))
                throw new ArgumentException();
        }

        public static void CheckTicks(int tickLower, int tickUpper)
        {
            if (tickLower >= tickUpper)
                throw new ArgumentException("TLU");
            if (tickLower < TickMath.MIN_TICK)
                throw new ArgumentOutOfRangeException(nameof(tickLower), tickLower, "tickLower lower minimum");
            if (tickUpper > TickMath.MAX_TICK)
                throw new ArgumentOutOfRangeException(nameof(tickUpper), tickUpper, "tickUpper upper maximum");
        }
    }
}
