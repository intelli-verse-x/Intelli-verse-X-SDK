using System;
using System.Threading.Tasks;
using IntelliVerseX.MoreOfUs;
using UnityEngine;
using UnityEngine.UIElements;

namespace IntelliVerseX.Samples.UIToolkit
{
    [AddComponentMenu("IntelliVerse-X/Samples/UITK More Of Us Demo")]
    public sealed class IVXUITKMoreOfUsDemo : IVXUITKFeatureDemoBase
    {
        protected override void BuildFeatureUI()
        {
            SetTitle("More Of Us", "App catalog via IVXMoreOfUsManager");
            AddAction("Fetch Catalog", () => _ = FetchAsync(false));
            AddAction("Force Refresh", () => _ = FetchAsync(true));
            AddAction("List Platform Apps", ListApps);
            SetStatus("More Of Us demo ready.");
        }

        private async Task FetchAsync(bool force)
        {
            SetBusy(true);
            try
            {
                var catalog = await IVXMoreOfUsManager.Instance.FetchCatalogAsync(force);
                int count = catalog?.apps?.Count ?? 0;
                SetResult($"Catalog apps: {count}");
                SetStatus(force ? "Catalog force-refreshed." : "Catalog fetched.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
            finally
            {
                SetBusy(false);
            }
        }

        private void ListApps()
        {
            try
            {
                var apps = IVXMoreOfUsManager.Instance.GetAppsForCurrentPlatform();
                SetResult($"Platform apps: {apps?.Count ?? 0}");
                SetStatus("Listed platform apps.");
            }
            catch (Exception ex)
            {
                SetStatus(ex.Message, true);
            }
        }
    }
}
