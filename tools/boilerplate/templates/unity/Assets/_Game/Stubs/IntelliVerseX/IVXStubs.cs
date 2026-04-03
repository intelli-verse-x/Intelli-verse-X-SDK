using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace IntelliVerseX.Core
{
    public class IVXBootstrap : MonoBehaviour
    {
        public static IVXBootstrap Instance { get; private set; }
        public bool IsReady { get; set; }
        public event Action OnBootstrapComplete;
        private void Awake() { Instance = this; }
    }
}

namespace IntelliVerseX.Identity
{
    public class IVXAuthManager : MonoBehaviour
    {
        public static IVXAuthManager Instance { get; private set; }
        public bool IsAuthenticated { get; set; }
        private void Awake() { Instance = this; }
        
        public Task SignOutAsync() => Task.CompletedTask;
        public Task SignInGuestAsync() => Task.CompletedTask;
        public Task SignInEmailAsync(string email, string password) => Task.CompletedTask;
        public Task RegisterEmailAsync(string email, string password, string username) => Task.CompletedTask;
        public Task SignInSocialAsync(string provider) => Task.CompletedTask;
    }
}

namespace IntelliVerseX.Backend
{
    public static class IVXWalletManager
    {
        public delegate void BalanceChangedHandler(int gameBalance, int globalBalance);
        public static event BalanceChangedHandler OnBalanceChanged;
        
        public static int GetGameBalance() => 0;
        public static int GetGlobalBalance() => 0;
    }
}

namespace IntelliVerseX.Hiro
{
    public class IVXHiroCoordinator : MonoBehaviour
    {
        public static IVXHiroCoordinator Instance { get; private set; }
        private void Awake() { Instance = this; }
        public Task InitializeAllAsync() => Task.CompletedTask;
    }

    public class IVXAchievementManager : MonoBehaviour
    {
        public static IVXAchievementManager Instance { get; private set; }
        public event Action<IVXAchievement> OnAchievementUnlocked;
        private void Awake() { Instance = this; }
    }

    public class IVXFriendStreakManager : MonoBehaviour
    {
        public static IVXFriendStreakManager Instance { get; private set; }
        public event Action<IVXFriendStreak> OnStreakUpdated;
        private void Awake() { Instance = this; }
    }

    public class IVXSeasonPassManager : MonoBehaviour
    {
        public static IVXSeasonPassManager Instance { get; private set; }
        public event Action<IVXSeasonPassState> OnLevelUp;
        private void Awake() { Instance = this; }
    }

    public class IVXAchievement 
    {
        public string id;
        public string title;
    }
    public class IVXFriendStreak 
    {
        public string friendId;
        public int currentStreak;
    }
    public class IVXSeasonPassState 
    {
        public int currentLevel;
    }
    public class IVXStoreItem 
    {
        public string id;
        public string name;
        public string description;
        public int cost;
        public string currency;
    }
    public class IVXStreak 
    {
        public int CurrentStreak => 0;
        public bool CanClaimToday => false;
        public bool HasClaimedToday => false;
        public DateTime NextClaimTime => DateTime.UtcNow;
    }
    
    public class IVXStoreSystem : MonoBehaviour
    {
        public static IVXStoreSystem Instance { get; private set; }
        private void Awake() { Instance = this; }
        public Task PurchaseAsync(string sectionId, string itemId) => Task.CompletedTask;
    }
}

namespace IntelliVerseX.Satori
{
    public class IVXSatoriClient : MonoBehaviour
    {
        public static IVXSatoriClient Instance { get; private set; }
        public bool IsInitialized { get; set; }
        private void Awake() { Instance = this; }
        
        public void TrackEvent(string name, Dictionary<string, string> properties = null) {}
        public Task CaptureEventAsync(string name, Dictionary<string, string> properties = null) => Task.CompletedTask;
        public void IdentifyAsync() {}
    }
}

namespace IntelliVerseX.Analytics
{
    // Dummy alias to resolve compilation if AuthFlowController uses IntelliVerseX.Analytics for IVXSatoriClient
    public class IVXSatoriClient : IntelliVerseX.Satori.IVXSatoriClient {}
}
