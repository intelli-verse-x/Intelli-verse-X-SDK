using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using IntelliVerseX.Core;

namespace IntelliVerseX.Examples
{
    /// <summary>
    /// Production-ready wallet test controller for the IVX_WalletTest scene.
    /// 
    /// Features:
    /// - Add/Spend from Game Wallet
    /// - Add/Spend from Global Wallet
    /// - Real-time balance display
    /// - Input validation
    /// - Nakama backend integration via IVXNWalletManager
    /// 
    /// Usage:
    /// 1. Attach to a GameObject in the IVX_WalletTest scene
    /// 2. Assign UI references in the Inspector
    /// 3. Ensure Nakama backend is configured and user is authenticated
    /// </summary>
    public class IVXWalletTestController : MonoBehaviour
    {
        #region Serialized Fields

        [Header("Input")]
        [SerializeField] private TMP_InputField _amountInputField;

        [Header("Buttons")]
        [SerializeField] private Button _addToGameWalletButton;
        [SerializeField] private Button _spendFromGameWalletButton;
        [SerializeField] private Button _addToGlobalWalletButton;
        [SerializeField] private Button _spendFromGlobalWalletButton;
        [SerializeField] private Button _refreshButton;

        [Header("Balance Display")]
        [SerializeField] private TMP_Text _gameWalletBalanceText;
        [SerializeField] private TMP_Text _globalWalletBalanceText;

        [Header("Status")]
        [SerializeField] private TMP_Text _statusText;

        [Header("Configuration")]
        [SerializeField] private long _defaultAmount = 100;
        [SerializeField] private bool _autoRefreshOnStart = true;

        #endregion

        #region Private Fields

        private const string LOG_PREFIX = "[IVX-WalletTest]";
        private bool _isProcessing;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ValidateReferences();
        }

        private void OnEnable()
        {
            SubscribeToEvents();
        }

        private void OnDisable()
        {
            UnsubscribeFromEvents();
        }

        private async void Start()
        {
            SetupUI();
            
            if (_autoRefreshOnStart)
            {
                await RefreshBalancesAsync();
            }
            else
            {
                UpdateBalanceDisplay(IVXNWalletManager.GameBalance, IVXNWalletManager.GlobalBalance);
            }
        }

        #endregion

        #region Setup

        private void ValidateReferences()
        {
            if (_amountInputField == null)
                Debug.LogWarning($"{LOG_PREFIX} Amount InputField not assigned.");
            
            if (_addToGameWalletButton == null)
                Debug.LogWarning($"{LOG_PREFIX} Add to Game Wallet button not assigned.");
            
            if (_spendFromGameWalletButton == null)
                Debug.LogWarning($"{LOG_PREFIX} Spend from Game Wallet button not assigned.");
            
            if (_addToGlobalWalletButton == null)
                Debug.LogWarning($"{LOG_PREFIX} Add to Global Wallet button not assigned.");
            
            if (_spendFromGlobalWalletButton == null)
                Debug.LogWarning($"{LOG_PREFIX} Spend from Global Wallet button not assigned.");
            
            if (_gameWalletBalanceText == null)
                Debug.LogWarning($"{LOG_PREFIX} Game Wallet Balance text not assigned.");
            
            if (_globalWalletBalanceText == null)
                Debug.LogWarning($"{LOG_PREFIX} Global Wallet Balance text not assigned.");
        }

        private void SetupUI()
        {
            if (_amountInputField != null)
            {
                _amountInputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                _amountInputField.text = _defaultAmount.ToString();
            }

            if (_addToGameWalletButton != null)
                _addToGameWalletButton.onClick.AddListener(OnAddToGameWalletClicked);
            
            if (_spendFromGameWalletButton != null)
                _spendFromGameWalletButton.onClick.AddListener(OnSpendFromGameWalletClicked);
            
            if (_addToGlobalWalletButton != null)
                _addToGlobalWalletButton.onClick.AddListener(OnAddToGlobalWalletClicked);
            
            if (_spendFromGlobalWalletButton != null)
                _spendFromGlobalWalletButton.onClick.AddListener(OnSpendFromGlobalWalletClicked);

            if (_refreshButton != null)
                _refreshButton.onClick.AddListener(OnRefreshClicked);

            SetStatus("Ready. Enter an amount and tap a button.");
        }

        private void SubscribeToEvents()
        {
            IVXNWalletManager.OnWalletBalanceChanged += UpdateBalanceDisplay;
            IVXNWalletManager.OnWalletOperationCompleted += OnOperationCompleted;
        }

        private void UnsubscribeFromEvents()
        {
            IVXNWalletManager.OnWalletBalanceChanged -= UpdateBalanceDisplay;
            IVXNWalletManager.OnWalletOperationCompleted -= OnOperationCompleted;
        }

        #endregion

        #region Button Handlers

        private async void OnAddToGameWalletClicked()
        {
            if (_isProcessing) return;

            long amount = GetInputAmount();
            if (amount <= 0)
            {
                SetStatus("Please enter a valid positive amount.");
                return;
            }

            _isProcessing = true;
            SetButtonsInteractable(false);
            SetStatus($"Adding {amount} to Game Wallet...");

            try
            {
                bool success = await IVXNWalletManager.CreditGameAsync(amount, "Wallet Test - Add");
                
                if (success)
                {
                    SetStatus($"Successfully added {amount} to Game Wallet!");
                    Debug.Log($"{LOG_PREFIX} Added {amount} to Game Wallet. New balance: {IVXNWalletManager.GameBalance}");
                }
                else
                {
                    SetStatus("Failed to add to Game Wallet. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
                Debug.LogError($"{LOG_PREFIX} Error adding to Game Wallet: {ex}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async void OnSpendFromGameWalletClicked()
        {
            if (_isProcessing) return;

            long amount = GetInputAmount();
            if (amount <= 0)
            {
                SetStatus("Please enter a valid positive amount.");
                return;
            }

            if (IVXNWalletManager.GameBalance < amount)
            {
                SetStatus($"Insufficient Game Wallet balance. Have: {IVXNWalletManager.GameBalance}, Need: {amount}");
                return;
            }

            _isProcessing = true;
            SetButtonsInteractable(false);
            SetStatus($"Spending {amount} from Game Wallet...");

            try
            {
                bool success = await IVXNWalletManager.TrySpendGameAsync(amount, "Wallet Test - Spend");
                
                if (success)
                {
                    SetStatus($"Successfully spent {amount} from Game Wallet!");
                    Debug.Log($"{LOG_PREFIX} Spent {amount} from Game Wallet. New balance: {IVXNWalletManager.GameBalance}");
                }
                else
                {
                    SetStatus("Failed to spend from Game Wallet. Insufficient funds or server error.");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
                Debug.LogError($"{LOG_PREFIX} Error spending from Game Wallet: {ex}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async void OnAddToGlobalWalletClicked()
        {
            if (_isProcessing) return;

            long amount = GetInputAmount();
            if (amount <= 0)
            {
                SetStatus("Please enter a valid positive amount.");
                return;
            }

            _isProcessing = true;
            SetButtonsInteractable(false);
            SetStatus($"Adding {amount} to Global Wallet...");

            try
            {
                bool success = await IVXNWalletManager.CreditGlobalAsync(amount, "Wallet Test - Add");
                
                if (success)
                {
                    SetStatus($"Successfully added {amount} to Global Wallet!");
                    Debug.Log($"{LOG_PREFIX} Added {amount} to Global Wallet. New balance: {IVXNWalletManager.GlobalBalance}");
                }
                else
                {
                    SetStatus("Failed to add to Global Wallet. Check console for details.");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
                Debug.LogError($"{LOG_PREFIX} Error adding to Global Wallet: {ex}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async void OnSpendFromGlobalWalletClicked()
        {
            if (_isProcessing) return;

            long amount = GetInputAmount();
            if (amount <= 0)
            {
                SetStatus("Please enter a valid positive amount.");
                return;
            }

            if (IVXNWalletManager.GlobalBalance < amount)
            {
                SetStatus($"Insufficient Global Wallet balance. Have: {IVXNWalletManager.GlobalBalance}, Need: {amount}");
                return;
            }

            _isProcessing = true;
            SetButtonsInteractable(false);
            SetStatus($"Spending {amount} from Global Wallet...");

            try
            {
                bool success = await IVXNWalletManager.TrySpendGlobalAsync(amount, "Wallet Test - Spend");
                
                if (success)
                {
                    SetStatus($"Successfully spent {amount} from Global Wallet!");
                    Debug.Log($"{LOG_PREFIX} Spent {amount} from Global Wallet. New balance: {IVXNWalletManager.GlobalBalance}");
                }
                else
                {
                    SetStatus("Failed to spend from Global Wallet. Insufficient funds or server error.");
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Error: {ex.Message}");
                Debug.LogError($"{LOG_PREFIX} Error spending from Global Wallet: {ex}");
            }
            finally
            {
                _isProcessing = false;
                SetButtonsInteractable(true);
            }
        }

        private async void OnRefreshClicked()
        {
            if (_isProcessing) return;

            _isProcessing = true;
            SetButtonsInteractable(false);
            SetStatus("Refreshing balances from server...");

            await RefreshBalancesAsync();

            _isProcessing = false;
            SetButtonsInteractable(true);
        }

        #endregion

        #region Helpers

        private long GetInputAmount()
        {
            if (_amountInputField == null) return _defaultAmount;

            string text = _amountInputField.text;
            if (string.IsNullOrWhiteSpace(text)) return 0;

            if (long.TryParse(text, out long amount))
            {
                return Math.Abs(amount);
            }

            return 0;
        }

        private void UpdateBalanceDisplay(long gameBalance, long globalBalance)
        {
            if (_gameWalletBalanceText != null)
            {
                _gameWalletBalanceText.text = $"Game Wallet: {gameBalance:N0}";
            }

            if (_globalWalletBalanceText != null)
            {
                _globalWalletBalanceText.text = $"Global Wallet: {globalBalance:N0}";
            }

            Debug.Log($"{LOG_PREFIX} Balance updated - Game: {gameBalance}, Global: {globalBalance}");
        }

        private void OnOperationCompleted(bool success, string errorMessage)
        {
            if (!success && !string.IsNullOrEmpty(errorMessage))
            {
                Debug.LogWarning($"{LOG_PREFIX} Operation failed: {errorMessage}");
            }
        }

        private void SetStatus(string message)
        {
            if (_statusText != null)
            {
                _statusText.text = message;
            }
            Debug.Log($"{LOG_PREFIX} Status: {message}");
        }

        private void SetButtonsInteractable(bool interactable)
        {
            if (_addToGameWalletButton != null) 
                _addToGameWalletButton.interactable = interactable;
            
            if (_spendFromGameWalletButton != null) 
                _spendFromGameWalletButton.interactable = interactable;
            
            if (_addToGlobalWalletButton != null) 
                _addToGlobalWalletButton.interactable = interactable;
            
            if (_spendFromGlobalWalletButton != null) 
                _spendFromGlobalWalletButton.interactable = interactable;

            if (_refreshButton != null)
                _refreshButton.interactable = interactable;
        }

        private async Task RefreshBalancesAsync()
        {
            try
            {
                bool success = await IVXNWalletManager.RefreshBalancesAsync();
                
                if (success)
                {
                    SetStatus("Balances refreshed successfully!");
                }
                else
                {
                    SetStatus("Could not refresh balances. Using cached values.");
                    UpdateBalanceDisplay(IVXNWalletManager.GameBalance, IVXNWalletManager.GlobalBalance);
                }
            }
            catch (Exception ex)
            {
                SetStatus($"Refresh error: {ex.Message}");
                Debug.LogError($"{LOG_PREFIX} Refresh error: {ex}");
                UpdateBalanceDisplay(IVXNWalletManager.GameBalance, IVXNWalletManager.GlobalBalance);
            }
        }

        #endregion
    }
}
