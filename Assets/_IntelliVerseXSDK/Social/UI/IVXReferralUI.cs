using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace IntelliVerseX.Social.UI
{
    /// <summary>
    /// UI component for referral system (Invite & Earn).
    /// Provides ready-to-use buttons and displays for:
    /// - Share referral code
    /// - Copy referral link
    /// - Display referral stats
    /// - Claim rewards
    /// </summary>
    public class IVXReferralUI : MonoBehaviour
    {
        #region Inspector Fields
        [Header("Buttons")]
        [SerializeField] private Button shareButton;
        [SerializeField] private Button copyButton;
        [SerializeField] private Button claimRewardsButton;
        [SerializeField] private Button refreshStatsButton;

        [Header("Text Displays")]
        [SerializeField] private TextMeshProUGUI referralCodeText;
        [SerializeField] private TextMeshProUGUI referralUrlText;
        [SerializeField] private TextMeshProUGUI totalReferralsText;
        [SerializeField] private TextMeshProUGUI completedReferralsText;
        [SerializeField] private TextMeshProUGUI pendingReferralsText;
        [SerializeField] private TextMeshProUGUI expiredReferralsText;
        [SerializeField] private TextMeshProUGUI statusText;

        [Header("Loading Indicators")]
        [SerializeField] private GameObject loadingPanel;
        [SerializeField] private GameObject statsPanel;

        [Header("Settings")]
        [SerializeField] private bool autoLoadOnStart = true;
        [SerializeField] private string shareMessageTemplate = "Join me on IntelliVerseX! Use my referral code: {CODE}\n{URL}";
        [SerializeField] private Color successColor = Color.green;
        [SerializeField] private Color errorColor = Color.red;
        [SerializeField] private Color infoColor = Color.white;
        #endregion

        #region Unity Lifecycle
        private void Start()
        {
            SetupButtons();
            
            if (autoLoadOnStart)
            {
                LoadReferralData();
            }
        }

        private void OnDestroy()
        {
            RemoveButtonListeners();
        }
        #endregion

        #region Button Setup
        private void SetupButtons()
        {
            if (shareButton != null)
                shareButton.onClick.AddListener(OnShareButtonClicked);
            
            if (copyButton != null)
                copyButton.onClick.AddListener(OnCopyButtonClicked);
            
            if (claimRewardsButton != null)
                claimRewardsButton.onClick.AddListener(OnClaimRewardsButtonClicked);
            
            if (refreshStatsButton != null)
                refreshStatsButton.onClick.AddListener(OnRefreshStatsButtonClicked);
        }

        private void RemoveButtonListeners()
        {
            if (shareButton != null)
                shareButton.onClick.RemoveListener(OnShareButtonClicked);
            
            if (copyButton != null)
                copyButton.onClick.RemoveListener(OnCopyButtonClicked);
            
            if (claimRewardsButton != null)
                claimRewardsButton.onClick.RemoveListener(OnClaimRewardsButtonClicked);
            
            if (refreshStatsButton != null)
                refreshStatsButton.onClick.RemoveListener(OnRefreshStatsButtonClicked);
        }
        #endregion

        #region Public Methods
    /// <summary>
    /// Load referral data (URL and stats).
    /// </summary>
    public async void LoadReferralData()
    {
        ShowLoading(true);
        SetStatus("Loading referral data...", infoColor);

        try
        {
            // Load referral URL
            var urlResponse = await APIManager.GetReferralUrlAsync(null);
            
            if (urlResponse.status && urlResponse.data != null)
            {
                UpdateReferralDisplay(urlResponse.data);
            }
            else
            {
                SetStatus($"Failed to load referral URL: {urlResponse.message}", errorColor);
                ShowLoading(false);
                return;
            }

            // Load referral stats
            var statsResponse = await APIManager.GetReferralStatsAsync(null);
            
            if (statsResponse.status && statsResponse.data != null)
            {
                UpdateStatsDisplay(statsResponse.data);
                SetStatus("Referral data loaded!", successColor);
            }
            else
            {
                SetStatus($"Failed to load stats: {statsResponse.message}", errorColor);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", errorColor);
            Debug.LogError($"[IVXReferralUI] Load error: {ex.Message}");
        }
        finally
        {
            ShowLoading(false);
        }
    }

    /// <summary>
    /// Refresh stats only.
    /// </summary>
    public async void RefreshStats()
    {
        SetStatus("Refreshing stats...", infoColor);

        try
        {
            var response = await APIManager.GetReferralStatsAsync(null);
            
            if (response.status && response.data != null)
            {
                UpdateStatsDisplay(response.data);
                SetStatus("Stats refreshed!", successColor);
            }
            else
            {
                SetStatus($"Failed to refresh: {response.message}", errorColor);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", errorColor);
        }
    }
        #endregion

        #region Button Handlers
    private async void OnShareButtonClicked()
    {
        try
        {
            // Get referral data if not loaded
            var urlResponse = await APIManager.GetReferralUrlAsync(null);
            
            if (!urlResponse.status || urlResponse.data == null)
            {
                SetStatus("Failed to get referral data", errorColor);
                return;
            }

            string code = urlResponse.data.referralCode;
            string url = urlResponse.data.referralUrl;
            string message = shareMessageTemplate.Replace("{CODE}", code).Replace("{URL}", url);

            // Use static helper for native share
            IVXNativeShareHelper.ShareReferralCode(code, url, message);
            SetStatus("Share dialog opened!", successColor);
        }
        catch (Exception ex)
        {
            SetStatus($"Share failed: {ex.Message}", errorColor);
        }
    }

    private async void OnCopyButtonClicked()
    {
        try
        {
            var urlResponse = await APIManager.GetReferralUrlAsync(null);
            
            if (urlResponse.status && urlResponse.data != null)
            {
                IVXNativeShareHelper.CopyToClipboard(urlResponse.data.referralUrl);
                SetStatus("Referral link copied to clipboard!", successColor);
            }
            else
            {
                SetStatus("Failed to get referral URL", errorColor);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Copy failed: {ex.Message}", errorColor);
        }
    }

    private async void OnClaimRewardsButtonClicked()
    {
        if (claimRewardsButton != null)
            claimRewardsButton.interactable = false;

        SetStatus("Claiming rewards...", infoColor);

        try
        {
            // Claim all completed referrals
            var response = await APIManager.ClaimReferralRewardsAsync(null, null);
            
            if (response.status && response.data != null)
            {
                SetStatus($"Claimed {response.data.totalClaimed} rewards! Total: {response.data.totalRewardAmount} {response.data.rewardCurrency}", successColor);
                
                // Refresh stats
                var statsResponse = await APIManager.GetReferralStatsAsync(null);
                if (statsResponse.status && statsResponse.data != null)
                {
                    UpdateStatsDisplay(statsResponse.data);
                }
            }
            else
            {
                SetStatus($"Claim failed: {response.message}", errorColor);
            }
        }
        catch (Exception ex)
        {
            SetStatus($"Error: {ex.Message}", errorColor);
        }
        finally
        {
            if (claimRewardsButton != null)
                claimRewardsButton.interactable = true;
        }
    }

        private void OnRefreshStatsButtonClicked()
        {
            RefreshStats();
        }
        #endregion

        #region UI Updates
    private void UpdateReferralDisplay(APIManager.ReferralUrlData data)
    {
        if (referralCodeText != null)
            referralCodeText.text = data.referralCode;
        
        if (referralUrlText != null)
            referralUrlText.text = data.referralUrl;
    }

    private void UpdateStatsDisplay(APIManager.ReferralStatsData data)
    {
        if (data == null) return;

        if (totalReferralsText != null)
            totalReferralsText.text = data.totalReferrals.ToString();
        
        if (completedReferralsText != null)
            completedReferralsText.text = data.completedReferrals.ToString();
        
        if (pendingReferralsText != null)
            pendingReferralsText.text = data.pendingReferrals.ToString();
        
        if (expiredReferralsText != null)
            expiredReferralsText.text = data.expiredReferrals.ToString();

        // Enable claim button if there are completed referrals
        if (claimRewardsButton != null)
            claimRewardsButton.interactable = data.completedReferrals > 0;
    }

        private void ShowLoading(bool show)
        {
            if (loadingPanel != null)
                loadingPanel.SetActive(show);
            
            if (statsPanel != null)
                statsPanel.SetActive(!show);
        }

        private void SetStatus(string message, Color color)
        {
            if (statusText != null)
            {
                statusText.text = message;
                statusText.color = color;
            }
            
            Debug.Log($"[IVXReferralUI] {message}");
        }
        #endregion
    }
}
