using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    /// <summary>
    /// One vertical reel. Shows 3 rows (top / PAYLINE in the middle / bottom).
    ///
    /// How the animation works:
    /// The reel is a virtual infinite strip of symbols. 'position' says which strip index is
    /// currently in the centre of the window (it's a float, so 2.5 means halfway between two symbols).
    /// Only 6 UI cells exist; they are recycled as the strip scrolls, so spinning costs almost nothing.
    /// The final symbol is decided BEFORE the spin starts (by the controller's RNG) and is written
    /// into the strip a few rows ahead, then the reel decelerates smoothly onto it.
    /// </summary>
    public class Reel : MonoBehaviour
    {
        private const int CellCount = 6;      // 3 visible rows + buffer above and below.
        private const int MinStopRows = 6;    // Landing symbol is at least this many rows ahead (keeps it off-screen until it scrolls in).
        private const float OvershootRows = 0.25f;

        [Header("Layout")]
        [SerializeField] private RectTransform viewport;   // Parent with a RectMask2D; its height should be cellHeight * 3.
        [SerializeField] private RectTransform strip;      // Child that holds the cells.
        [SerializeField] private Image cellPrefab;         // Simple UI Image used for every symbol.
        [SerializeField] private float cellHeight = 160f;

        [Header("Animation")]
        [SerializeField] private float spinSpeed = 25f;        // Rows per second at full speed.
        [SerializeField] private float spinUpTime = 0.2f;      // Ramp-up so the reel doesn't start instantly.
        [SerializeField] private float settleDuration = 0.15f; // Little "click back" after the overshoot.

        private readonly Image[] cells = new Image[CellCount];
        private readonly Dictionary<int, SymbolData> strip_symbols = new Dictionary<int, SymbolData>();

        private IReadOnlyList<SymbolData> symbolPool;
        private RandomService randomService;
        private float position;

        public bool IsSpinning { get; private set; }

        /// <summary>Builds the cells and shows a random starting symbol.</summary>
        public void Initialize(IReadOnlyList<SymbolData> pool, RandomService rng)
        {
            symbolPool = pool;
            randomService = rng;

            for (int i = 0; i < CellCount; i++)
            {
                Image cell = Instantiate(cellPrefab, strip);
                cell.rectTransform.sizeDelta = new Vector2(viewport.rect.width, cellHeight);
                cell.preserveAspect = true;
                cells[i] = cell;
            }

            position = 0f;
            Render();
        }

        /// <summary>
        /// Spins for 'spinTime' seconds, then eases to a stop with 'result' on the payline.
        /// Run with StartCoroutine; poll IsSpinning to know when it's finished.
        /// </summary>
        public IEnumerator SpinAndStop(SymbolData result, float spinTime)
        {
            IsSpinning = true;

            // Phase 1: spin at constant speed (with a short ramp-up).
            float elapsed = 0f;
            while (elapsed < spinTime)
            {
                elapsed += Time.deltaTime;
                float speed = spinSpeed * Mathf.Clamp01(elapsed / spinUpTime);
                position += speed * Time.deltaTime;
                Render();
                yield return null;
            }

            // Phase 2: decide where to land and write the result into the strip.
            int targetIndex = Mathf.CeilToInt(position) + MinStopRows;
            strip_symbols[targetIndex] = result;

            // Phase 3: ease-out to slightly PAST the target. The cubic ease-out starts with speed
            // 3 * distance / duration, so choosing duration = 3 * distance / spinSpeed makes the
            // deceleration start at exactly the spin speed (no visible jump).
            float startPosition = position;
            float overshootTarget = targetIndex + OvershootRows;
            float distance = overshootTarget - startPosition;
            float stopDuration = 3f * distance / spinSpeed;

            elapsed = 0f;
            while (elapsed < stopDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / stopDuration);
                float eased = 1f - Mathf.Pow(1f - t, 3f); // Cubic ease-out.
                position = startPosition + distance * eased;
                Render();
                yield return null;
            }

            // Phase 4: small settle back onto the exact symbol for a mechanical "clunk".
            elapsed = 0f;
            while (elapsed < settleDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / settleDuration);
                float eased = t * t * (3f - 2f * t); // Smoothstep.
                position = Mathf.Lerp(overshootTarget, targetIndex, eased);
                Render();
                yield return null;
            }

            position = targetIndex;
            Render();
            PruneOldSymbols();
            IsSpinning = false;
        }

        /// <summary>Quick scale pulse used to celebrate a win.</summary>
        public void PlayWinPulse()
        {
            StartCoroutine(WinPulseRoutine());
        }

        private IEnumerator WinPulseRoutine()
        {
            const float duration = 0.8f;
            const float maxScaleBoost = 0.08f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                // Two quick bounces using a sine wave.
                float pulse = Mathf.Abs(Mathf.Sin(elapsed / duration * Mathf.PI * 2f));
                viewport.localScale = Vector3.one * (1f + maxScaleBoost * pulse);
                yield return null;
            }

            viewport.localScale = Vector3.one;
        }

        /// <summary>Positions each cell and assigns its sprite based on the current scroll position.</summary>
        private void Render()
        {
            int baseIndex = Mathf.FloorToInt(position);

            for (int i = baseIndex - 2; i <= baseIndex + 3; i++)
            {
                // Same strip index always maps to the same cell, so cells never "pop" while scrolling.
                Image cell = cells[((i % CellCount) + CellCount) % CellCount];

                Sprite sprite = GetSymbolAt(i).sprite;
                if (cell.sprite != sprite)
                {
                    cell.sprite = sprite;
                }

                // Higher index = higher on screen. As 'position' grows the symbols slide downward.
                cell.rectTransform.anchoredPosition = new Vector2(0f, (i - position) * cellHeight);
            }
        }

        /// <summary>Returns the symbol at a strip index, generating random filler the first time it's needed.</summary>
        private SymbolData GetSymbolAt(int index)
        {
            if (!strip_symbols.TryGetValue(index, out SymbolData symbol))
            {
                symbol = randomService.PickWeighted(symbolPool);
                strip_symbols[index] = symbol;
            }

            return symbol;
        }

        /// <summary>Drops strip entries that are far behind us so the dictionary doesn't grow forever.</summary>
        private void PruneOldSymbols()
        {
            int oldestToKeep = Mathf.FloorToInt(position) - 4;
            List<int> toRemove = new List<int>();

            foreach (int index in strip_symbols.Keys)
            {
                if (index < oldestToKeep)
                {
                    toRemove.Add(index);
                }
            }

            foreach (int index in toRemove)
            {
                strip_symbols.Remove(index);
            }
        }
    }
}
