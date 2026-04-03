using IntelliVerseX.Backend;
using TMPro;
using UnityEngine;

namespace {{game_name}}.UI
{
    /// <summary>
    /// HUD for game (coins) and global (gems) balances. Uses <see cref="IVXWalletManager"/>
    /// (game wallet = coins, global wallet = gems) and mirrors the economy surface described
    /// for IVXEconomyManager-style flows.
    /// </summary>
    public sealed class WalletDisplay : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Counters")]
        [SerializeField] private TextMeshProUGUI _coinText;
        [SerializeField] private TextMeshProUGUI _gemText;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            IVXWalletManager.OnBalanceChanged += OnBalanceChanged;
        }

        private void Start()
        {
            Refresh(IVXWalletManager.GetGameBalance(), IVXWalletManager.GetGlobalBalance());
        }

        private void OnDisable()
        {
            IVXWalletManager.OnBalanceChanged -= OnBalanceChanged;
        }

        #endregion

        #region Private Methods

        private void OnBalanceChanged(int gameBalance, int globalBalance)
        {
            Refresh(gameBalance, globalBalance);
        }

        private void Refresh(int coins, int gems)
        {
            if (_coinText != null)
                _coinText.text = coins.ToString();
            if (_gemText != null)
                _gemText.text = gems.ToString();
        }

        #endregion
    }
}
