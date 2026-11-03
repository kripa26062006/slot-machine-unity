using System.Collections.Generic;

namespace SlotGame
{
    /// <summary>Immutable result of evaluating one spin.</summary>
    public readonly struct SpinOutcome
    {
        public readonly bool IsWin;
        public readonly SymbolData WinningSymbol;
        public readonly int Payout;

        public SpinOutcome(bool isWin, SymbolData winningSymbol, int payout)
        {
            IsWin = isWin;
            WinningSymbol = winningSymbol;
            Payout = payout;
        }

        public static SpinOutcome Loss => new SpinOutcome(false, null, 0);
    }

    /// <summary>
    /// Pure game-rule logic: no Unity objects, no UI, no state. Easy to reason about and unit test.
    /// Rule: the player wins when ALL reels show the same symbol. Wild symbols (bonus) count as any symbol.
    /// Payout = bet x the winning symbol's payoutMultiplier.
    /// </summary>
    public static class PayoutCalculator
    {
        public static SpinOutcome Evaluate(IReadOnlyList<SymbolData> reelResults, int bet)
        {
            SymbolData target = null;

            foreach (SymbolData symbol in reelResults)
            {
                if (symbol.isWild)
                {
                    continue; // A wild matches anything, so it never breaks a line.
                }

                if (target == null)
                {
                    target = symbol;
                }
                else if (symbol != target)
                {
                    return SpinOutcome.Loss; // Two different normal symbols -> no win.
                }
            }

            // Every reel was a wild: pay out as the wild symbol itself.
            if (target == null)
            {
                target = reelResults[0];
            }

            return new SpinOutcome(true, target, bet * target.payoutMultiplier);
        }
    }
}
