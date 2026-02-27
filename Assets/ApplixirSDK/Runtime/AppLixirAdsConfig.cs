using System;
using UnityEngine;

namespace ApplixirSDK.Runtime
{
    [CreateAssetMenu(fileName = "AppLixirAdsConfig", menuName = "AppLixir/AdsConfig")]
    public class AppLixirAdsConfig : ScriptableObject
    {
        private const string ApplixirUserId = "d0c89e2ab5dc38b78958ba6ee567e0af13d3a655";

        public string apiKey;
        public LogLevel logLevel = LogLevel.None;
        public PlayVideoResult debugPlayVideoResponse = PlayVideoResult.unavailable;

        public string UserId
        {
            get
            {
                if (string.IsNullOrEmpty(_userId))
                {
                    GetOrCreateUserId();
                }

                return _userId;
            }
        }

        private string _userId;
        
        private void GetOrCreateUserId()
        {
            _userId = PlayerPrefs.GetString(ApplixirUserId, null);
            if (string.IsNullOrEmpty(_userId))
            {
                _userId = Guid.NewGuid().ToString();
                PlayerPrefs.SetString(ApplixirUserId, _userId);
            }
        }
    }
}