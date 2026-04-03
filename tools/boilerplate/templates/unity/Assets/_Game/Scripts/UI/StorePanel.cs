using System.Collections.Generic;
using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using IntelliVerseX.Hiro.Systems;
using IntelliVerseX.Satori;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Loads Hiro store sections via <see cref="IVXHiroCoordinator"/>, renders rows under a grid root,
    /// and purchases through <see cref="IVXStoreSystem.PurchaseAsync"/>.
    /// </summary>
    public sealed class StorePanel : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private RectTransform _gridRoot;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private GameObject _rewardPopup;
        [SerializeField] private TextMeshProUGUI _rewardSummary;

        #endregion

        #region Unity Lifecycle

        private async void OnEnable()
        {
            await ReloadAsync();
        }

        #endregion

        #region Private Methods

        private async Task ReloadAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized || _gridRoot == null || _rowPrefab == null)
                return;

            for (var i = _gridRoot.childCount - 1; i >= 0; i--)
                Destroy(_gridRoot.GetChild(i).gameObject);

            var list = await hi.Store.ListAsync("{{game_id}}");
            foreach (var section in list.sections)
            {
                foreach (var item in section.items)
                {
                    var row = Instantiate(_rowPrefab, _gridRoot);
                    BindRow(row, section.sectionId, item);
                }
            }
        }

        private void BindRow(GameObject row, string sectionId, IVXStoreItem item)
        {
            var title = row.transform.Find("Title")?.GetComponent<TextMeshProUGUI>();
            var cost = row.transform.Find("Cost")?.GetComponent<TextMeshProUGUI>();
            var buy = row.transform.Find("Buy")?.GetComponent<Button>();
            if (title != null)
                title.text = item.name;
            if (cost != null)
                cost.text = FormatCost(item.cost);
            buy?.onClick.AddListener(() => _ = PurchaseAsync(sectionId, item.itemId));
        }

        private static string FormatCost(Dictionary<string, long> cost)
        {
            if (cost == null || cost.Count == 0)
                return "—";
            foreach (var kv in cost)
                return $"{kv.Value} {kv.Key}";
            return "—";
        }

        private async Task PurchaseAsync(string sectionId, string itemId)
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized)
                return;

            var result = await hi.Store.PurchaseAsync(sectionId, itemId, "{{game_id}}");
            if (result == null)
                return;

            if (_rewardPopup != null)
                _rewardPopup.SetActive(true);
            if (_rewardSummary != null)
                _rewardSummary.text = "Purchase complete!";

            var satori = IVXSatoriClient.Instance;
            if (satori != null && satori.IsInitialized)
                _ = satori.CaptureEventAsync("store_purchase", new Dictionary<string, string>
                {
                    { "game_id", "{{game_id}}" },
                    { "item_id", itemId },
                    { "section_id", sectionId },
                });
        }

        #endregion
    }
}
