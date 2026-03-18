using System;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Data Transfer Objects (DTOs) for authentication API requests and responses.
    /// DTOs ONLY - No logic, no UnityWebRequest.
    /// </summary>
    
    #region Registration Models

    [Serializable]
    public class RegisterRequest
    {
        public string email;
        public string password;
        public string username;
    }

    [Serializable]
    public class RegisterResponse
    {
        public bool status;
        public string message;
    }

    #endregion

    #region OTP Verification Models

    [Serializable]
    public class OtpVerifyRequest
    {
        public string email;
        public string otp;
    }

    [Serializable]
    public class VerifyOtpResponse
    {
        public bool status;
        public string message;
        public LoginResponseDto data;
    }

    #endregion

    #region Login Models

    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
    }

    [Serializable]
    public class LoginResponseDto
    {
        public LoginUserDto user;
        public string token;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public int expiresIn;
    }

    [Serializable]
    public class LoginUserDto
    {
        public string id;
        public string idpUsername;
        public string email;
        public string userName;
        public string firstName;
        public string lastName;
        public string role;
    }

    #endregion

    #region Social Login Models

    [Serializable]
    public class GoogleLoginRequest
    {
        public string idToken;
    }

    [Serializable]
    public class AppleLoginRequest
    {
        public string identityToken;
    }

    [Serializable]
    public class GuestLoginRequest
    {
        public string deviceId;
    }

    #endregion
}
