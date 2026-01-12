// IVXWalletDisplay.cs
// Reusable wallet balance display component
// Now aligned with IVXNWalletManager / QVNWalletManager stack
// Auto-updates when balances change via IVXNWalletManager.OnWalletBalanceChanged

using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using IntelliVerseX.Core; // IVXNWalletManager is in Core namespace

namespace IntelliVerseX.UI
{
    /// <summary>
    /// Displays and auto-updates wallet balance.
    /// Works with the new IntelliVerseX wallet stack:
    /// - IVXNManager (backend & Nakama wiring)
    /// - IVXNWalletManager (central static snapshot + events)
    /// - QVNWalletManager (game-level wrapper in QuizVerse)
    ///
    /// Usage:
    /// 1. Attach to a GameObject with a TMP_Text component.
    /// 2. Configure wallet type (Game / Global).
    /// 3. Optional: set prefix/suffix/format/abbreviation.
    /// 4. Ensure IVXNWalletManager is initialized (e.g. by QVNWalletManager).
    /// 5. Component auto-subscribes to IVXNWalletManager.OnWalletBalanceChanged.
    /// </summary>
    public class IVXWalletDisplay : MonoBehaviour
    {
        #region Configurable Fields
        [Header("Wallet Configuration")]
        [Tooltip("Show game-specific currency or global IVX tokens")]
        [SerializeField] private WalletType walletType = WalletType.Game;

        [Tooltip("Currency ID for game wallet (for your own labeling/logic if needed)")]
        [SerializeField] private string currencyId = "gems";

        [Header("UI References")]
        [SerializeField] private TMP_Text balanceText;
        [SerializeField] private Image currencyIcon;

        [Header("Formatting")]
        [SerializeField] private string prefix = "";
        [SerializeField] private string suffix = "";
        [SerializeField] private string format = "N0"; // N0 = 1,234
        [SerializeField] private bool abbreviateLargeNumbers = true;

        [Header("Animation")]
        [SerializeField] private bool animateChanges = true;
        [SerializeField] private float animationDuration = 0.5f;
        [SerializeField] private AnimationCurve animationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        // Internal state
        private long currentBalance;
        private long targetBalance;
        private float animationTimer;
        private bool isAnimating;
        private bool isSubscribedToWalletEvents;

        public enum WalletType
        {
            Game,   // Game-specific currency (gems, coins, etc.)
            Global  // Global IVX tokens
        }
        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            // Auto-find TMP_Text if not assigned
            if (balanceText == null)
                balanceText = GetComponent<TMP_Text>();

            if (balanceText == null)
            {
                Debug.LogError("[IVXWalletDisplay] No TMP_Text found. Please assign one.");
            }
        }

        private void OnEnable()
        {
            SubscribeToWalletEvents();
            // Initialize with current snapshot from IVXNWalletManager
            RefreshBalanceFromSnapshot();
        }

        private void OnDisable()
        {
            UnsubscribeFromWalletEvents();
        }

        private void Update()
        {
            if (!isAnimating)
                return;

            if (animationDuration <= 0f)
            {
                // No duration → snap instantly
                isAnimating = false;
                currentBalance = targetBalance;
                UpdateBalanceText(currentBalance);
                return;
            }

            animationTimer += Time.deltaTime;
            float t = Mathf.Clamp01(animationTimer / animationDuration);
            float curveValue = animationCurve != null ? animationCurve.Evaluate(t) : t;

            long displayBalance = (long)Mathf.Lerp(currentBalance, targetBalance, curveValue);
            UpdateBalanceText(displayBalance);

            if (t >= 1f)
            {
                isAnimating = false;
                currentBalance = targetBalance;
                UpdateBalanceText(currentBalance);
            }
        }

        #endregion

        #region Event Wiring

        private void SubscribeToWalletEvents()
        {
            if (isSubscribedToWalletEvents)
                return;

            // Listen directly to the core wallet manager.
            // QVNWalletManager already feeds into IVXNWalletManager, so this covers all flows.
            IVXNWalletManager.OnWalletBalanceChanged += HandleWalletUpdated;
            isSubscribedToWalletEvents = true;
        }

        private void UnsubscribeFromWalletEvents()
        {
            if (!isSubscribedToWalletEvents)
                return;

            IVXNWalletManager.OnWalletBalanceChanged -= HandleWalletUpdated;
            isSubscribedToWalletEvents = false;
        }

        /// <summary>
        /// Called whenever IVXNWalletManager changes balances.
        /// </summary>
        private void HandleWalletUpdated(long gameBalance, long globalBalance)
        {
            long newBalance = walletType == WalletType.Game ? gameBalance : globalBalance;
            ApplyNewBalance(newBalance);
        }

        #endregion

        #region Balance Logic

        /// <summary>
        /// Manually re-read balances from IVXNWalletManager snapshot.
        /// Useful if something changed before this display was enabled.
        /// </summary>
        public void RefreshBalance()
        {
            RefreshBalanceFromSnapshot();
        }

        private void RefreshBalanceFromSnapshot()
        {
            // Ensure IVXNWalletManager is initialized; if not, it safely initializes to zero.
            long game = IVXNWalletManager.GameBalance;
            long global = IVXNWalletManager.GlobalBalance;

            long newBalance = walletType == WalletType.Game ? game : global;
            ApplyNewBalance(newBalance, animateIfNeeded: false);
        }

        private void ApplyNewBalance(long newBalance, bool animateIfNeeded = true)
        {
            if (newBalance < 0)
                newBalance = 0; // Safety clamp, though QVNWalletManager should already handle this.

            // If we don't want animation (initial snap), just set directly.
            if (!animateChanges || !Application.isPlaying || !animateIfNeeded)
            {
                currentBalance = newBalance;
                targetBalance = newBalance;
                isAnimating = false;
                UpdateBalanceText(currentBalance);
                return;
            }

            // If balance changed, start a smooth animation from currentBalance to newBalance.
            if (newBalance != targetBalance || !isAnimating)
            {
                // We keep currentBalance as the "from" value for the lerp.
                targetBalance = newBalance;
                animationTimer = 0f;
                isAnimating = true;
            }
        }

        private void UpdateBalanceText(long balance)
        {
            if (balanceText == null) return;

            string formattedBalance = abbreviateLargeNumbers
                ? AbbreviateNumber(balance)
                : balance.ToString(format);

            balanceText.text = $"{prefix}{formattedBalance}{suffix}";
        }

        private string AbbreviateNumber(long value)
        {
            if (value < 1000)
                return value.ToString();

            if (value < 1_000_000)
                return $"{value / 1000f:F1}K";

            if (value < 1_000_000_000)
                return $"{value / 1_000_000f:F1}M";

            return $"{value / 1_000_000_000f:F1}B";
        }

        #endregion

        #region Public API

        /// <summary>
        /// Manually set display balance (for testing or special cases).
        /// NOTE: This does NOT update IVXNWalletManager, only the UI.
        /// </summary>
        public void SetBalance(long balance)
        {
            ApplyNewBalance(balance);
        }

        /// <summary>
        /// Change currency ID at runtime (for game wallet only).
        /// This is currently informational; actual numeric values still come from IVXNWalletManager.
        /// </summary>
        public void SetCurrency(string newCurrencyId)
        {
            currencyId = newCurrencyId;
            // No numeric change here; if you want currency-specific mapping,
            // you can extend this method later.
        }

        /// <summary>
        /// Switch between game/global wallet at runtime.
        /// </summary>
        public void SetWalletType(WalletType type)
        {
            walletType = type;
            RefreshBalanceFromSnapshot();
        }

        #endregion
    }
}
