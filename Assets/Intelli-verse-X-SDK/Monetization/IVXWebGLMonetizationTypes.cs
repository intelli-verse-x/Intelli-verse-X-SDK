// IVXWebGLMonetizationTypes.cs
// WebGL Monetization Types and Enums
// Version: 2.0.0

using System;

namespace IntelliVerseX.Monetization
{
    /// <summary>
    /// WebGL Ad Network types
    /// </summary>
    public enum WebGLAdNetwork
    {
        None = 0,
        AdSense = 1,
        Applixir = 2
    }

    /// <summary>
    /// WebGL Ad Event Types for analytics
    /// </summary>
    public enum WebGLAdEventType
    {
        Impression = 0,
        Click = 1,
        Close = 2,
        Complete = 3,
        Error = 4,
        Load = 5,
        Skip = 6
    }

    /// <summary>
    /// Applixir ad completion status
    /// </summary>
    public enum ApplixirCompletionStatus
    {
        Unknown = 0,
        Completed = 1,
        Skipped = 2,
        Closed = 3,
        Error = 4
    }

    /// <summary>
    /// Game orientation for WebGL builds
    /// </summary>
    public enum GameOrientation
    {
        Portrait = 0,
        Landscape = 1,
        Auto = 2
    }
}
