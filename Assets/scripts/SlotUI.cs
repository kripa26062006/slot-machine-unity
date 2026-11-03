using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SlotGame
{
    /// <summary>
    /// Owns every on-screen label and button. It knows nothing about game rules:
    /// it just displays what it's told and raises events when the player clicks.
    /// </summary>
    public class SlotUI : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TMP_Text creditsText;
        [SerializeField] private TMP_Text betText;
        [SerializeField] private TMP_Text winText;
        [SerializeField] private TMP_Text messageText;

        [Header("Buttons")]
        [SerializeField] private Button spinButton;
        [SerializeField] private Button betUpButton;
        [SerializeField] private Button betDownButton;

        public event Action SpinClicked;
        public event Action BetIncreased;
        public event Action BetDecreased;

        private Coroutine winCountRoutine;

        private void Awake()
        {
            spinButton.onClick.AddListener(() => SpinClicked?.Invoke());
            betUpButton.onClick.AddListener(() => BetIncreased?.Invoke());
            betDownButton.onClick.AddListener(() => BetDecreased?.Invoke());
        }

        public void SetCredits(int credits) => creditsText.text = $"CREDITS: {credits}";

        public void SetBet(int bet) => betText.text = $"BET: {bet}";

        public void SetMessage(string message) => messageText.text = message;

        /// <summary>Disables the buttons while the reels are spinning so the player can't double-spin.</summary>
        public void SetControlsInteractable(bool interactable)
        {
            spinButton.interactable = interactable;
            betUpButton.interactable = interactable;
            betDownButton.interactable = interactable;
        }

        public void ClearWin()
        {
            if (winCountRoutine != null)
            {
                StopCoroutine(winCountRoutine);
            }

            winText.text = string.Empty;
        }

        /// <summary>Shows the win amount with a short count-up animation.</summary>
        public void ShowWin(int amount)
        {
            ClearWin();
            winCountRoutine = StartCoroutine(CountUpRoutine(amount));
        }

        private IEnumerator CountUpRoutine(int amount)
        {
            const float duration = 0.8f;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                int shown = Mathf.RoundToInt(Mathf.Lerp(0f, amount, elapsed / duration));
                winText.text = $"WIN: {shown}";
                yield return null;
            }

            winText.text = $"WIN: {amount}";
        }
    }
}
