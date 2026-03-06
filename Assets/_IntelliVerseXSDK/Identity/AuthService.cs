using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace IntelliVerseX.Identity
{
    /// <summary>
    /// Centralized authentication service for all backend API calls.
    /// Handles register, verify-otp, login, social login, and guest login.
    /// NO fake responses, NO mock tokens, NO Task.Delay.
    /// </summary>
    public class AuthService : MonoBehaviour
    {
        /// <summary>
        /// Default base URL for authentication endpoints when config/keys.json is not used.
        /// </summary>
        private const string DefaultBaseUrl = "http://localhost:3000/auth";

        /// <summary>
        /// Base URL for authentication endpoints. Loaded from config/keys.json (key: authBaseUrl) when present; otherwise uses default.
        /// See config/keys.example.json and config/README.md. Ensure your backend exposes: /auth/register, /auth/verify-otp, /auth/login, /auth/google, /auth/apple, /auth/guest.
        /// </summary>
        private static string BASE_URL => GetAuthBaseUrlFromConfig();

        private static string GetAuthBaseUrlFromConfig()
        {
#if UNITY_EDITOR
            try
            {
                string projectRoot = Path.GetDirectoryName(Application.dataPath);
                if (!string.IsNullOrEmpty(projectRoot))
                {
                    string path = Path.Combine(projectRoot, "config", "keys.json");
                    if (File.Exists(path))
                    {
                        string json = File.ReadAllText(path);
                        var config = JsonUtility.FromJson<AuthSecretsConfig>(json);
                        if (config != null && !string.IsNullOrEmpty(config.authBaseUrl))
                            return config.authBaseUrl.Trim();
                    }
                }
            }
            catch (Exception) { /* fallback to default */ }
#endif
            return DefaultBaseUrl;
        }

        [Serializable]
        private class AuthSecretsConfig
        {
            public string authBaseUrl;
        }

        /// <summary>
        /// Calls POST /auth/register. On success invokes onSuccess; on error invokes onError with message.
        /// </summary>
        public void Register(
            string email,
            string password,
            string username,
            Action onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(RegisterCoroutine(email, password, username, onSuccess, onError));
        }

        /// <summary>
        /// Calls POST /auth/verify-otp. On success invokes onSuccess with login response; on error invokes onError.
        /// </summary>
        public void VerifyOtp(
            string email,
            string otp,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(VerifyOtpCoroutine(email, otp, onSuccess, onError));
        }

        /// <summary>
        /// Calls POST /auth/login. On success invokes onSuccess with login response; on error invokes onError.
        /// </summary>
        public void Login(
            string email,
            string password,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(LoginCoroutine(email, password, onSuccess, onError));
        }

        /// <summary>
        /// Calls POST /auth/google. On success invokes onSuccess with login response; on error invokes onError.
        /// </summary>
        public void GoogleLogin(
            string idToken,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(GoogleLoginCoroutine(idToken, onSuccess, onError));
        }

        /// <summary>
        /// Calls POST /auth/apple. On success invokes onSuccess with login response; on error invokes onError.
        /// </summary>
        public void AppleLogin(
            string identityToken,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(AppleLoginCoroutine(identityToken, onSuccess, onError));
        }

        /// <summary>
        /// Calls POST /auth/guest. On success invokes onSuccess with login response; on error invokes onError.
        /// </summary>
        public void GuestLogin(
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            if (onSuccess == null) throw new ArgumentNullException(nameof(onSuccess));
            if (onError == null) throw new ArgumentNullException(nameof(onError));
            StartCoroutine(GuestLoginCoroutine(onSuccess, onError));
        }

        private IEnumerator RegisterCoroutine(
            string email,
            string password,
            string username,
            Action onSuccess,
            Action<string> onError)
        {
            var body = new RegisterRequest
            {
                email = email ?? string.Empty,
                password = password ?? string.Empty,
                username = username ?? string.Empty
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/register";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    onSuccess();
                    yield break;
                }

                // Extract error message from response
                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "Registration endpoint not found. Please check backend configuration.";
                    else
                        errorMessage = req.error ?? $"Registration failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        private IEnumerator VerifyOtpCoroutine(
            string email,
            string otp,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            var body = new OtpVerifyRequest
            {
                email = email ?? string.Empty,
                otp = otp ?? string.Empty
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/verify-otp";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    string text = req.downloadHandler?.text ?? string.Empty;
                    APIManager.LoginResponse loginResp = null;
                    try
                    {
                        // Parse backend response and convert to APIManager format
                        var backendResp = JsonUtility.FromJson<VerifyOtpResponse>(text);
                        if (backendResp?.data != null)
                        {
                            loginResp = ConvertToAPIManagerLoginResponse(backendResp.data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AuthService] VerifyOtp parse error: {ex.Message}");
                    }

                    if (loginResp?.data != null)
                    {
                        onSuccess(loginResp);
                        yield break;
                    }
                }

                // Extract error message from response
                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "OTP verification endpoint not found. Please check backend configuration.";
                    else if (req.responseCode == 400)
                        errorMessage = "Invalid OTP code. Please try again.";
                    else
                        errorMessage = req.error ?? $"OTP verification failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        private IEnumerator LoginCoroutine(
            string email,
            string password,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            var body = new LoginRequest
            {
                email = email ?? string.Empty,
                password = password ?? string.Empty
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/login";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    string text = req.downloadHandler?.text ?? string.Empty;
                    APIManager.LoginResponse loginResp = null;
                    try
                    {
                        // Parse backend response to APIManager.LoginResponse format
                        var backendResp = JsonUtility.FromJson<VerifyOtpResponse>(text);
                        if (backendResp?.data != null)
                        {
                            loginResp = ConvertToAPIManagerLoginResponse(backendResp.data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AuthService] Login parse error: {ex.Message}");
                    }

                    if (loginResp?.data != null)
                    {
                        onSuccess(loginResp);
                        yield break;
                    }
                }

                // Extract error message from response
                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "Login endpoint not found. Please check backend configuration.";
                    else if (req.responseCode == 401)
                        errorMessage = "Invalid email or password.";
                    else if (req.responseCode == 500)
                        errorMessage = "Server error. Please try again later.";
                    else
                        errorMessage = req.error ?? $"Login failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        private IEnumerator GoogleLoginCoroutine(
            string idToken,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            var body = new GoogleLoginRequest
            {
                idToken = idToken ?? string.Empty
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/google";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    string text = req.downloadHandler?.text ?? string.Empty;
                    APIManager.LoginResponse loginResp = null;
                    try
                    {
                        var backendResp = JsonUtility.FromJson<VerifyOtpResponse>(text);
                        if (backendResp?.data != null)
                        {
                            loginResp = ConvertToAPIManagerLoginResponse(backendResp.data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AuthService] GoogleLogin parse error: {ex.Message}");
                    }

                    if (loginResp?.data != null)
                    {
                        onSuccess(loginResp);
                        yield break;
                    }
                }

                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "Google login endpoint not found. Please check backend configuration.";
                    else
                        errorMessage = req.error ?? $"Google login failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        private IEnumerator AppleLoginCoroutine(
            string identityToken,
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            var body = new AppleLoginRequest
            {
                identityToken = identityToken ?? string.Empty
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/apple";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    string text = req.downloadHandler?.text ?? string.Empty;
                    APIManager.LoginResponse loginResp = null;
                    try
                    {
                        var backendResp = JsonUtility.FromJson<VerifyOtpResponse>(text);
                        if (backendResp?.data != null)
                        {
                            loginResp = ConvertToAPIManagerLoginResponse(backendResp.data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AuthService] AppleLogin parse error: {ex.Message}");
                    }

                    if (loginResp?.data != null)
                    {
                        onSuccess(loginResp);
                        yield break;
                    }
                }

                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "Apple login endpoint not found. Please check backend configuration.";
                    else
                        errorMessage = req.error ?? $"Apple login failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        private IEnumerator GuestLoginCoroutine(
            Action<APIManager.LoginResponse> onSuccess,
            Action<string> onError)
        {
            var body = new GuestLoginRequest
            {
                deviceId = SystemInfo.deviceUniqueIdentifier
            };

            string json = JsonUtility.ToJson(body);
            string url = BASE_URL + "/guest";
            Debug.Log($"AUTH API CALL → POST {url}");

            using (var req = new UnityWebRequest(url, "POST"))
            {
                req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json");
                req.timeout = 25;

                yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = req.result == UnityWebRequest.Result.Success;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError;
#endif

                if (isSuccess && req.responseCode >= 200 && req.responseCode < 300)
                {
                    string text = req.downloadHandler?.text ?? string.Empty;
                    APIManager.LoginResponse loginResp = null;
                    try
                    {
                        var backendResp = JsonUtility.FromJson<VerifyOtpResponse>(text);
                        if (backendResp?.data != null)
                        {
                            loginResp = ConvertToAPIManagerLoginResponse(backendResp.data);
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[AuthService] GuestLogin parse error: {ex.Message}");
                    }

                    if (loginResp?.data != null)
                    {
                        onSuccess(loginResp);
                        yield break;
                    }
                }

                string errorMessage = ExtractErrorMessage(req);
                if (string.IsNullOrEmpty(errorMessage))
                {
                    if (req.responseCode == 404)
                        errorMessage = "Guest login endpoint not found. Please check backend configuration.";
                    else
                        errorMessage = req.error ?? $"Guest login failed (HTTP {req.responseCode})";
                }
                onError(errorMessage);
            }
        }

        /// <summary>
        /// Extracts error message from UnityWebRequest response.
        /// Tries to parse JSON error response, falls back to raw text or HTTP status.
        /// </summary>
        private string ExtractErrorMessage(UnityWebRequest req)
        {
            string responseText = req.downloadHandler?.text?.Trim();
            
            if (string.IsNullOrEmpty(responseText))
                return null;

            // Try to parse as JSON error response
            try
            {
                var errorResponse = JsonUtility.FromJson<ErrorResponse>(responseText);
                if (!string.IsNullOrEmpty(errorResponse.message))
                    return errorResponse.message;
                if (!string.IsNullOrEmpty(errorResponse.error))
                    return errorResponse.error;
            }
            catch
            {
                // Not JSON, continue to return raw text
            }

            // Return raw response text if it's not too long
            if (responseText.Length < 200)
                return responseText;

            return null;
        }

        /// <summary>
        /// Error response DTO for parsing backend error messages.
        /// </summary>
        [Serializable]
        private class ErrorResponse
        {
            public string message;
            public string error;
            public string status;
        }

        /// <summary>
        /// Converts backend LoginResponseDto to APIManager.LoginResponse format.
        /// </summary>
        private APIManager.LoginResponse ConvertToAPIManagerLoginResponse(LoginResponseDto backendData)
        {
            if (backendData == null) return null;

            return new APIManager.LoginResponse
            {
                status = true,
                message = "OK",
                data = new APIManager.LoginData
                {
                    user = new APIManager.LoginUser
                    {
                        id = backendData.user?.id ?? string.Empty,
                        idpUsername = backendData.user?.idpUsername ?? backendData.user?.id ?? string.Empty,
                        email = backendData.user?.email ?? string.Empty,
                        userName = backendData.user?.userName ?? string.Empty,
                        firstName = backendData.user?.firstName ?? string.Empty,
                        lastName = backendData.user?.lastName ?? string.Empty,
                        role = backendData.user?.role ?? "user"
                    },
                    accessToken = backendData.accessToken ?? backendData.token ?? string.Empty,
                    token = backendData.token ?? backendData.accessToken ?? string.Empty,
                    idToken = backendData.idToken ?? string.Empty,
                    refreshToken = backendData.refreshToken ?? string.Empty,
                    expiresIn = backendData.expiresIn
                }
            };
        }
    }
}
