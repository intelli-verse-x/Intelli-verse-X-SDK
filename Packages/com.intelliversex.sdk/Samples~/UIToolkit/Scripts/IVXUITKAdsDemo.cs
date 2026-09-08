using System;
using IntelliVerseX.Monetization;
using IntelliVerseX.Monetization.Ads;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Ads Demo")]
    public sealed class IVXUITKAdsDemo : IVXUITKFeatureDemoBase
    {
        protected override void BuildFeatureUI()
        {
            SetTitle("Ads", "Rewarded / interstitial via IVXAdsManager");
            AddAction("Init Ads", InitAds);
            AddAction("Show Rewarded", ShowRewarded);
            AddAction("Show Interstitial", ShowInterstitial);
            AddAction("Log Status", LogStatus, "ivx-btn--ghost");
            SetStatus(IVXAdsManager.IsInitialized() ? "Ads already initialized." : "Ads not initialized.");
        }

        private void InitAds()
        {
            try
            {
                IVXAdsManager.Initialize();
                SetStatus(IVXAdsManager.IsInitialized() ? "Ads initialized." : "Init called (check logs).");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void ShowRewarded()
        {
            try
            {
                if (!IVXAdsManager.IsInitialized())
                    IVXAdsManager.Initialize();

                IVXAdsManager.ShowRewardedAd((ok, reward) =>
                {
                    SetStatus(ok ? $"Rewarded complete (+{reward})." : "Rewarded cancelled/failed.", !ok);
                    SetResult($"Last reward amount: {reward}");
                });
                SetStatus("Rewarded requested...");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void ShowInterstitial()
        {
            try
            {
                if (!IVXAdsManager.IsInitialized())
                    IVXAdsManager.Initialize();

                IVXAdsManager.ShowInterstitialAd(ok =>
                {
                    SetStatus(ok ? "Interstitial complete." : "Interstitial cancelled/failed.", !ok);
                });
                SetStatus("Interstitial requested...");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void LogStatus()
        {
            bool ready = IVXAdsManager.IsInitialized();
            bool canInterstitial = false;
            try { canInterstitial = IVXAdsManager.CanShowInterstitial(); } catch { /* optional */ }
            SetResult($"Initialized={ready}, CanShowInterstitial={canInterstitial}");
            SetStatus("Status logged.");
        }
    }
}
