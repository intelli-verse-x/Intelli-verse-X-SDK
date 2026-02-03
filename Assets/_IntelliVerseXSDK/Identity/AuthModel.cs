using System;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Generic status + message response. For auth DTOs see AuthModels.cs.
    /// </summary>
    [Serializable]
    public class BasicResponse
    {
        public bool status;
        public string message;
    }

    [Serializable]
    public class LoginTokens
    {
        public string accessToken;
        public string refreshToken;
        public int expiresIn;
    }
}