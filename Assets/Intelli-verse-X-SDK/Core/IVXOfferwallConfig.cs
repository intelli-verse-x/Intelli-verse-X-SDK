namespace IntelliVerseX.Core
{
    /// <summary>
    /// Offerwall configuration for IntelliVerse-X platform.
    /// 
    /// Supported Offerwall Networks:
    /// 1. IronSource Offerwall (High eCPM, integrated with IronSource ads)
    /// 2. Tapjoy (Video offers, high engagement)
    /// 3. Fyber (DT FairBid) (Premium offers)
    /// 4. AdGate Media (Survey & offers)
    /// 5. OfferToro (Wide variety)
    /// 6. Pollfish (Survey specialist)
    /// 7. TheoremReach (Survey specialist)
    /// 
    /// Offerwall Types:
    /// - Surveys (pay per completion)
    /// - App installs (CPI)
    /// - Video watching
    /// - In-app purchases
    /// - Quiz/trivia completions
    /// 
    /// Revenue Sharing:
    /// - Network takes 20-40% of offer value
    /// - User gets virtual currency
    /// - You get net revenue
    /// 
    /// Integration with Xsolla/Pollwall:
    /// - Xsolla: Virtual currency management + payment processing
    /// - Pollwall: Survey aggregator (multiple providers)
    /// See OFFERWALL_SETUP_GUIDE.md for complete integration.
    /// 
    /// NOTE: Each network requires separate registration and approval.
    /// </summary>
    public static class IVXOfferwallConfig
    {
        #region IronSource Offerwall Configuration
        
        /// <summary>
        /// IronSource Offerwall uses same app key as ads
        /// Just enable offerwall in IronSource dashboard
        /// No separate configuration needed
        /// </summary>
        public const bool IRONSOURCE_OFFERWALL_ENABLED = true;

        /// <summary>
        /// IronSource offerwall currency name
        /// Shown to users (e.g., "Coins", "Gems")
        /// </summary>
        public const string IRONSOURCE_CURRENCY_NAME = "Coins";

        #endregion

        #region Tapjoy Configuration

        /// <summary>
        /// Tapjoy SDK Key (iOS)
        /// Get from: https://ltv.tapjoy.com/
        /// Format: Long alphanumeric string
        /// </summary>
        public const string TAPJOY_SDK_KEY_IOS = "YOUR_TAPJOY_IOS_SDK_KEY";

        /// <summary>
        /// Tapjoy SDK Key (Android)
        /// </summary>
        public const string TAPJOY_SDK_KEY_ANDROID = "YOUR_TAPJOY_ANDROID_SDK_KEY";

        /// <summary>
        /// Tapjoy placement name for offerwall
        /// Default: "Offerwall" or create custom in dashboard
        /// </summary>
        public const string TAPJOY_OFFERWALL_PLACEMENT = "Offerwall";

        /// <summary>
        /// Enable Tapjoy debug mode
        /// Shows logs and test content
        /// </summary>
        public const bool TAPJOY_DEBUG_MODE = true; // Set false in production

        public static string GetTapjoySdkKey()
        {
            #if UNITY_IOS
                return TAPJOY_SDK_KEY_IOS;
            #elif UNITY_ANDROID
                return TAPJOY_SDK_KEY_ANDROID;
            #else
                return TAPJOY_SDK_KEY_ANDROID;
            #endif
        }

        #endregion

        #region Fyber Configuration

        /// <summary>
        /// Fyber App ID (iOS)
        /// Get from: https://www.fyber.com/
        /// Fyber rebranded to Digital Turbine FairBid
        /// </summary>
        public const string FYBER_APP_ID_IOS = "YOUR_FYBER_IOS_APP_ID";

        /// <summary>
        /// Fyber App ID (Android)
        /// </summary>
        public const string FYBER_APP_ID_ANDROID = "YOUR_FYBER_ANDROID_APP_ID";

        /// <summary>
        /// Fyber Security Token
        /// Used for server-side validation
        /// </summary>
        public const string FYBER_SECURITY_TOKEN = "YOUR_FYBER_SECURITY_TOKEN";

        public static string GetFyberAppId()
        {
            #if UNITY_IOS
                return FYBER_APP_ID_IOS;
            #elif UNITY_ANDROID
                return FYBER_APP_ID_ANDROID;
            #else
                return FYBER_APP_ID_ANDROID;
            #endif
        }

        #endregion

        #region AdGate Media Configuration

        /// <summary>
        /// AdGate Media Wall ID
        /// Get from: https://panel.adgatemedia.com/
        /// Wall ID is numeric
        /// </summary>
        public const string ADGATE_WALL_ID = "YOUR_ADGATE_WALL_ID";

        /// <summary>
        /// AdGate User ID variable name
        /// How you identify users in callbacks
        /// Common: "subid1", "user_id", "s1"
        /// </summary>
        public const string ADGATE_USER_ID_PARAM = "s1";

        #endregion

        #region OfferToro Configuration

        /// <summary>
        /// OfferToro App ID
        /// Get from: https://www.offertoro.com/
        /// </summary>
        public const string OFFERTORO_APP_ID = "YOUR_OFFERTORO_APP_ID";

        /// <summary>
        /// OfferToro Secret Key
        /// Used for server callbacks
        /// </summary>
        public const string OFFERTORO_SECRET_KEY = "YOUR_OFFERTORO_SECRET_KEY";

        #endregion

        #region Pollfish Configuration

        /// <summary>
        /// Pollfish API Key (iOS)
        /// Get from: https://www.pollfish.com/dashboard/
        /// Format: UUID string
        /// </summary>
        public const string POLLFISH_API_KEY_IOS = "YOUR_POLLFISH_IOS_API_KEY";

        /// <summary>
        /// Pollfish API Key (Android)
        /// </summary>
        public const string POLLFISH_API_KEY_ANDROID = "YOUR_POLLFISH_ANDROID_API_KEY";

        /// <summary>
        /// Pollfish position
        /// Where survey indicator appears
        /// </summary>
        public const IVXPollfishPosition POLLFISH_POSITION = IVXPollfishPosition.BottomRight;

        /// <summary>
        /// Pollfish release mode
        /// false = debug mode with test surveys
        /// true = production mode
        /// </summary>
        #if UNITY_EDITOR || DEVELOPMENT_BUILD
            public const bool POLLFISH_RELEASE_MODE = false;
        #else
            public const bool POLLFISH_RELEASE_MODE = true;
        #endif

        public static string GetPollfishApiKey()
        {
            #if UNITY_IOS
                return POLLFISH_API_KEY_IOS;
            #elif UNITY_ANDROID
                return POLLFISH_API_KEY_ANDROID;
            #else
                return POLLFISH_API_KEY_ANDROID;
            #endif
        }

        #endregion

        #region TheoremReach Configuration

        /// <summary>
        /// TheoremReach API Key
        /// Get from: https://theoremreach.com/
        /// Survey monetization specialist
        /// </summary>
        public const string THEOREMREACH_API_KEY = "YOUR_THEOREMREACH_API_KEY";

        /// <summary>
        /// TheoremReach User ID prefix
        /// Prepended to your user IDs
        /// </summary>
        public const string THEOREMREACH_USER_PREFIX = "user_";

        #endregion

        #region Xsolla Integration (Virtual Currency & Payments)

        /// <summary>
        /// Enable Xsolla integration
        /// Xsolla provides: virtual currency management, payment processing, offerwall aggregation
        /// Get from: https://publisher.xsolla.com/
        /// </summary>
        public const bool ENABLE_XSOLLA = false;

        /// <summary>
        /// Xsolla Project ID
        /// Found in: Publisher Account > Project Settings
        /// </summary>
        public const string XSOLLA_PROJECT_ID = "YOUR_XSOLLA_PROJECT_ID";

        /// <summary>
        /// Xsolla Merchant ID
        /// </summary>
        public const int XSOLLA_MERCHANT_ID = 0; // Replace with your merchant ID

        /// <summary>
        /// Xsolla API Key
        /// Used for server-side requests
        /// </summary>
        public const string XSOLLA_API_KEY = "YOUR_XSOLLA_API_KEY";

        /// <summary>
        /// Xsolla virtual currency SKU
        /// Your in-game currency (e.g., "coins", "gems")
        /// </summary>
        public const string XSOLLA_CURRENCY_SKU = "coins";

        /// <summary>
        /// Enable Xsolla Pay Station (payment UI)
        /// Shows multiple payment methods (cards, PayPal, etc.)
        /// </summary>
        public const bool XSOLLA_ENABLE_PAY_STATION = true;

        #endregion

        #region Pollwall Integration (Survey Aggregator)

        /// <summary>
        /// Enable Pollwall
        /// Pollwall aggregates surveys from multiple providers
        /// Alternative to managing multiple survey networks
        /// </summary>
        public const bool ENABLE_POLLWALL = false;

        /// <summary>
        /// Pollwall Publisher ID
        /// Get from: Pollwall dashboard (if using)
        /// </summary>
        public const string POLLWALL_PUBLISHER_ID = "YOUR_POLLWALL_PUBLISHER_ID";

        /// <summary>
        /// Pollwall App ID
        /// </summary>
        public const string POLLWALL_APP_ID = "YOUR_POLLWALL_APP_ID";

        #endregion

        #region General Offerwall Settings

        /// <summary>
        /// Virtual currency name
        /// Displayed to users across all offerwalls
        /// </summary>
        public const string VIRTUAL_CURRENCY_NAME = "Coins";

        /// <summary>
        /// Virtual currency icon (sprite path)
        /// Resources path to currency icon
        /// </summary>
        public const string VIRTUAL_CURRENCY_ICON_PATH = "UI/Icons/coin";

        /// <summary>
        /// Enable offerwall notifications
        /// Notify user when offerwall credits are ready
        /// </summary>
        public const bool ENABLE_OFFERWALL_NOTIFICATIONS = true;

        /// <summary>
        /// Minimum payout per offer (in currency)
        /// Filter out low-value offers
        /// </summary>
        public const int MIN_OFFER_PAYOUT = 10;

        /// <summary>
        /// Enable server-side reward validation
        /// Recommended to prevent fraud
        /// Requires backend setup
        /// </summary>
        public const bool ENABLE_SERVER_VALIDATION = true;

        /// <summary>
        /// Offerwall callback URL (for server validation)
        /// Your server endpoint that receives offerwall completions
        /// Format: https://api.yourgame.com/offerwall/callback
        /// </summary>
        public const string OFFERWALL_CALLBACK_URL = "YOUR_CALLBACK_URL";

        /// <summary>
        /// Enable offerwall analytics
        /// Track completions, revenue, user engagement
        /// </summary>
        public const bool ENABLE_OFFERWALL_ANALYTICS = true;

        #endregion

        #region Offerwall UI Settings

        /// <summary>
        /// Offerwall button position
        /// Where to show offerwall access button
        /// </summary>
        public const IVXOfferwallButtonPosition OFFERWALL_BUTTON_POSITION = IVXOfferwallButtonPosition.Shop;

        /// <summary>
        /// Show offerwall badge
        /// Display notification badge when new offers available
        /// </summary>
        public const bool SHOW_OFFERWALL_BADGE = true;

        /// <summary>
        /// Offerwall color theme
        /// Primary color for offerwall UI (hex)
        /// </summary>
        public const string OFFERWALL_PRIMARY_COLOR = "#FFD700"; // Gold

        #endregion
    }

    /// <summary>
    /// Pollfish survey indicator position
    /// </summary>
    public enum IVXPollfishPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        MiddleLeft,
        MiddleRight
    }

    /// <summary>
    /// Offerwall button placement in UI
    /// </summary>
    public enum IVXOfferwallButtonPosition
    {
        MainMenu,
        Shop,
        Settings,
        Pause,
        Custom
    }
}
