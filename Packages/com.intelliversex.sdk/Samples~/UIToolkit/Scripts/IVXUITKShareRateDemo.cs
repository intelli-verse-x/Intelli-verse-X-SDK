using System;
using IntelliVerseX.Games.Social;
using IntelliVerseX.Social;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK Share & Rate Demo")]
    public sealed class IVXUITKShareRateDemo : IVXUITKFeatureDemoBase
    {
        protected override void BuildFeatureUI()
        {
            SetTitle("Share & Rate", "IVXGShareManager + IVXGRateAppManager");
            EnsureManagers();
            AddAction("Share Text", ShareText);
            AddAction("Share Screenshot", ShareScreenshot);
            AddAction("Rate App", RateApp, "ivx-btn--accent");
            SetStatus("Share & Rate demo ready.");
        }

        private void EnsureManagers()
        {
            if (IVXGShareManager.Instance == null)
            {
                var shareGo = new GameObject("[IVXGShareManager]");
                shareGo.AddComponent<IVXGShareManager>();
                DontDestroyOnLoad(shareGo);
            }

            if (IVXGRateAppManager.Instance == null)
            {
                var rateGo = new GameObject("[IVXGRateAppManager]");
                rateGo.AddComponent<IVXGRateAppManager>();
                DontDestroyOnLoad(rateGo);
            }
        }

        private void ShareText()
        {
            EnsureManagers();
            try
            {
                var share = IVXGShareManager.Instance;
                if (share != null)
                {
                    share.ShareText("Check out my IntelliVerseX game!", null,
                        ok => SetStatus(ok ? "Share text OK." : "Share cancelled.", !ok));
                }
                else
                {
                    IVXShareService.ShareText("Check out my IntelliVerseX game!", null,
                        ok => SetStatus(ok ? "Share text OK." : "Share cancelled.", !ok));
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void ShareScreenshot()
        {
            EnsureManagers();
            try
            {
                var share = IVXGShareManager.Instance;
                if (share != null)
                {
                    share.ShareWithScreenshot("Playing IntelliVerseX!", null,
                        ok => SetStatus(ok ? "Screenshot share OK." : "Share cancelled.", !ok));
                }
                else
                {
                    SetStatus("Share manager missing.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }

        private void RateApp()
        {
            EnsureManagers();
            try
            {
                var rate = IVXGRateAppManager.Instance;
                if (rate != null)
                {
                    rate.ForceShowRatePrompt();
                    SetStatus("Rate prompt requested.");
                }
                else
                {
                    SetStatus("Rate manager missing.", true);
                }
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }
    }
}
