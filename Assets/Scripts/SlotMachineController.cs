using System.Collections;
using System.Linq;
using UnityEngine;

namespace SlotGame
{
    /// <summary>
    /// Game director. Owns the credits and the spin flow, and wires the other classes together:
    ///   RandomService  -> decides the outcome
    ///   Reel           -> animates it
    ///   PayoutCalculator -> judges it
    ///   SlotUI         -> displays it
    /// The result is chosen up front, THEN the reels animate to it. That keeps the animation
    /// purely visual and the fairness of the game entirely inside RandomService.
    /// </summary>
    public class SlotMachineController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SymbolData[] symbols;
        [SerializeField] private Reel[] reels;
        [SerializeField] private SlotUI ui;

        [Header("Game Settings")]
        [SerializeField] private int startingCredits = 1000;
        [SerializeField] private int[] betOptions = { 10, 25, 50, 100 };

        [Header("Timing")]
        [SerializeField] private float baseSpinTime = 1.0f;   // How long the first reel spins before stopping.
        [SerializeField] private float reelStopDelay = 0.45f; // Each following reel spins this much longer (left-to-right stop).

        private RandomService randomService;
        private int credits;
        private int betIndex;
        private bool isSpinning;
        private bool isGameOver;

        private int CurrentBet => betOptions[betIndex];

        private void Awake()
        {
            randomService = new RandomService();
        }

        private void Start()
        {
            foreach (Reel reel in reels)
            {
                reel.Initialize(symbols, randomService);
            }

            ui.SpinClicked += HandleSpinClicked;
            ui.BetIncreased += () => ChangeBet(+1);
            ui.BetDecreased += () => ChangeBet(-1);

            ResetGame();
        }

        private void ResetGame()
        {
            credits = startingCredits;
            betIndex = 0;
            isGameOver = false;

            ui.SetCredits(credits);
            ui.SetBet(CurrentBet);
            ui.ClearWin();
            ui.SetMessage("Press SPIN to play!");
            ui.SetControlsInteractable(true);
        }

        private void ChangeBet(int direction)
        {
            if (isSpinning) return;

            betIndex = Mathf.Clamp(betIndex + direction, 0, betOptions.Length - 1);
            ui.SetBet(CurrentBet);
        }

        private void HandleSpinClicked()
        {
            if (isSpinning) return;

            if (isGameOver)
            {
                ResetGame(); // Spin button doubles as "restart" after running out of credits.
                return;
            }

            if (credits < CurrentBet)
            {
                ui.SetMessage("Not enough credits for this bet - lower it!");
                return;
            }

            StartCoroutine(SpinRoutine(CurrentBet));
        }

        private IEnumerator SpinRoutine(int bet)
        {
            isSpinning = true;
            ui.SetControlsInteractable(false);
            ui.ClearWin();
            ui.SetMessage("Spinning...");

            credits -= bet;
            ui.SetCredits(credits);

            // 1) Decide the outcome first using the RNG.
            SymbolData[] results = new SymbolData[reels.Length];
            for (int i = 0; i < reels.Length; i++)
            {
                results[i] = randomService.PickWeighted(symbols);
            }

            // 2) Animate every reel toward its result; later reels spin longer so they stop one by one.
            for (int i = 0; i < reels.Length; i++)
            {
                StartCoroutine(reels[i].SpinAndStop(results[i], baseSpinTime + i * reelStopDelay));
            }

            while (reels.Any(reel => reel.IsSpinning))
            {
                yield return null;
            }

            // 3) Judge the result and pay out.
            SpinOutcome outcome = PayoutCalculator.Evaluate(results, bet);
            if (outcome.IsWin)
            {
                credits += outcome.Payout;
                ui.SetCredits(credits);
                ui.ShowWin(outcome.Payout);
                ui.SetMessage($"{outcome.WinningSymbol.symbolName} x{outcome.WinningSymbol.payoutMultiplier}! You win!");

                foreach (Reel reel in reels)
                {
                    reel.PlayWinPulse();
                }
            }
            else
            {
                ui.SetMessage("No match - try again!");
            }

            // 4) Out of money? Next click on SPIN restarts the game.
            if (credits < betOptions[0])
            {
                isGameOver = true;
                ui.SetMessage("Out of credits! Press SPIN to restart.");
            }

            isSpinning = false;
            ui.SetControlsInteractable(true);
        }
    }
}
