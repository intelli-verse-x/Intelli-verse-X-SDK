namespace IntelliVerseX.Bootstrap
{
    /// <summary>
    /// Outcome of <see cref="IVXBootstrap.InitializeAsync"/>.
    /// Prefer this over the legacy bool on <see cref="IVXBootstrap.OnBootstrapComplete"/>.
    /// </summary>
    public enum IVXBootstrapStatus
    {
        /// <summary>Initialization has not finished yet.</summary>
        NotStarted = 0,

        /// <summary>All enabled modules initialized and backend auth succeeded.</summary>
        Online = 1,

        /// <summary>
        /// Bootstrap finished; backend auth failed or was skipped — local/offline play only.
        /// Hiro/Satori that require a session were not started.
        /// </summary>
        Offline = 2,

        /// <summary>
        /// Bootstrap finished online, but one or more optional modules failed.
        /// Core identity may still be usable.
        /// </summary>
        Partial = 3,

        /// <summary>Hard failure (missing config, fatal exception). SDK is not ready.</summary>
        Failed = 4
    }
}
