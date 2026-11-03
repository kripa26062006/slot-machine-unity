using UnityEngine;

namespace SlotGame
{
    /// <summary>
    /// Data container for one slot symbol (cherry, bell, seven...).
    /// Create one asset per symbol: Right click in Project > Create > SlotGame > Symbol Data.
    /// Keeping symbols as ScriptableObjects means designers can tweak payouts/odds without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "Symbol_", menuName = "SlotGame/Symbol Data")]
    public class SymbolData : ScriptableObject
    {
        [Tooltip("Display name, also shown in the payout table.")]
        public string symbolName = "Symbol";

        [Tooltip("Sprite shown on the reel.")]
        public Sprite sprite;

        [Min(1), Tooltip("Win = bet x this value when ALL reels show this symbol.")]
        public int payoutMultiplier = 5;

        [Min(1), Tooltip("How often this symbol is picked. Higher = more common = lower payout usually.")]
        public int weight = 10;

        [Tooltip("BONUS: a wild substitutes for any other symbol when checking for a win.")]
        public bool isWild = false;
    }
}
