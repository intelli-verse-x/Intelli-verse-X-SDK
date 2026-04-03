using System.Threading.Tasks;
using IntelliVerseX.Hiro;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace {{game_name}}.UI
{
    /// <summary>
    /// Global vs friends Hiro leaderboards: loads board list, fetches records, and submits scores.
    /// </summary>
    public sealed class LeaderboardPanel : MonoBehaviour
    {
        #region Serialized Fields

        [SerializeField] private Button _tabGlobal;
        [SerializeField] private Button _tabFriends;
        [SerializeField] private RectTransform _listRoot;
        [SerializeField] private GameObject _rowPrefab;
        [SerializeField] private string _friendsLeaderboardId = "";
        [SerializeField] private TMP_InputField _scoreInput;
        [SerializeField] private Button _submitButton;

        #endregion

        #region Private Fields

        private bool _friendsMode;
        private string _activeBoardId;

        #endregion

        #region Unity Lifecycle

        private void OnEnable()
        {
            _tabGlobal?.onClick.AddListener(() => _ = SwitchTabAsync(false));
            _tabFriends?.onClick.AddListener(() => _ = SwitchTabAsync(true));
            _submitButton?.onClick.AddListener(() => _ = SubmitAsync());
            _ = SwitchTabAsync(false);
        }

        private void OnDisable()
        {
            _tabGlobal?.onClick.RemoveAllListeners();
            _tabFriends?.onClick.RemoveAllListeners();
            _submitButton?.onClick.RemoveAllListeners();
        }

        #endregion

        #region Private Methods

        private async Task SwitchTabAsync(bool friends)
        {
            _friendsMode = friends;
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized || _listRoot == null || _rowPrefab == null)
                return;

            var boards = await hi.Leaderboards.ListAsync("{{game_id}}");
            if (boards.leaderboards == null || boards.leaderboards.Count == 0)
                return;

            _activeBoardId = friends && !string.IsNullOrEmpty(_friendsLeaderboardId)
                ? _friendsLeaderboardId
                : boards.leaderboards[0].id;

            var records = await hi.Leaderboards.GetRecordsAsync(_activeBoardId, 20, null, null, "{{game_id}}");
            for (var i = _listRoot.childCount - 1; i >= 0; i--)
                Destroy(_listRoot.GetChild(i).gameObject);

            foreach (var r in records.records)
            {
                var row = Instantiate(_rowPrefab, _listRoot);
                var rank = row.transform.Find("Rank")?.GetComponent<TextMeshProUGUI>();
                var name = row.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();
                var score = row.transform.Find("Score")?.GetComponent<TextMeshProUGUI>();
                if (rank != null)
                    rank.text = r.rank.ToString();
                if (name != null)
                    name.text = string.IsNullOrEmpty(r.username) ? r.userId : r.username;
                if (score != null)
                    score.text = r.score.ToString();
            }
        }

        private async Task SubmitAsync()
        {
            var hi = IVXHiroCoordinator.Instance;
            if (hi == null || !hi.IsInitialized || string.IsNullOrEmpty(_activeBoardId))
                return;

            long score = 0;
            if (_scoreInput != null && long.TryParse(_scoreInput.text, out var parsed))
                score = parsed;

            await hi.Leaderboards.SubmitScoreAsync(_activeBoardId, score, 0, null, null, "{{game_id}}");
            await SwitchTabAsync(_friendsMode);
        }

        #endregion
    }
}
