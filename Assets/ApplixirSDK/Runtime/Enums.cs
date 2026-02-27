namespace ApplixirSDK.Runtime
{
    public enum PlayVideoResult
    {
        None,
        /// <summary>
        /// Fires when the ads manager is done playing all the valid ads
        /// in the ads response, or when the response doesn't return any valid ads
        /// </summary>
        allAdsCompleted,
        /// <summary>
        /// Fires when the ad is clicked
        /// </summary>
        click,
        /// <summary>
        /// Fires when the ad completes playing
        /// </summary>
        complete,
        /// <summary>
        /// Fires when the ad playhead crosses first quartile
        /// </summary>
        firstQuartile,
        /// <summary>
        /// Fires when the ad playhead crosses midpoint
        /// </summary>
        midpoint,
        /// <summary>
        /// Fires when the ad playhead crosses third quartile
        /// </summary>
        thirdQuartile,
        /// <summary>
        /// Fires when ad data is available
        /// </summary>
        loaded, 
        /// <summary>
        /// Fires when the ad is paused
        /// </summary>
        paused,
        /// <summary>
        /// Fires when the ad starts playing
        /// </summary>
        start, 
        /// <summary>
        /// Fires when the ad is skipped by the user
        /// </summary>
        skipped,
        /// <summary>
        /// Fires when the ad is manually ended by the user
        /// </summary>
        manuallyEnded,
        /// <summary>
        /// Fires when the thank you modal is closed by the user
        /// </summary>
        thankYouModalClosed,
        /// <summary>
        /// used if there is invalid ads response
        /// </summary>
        unknown,
        /// <summary>
        /// Fired if you call PlayVideo() before the SDK is initialized
        /// </summary>
        unavailable,
        /// <summary>
        /// Fired if initialisation of the sdk failed for any reason. For example
        /// if SDK loading was blocked by the browser or ads blocker. 
        /// </summary>
        initialisationFailed,
        /// <summary>
        /// fired when user receive a reward for video.
        /// </summary>
        adsRewarded
    }

    public enum LogLevel
    {
        None = 0,
        Error = 1,
        Warn = 2,
        Info = 3,
        Debug = 4,
        Trace = 5
    }
}