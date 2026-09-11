using IntelliVerseX.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static partial class APIManager
{
    #region Auth V2: Signup / Initiate (Cognito)

    // === Endpoint ===
    public static string SignupInitiateUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/signup/initiate";

    // === DTOs ===
    [Serializable]
    private class SignupInitiateRequest
    {
        public string email;
        public string password;
        public string role;
    }

    [Serializable]
    public class SignupInitiateResponse
    {
        public bool status;
        public string message;
    }

    // === Public API ===
    /// <summary>
    /// Creates a Cognito user (or resends OTP if already initiated).
    /// Validates email & password, masks sensitive values in logs,
    /// retries on 408/429/5xx with backoff, and returns a typed response.
    /// </summary>
    public static async Task<SignupInitiateResponse> SignupInitiateAsync(
        string email,
        string password,
        string role = "user",
        CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password is required.", nameof(password));
        ValidatePassword(password); // throws with helpful message if weak

        role = string.IsNullOrWhiteSpace(role) ? "user" : role.Trim();

        var reqObj = new SignupInitiateRequest
        {
            email = email.Trim(),
            password = password,
            role = role
        };

        // Real body for the request
        string bodyJson = JsonUtility.ToJson(reqObj);

        // Masked body ONLY for logs / cURL preview
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email" });

        // cURL preview (masked)
        PrintCurl("POST", SignupInitiateUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                // Exponential backoff with jitter
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SignupInitiateUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SignupInitiateUrl} (signup/initiate)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : text)}");

                    // Success (treat any 2xx as OK)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Parse typed response (fallback to raw text if shape changes)
                        SignupInitiateResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupInitiateResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }
                        return resp ?? new SignupInitiateResponse
                        {
                            status = true,
                            message = string.IsNullOrWhiteSpace(text) ? "OK" : text
                        };
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    // If 429, respect Retry-After when present
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    // Non-retryable or exhausted retries → raise with server message if any
                    string msg = string.IsNullOrWhiteSpace(text) ? req.error : text;
                    throw new Exception($"Signup initiate failed: HTTP {code} - {msg}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Signup/initiate cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (signup): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (signup) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown signup/initiate error");
    }

    // === Helpers (local to this region) ===
    private static bool IsValidEmail(string email)
    {
        // Lightweight RFC-ish check; avoids allocations & false-positives
        try
        {
            // Unity players may not have System.Net.Mail available everywhere; keep simple.
            // Must have one @, at least one dot after @, no spaces.
            email = email.Trim();
            int at = email.IndexOf('@');
            if (at <= 0 || at != email.LastIndexOf('@')) return false;
            int dot = email.IndexOf('.', at + 1);
            if (dot <= at + 1 || dot == email.Length - 1) return false;
            if (email.Contains(" ")) return false;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
            return false;
        }
    }

    private static void ValidatePassword(string password)
    {
        // Sensible defaults; adjust if backend has stricter policy.
        // - >= 8 chars
        // - at least 1 upper, 1 lower, 1 digit, 1 special
        if (password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(password));

        bool hasUpper = false, hasLower = false, hasDigit = false, hasSpecial = false;
        foreach (char c in password)
        {
            if (char.IsUpper(c)) hasUpper = true;
            else if (char.IsLower(c)) hasLower = true;
            else if (char.IsDigit(c)) hasDigit = true;
            else hasSpecial = true;
        }

        if (!(hasUpper && hasLower && hasDigit && hasSpecial))
            throw new ArgumentException("Password must include upper, lower, digit, and special character.", nameof(password));
    }

    private static string MaskSensitiveFields(string json, string[] fieldNames)
    {
        if (string.IsNullOrEmpty(json) || fieldNames == null || fieldNames.Length == 0) return json;

        // naive mask: "field": "anything" -> "field": "****"
        foreach (var f in fieldNames)
        {
            // handles whitespace variants:  "password"  :   "value"
            string pattern = $"(\"{Regex.Escape(f)}\"\\s*:\\s*\")([^\"]*)(\")";
            json = Regex.Replace(json, pattern, $"$1****$3");
        }
        return json;
    }

    #endregion

    #region Auth V2: Confirm Signup (Cognito + Local)

    // === Endpoint ===
    public static string SignupConfirmUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/signup";

    // === DTOs ===
    [Serializable]
    public class SignupConfirmRequest
    {
        public string email;
        public string firstName;
        public string lastName;
        public string otp;
        public string password;
        public string userName;
        public string role;          // e.g., "user"
        public string fcmToken;      // optional
        public string fromDevice;    // e.g., "web", "android", "ios", "unity" (optional)
        public string macAddress;    // optional
        public string referralCode;  // optional
    }

    [Serializable]
    public class SignupConfirmUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public string idpUsername;
        public bool isAdult;
        public string walletAddress;
    }

    [Serializable]
    public class SignupConfirmData
    {
        public SignupConfirmUser user;
        public string token;         // access token (JWT)
        public string idToken;       // id token (JWT)
        public string refreshToken;  // opaque/JWE
        public int expiresIn;     // seconds
    }

    [Serializable]
    public class SignupConfirmResponse
    {
        public bool status;
        public string message;
        public SignupConfirmData data;
    }

    /// <summary>
    /// Confirms email OTP in Cognito and creates the local user.
    /// On success, optionally configures runtime user-auth with returned tokens.
    /// </summary>
    /// <param name="request">Signup payload (email, otp, password, etc.)</param>
    /// <param name="configureUserAuthOnSuccess">
    /// If true, automatically enables runtime user-auth using (idpUsername, refreshToken, token, expiresIn).
    /// </param>
    public static async Task<SignupConfirmResponse> SignupConfirmAsync(
        SignupConfirmRequest request,
        bool configureUserAuthOnSuccess = true,
        CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (request == null) throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.email))
            throw new ArgumentException("Email is required.", nameof(request.email));
        if (!IsValidEmail(request.email))
            throw new ArgumentException("Email format is invalid.", nameof(request.email));

        if (string.IsNullOrWhiteSpace(request.otp))
            throw new ArgumentException("OTP is required.", nameof(request.otp));
        // Accept 4–8 digits (relax if backend supports alphanumerics)
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.otp.Trim(), @"^\d{4,8}$"))
            throw new ArgumentException("OTP must be 4–8 digits.", nameof(request.otp));

        if (string.IsNullOrWhiteSpace(request.password))
            throw new ArgumentException("Password is required.", nameof(request.password));
        ValidatePassword(request.password); // throws if weak

        if (string.IsNullOrWhiteSpace(request.userName))
            throw new ArgumentException("userName is required.", nameof(request.userName));
        // Username: 3–32, letters/digits/_/.- (feel free to relax/tighten as needed)
        if (!System.Text.RegularExpressions.Regex.IsMatch(request.userName.Trim(), @"^[A-Za-z0-9_.-]{3,32}$"))
            throw new ArgumentException("userName must be 3–32 chars (letters, digits, . _ -).", nameof(request.userName));

        request.role = string.IsNullOrWhiteSpace(request.role) ? "user" : request.role.Trim();
        request.firstName = request.firstName?.Trim() ?? "";
        request.lastName = request.lastName?.Trim() ?? "";

        // Resolve device fingerprint fields for machine authorization checks.
        DeviceInfoHelper.GetLoginDeviceFields(out string fromDeviceAuto, out string macAuto);
        request.fromDevice = string.IsNullOrWhiteSpace(request.fromDevice)
            ? (string.IsNullOrWhiteSpace(fromDeviceAuto) ? "web" : fromDeviceAuto)
            : request.fromDevice.Trim();

        // Backend registration policy expects web for this endpoint.
        if (string.Equals(request.fromDevice, "machine", StringComparison.OrdinalIgnoreCase))
        {
            request.fromDevice = "web";
        }

        if (string.IsNullOrWhiteSpace(request.macAddress) || IsKnownPlaceholderMac(request.macAddress))
        {
            request.macAddress = macAuto;
        }

        if (string.IsNullOrWhiteSpace(request.fcmToken))
        {
            request.fcmToken = request.macAddress;
        }

        if (!string.IsNullOrWhiteSpace(request.macAddress))
        {
            // Soft-validate MAC if provided
            if (!System.Text.RegularExpressions.Regex.IsMatch(request.macAddress.Trim(), @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                throw new ArgumentException("macAddress must be in format 00:11:22:33:44:55.", nameof(request.macAddress));
        }

        // Prepare JSON body (real)
        string bodyJson = JsonUtility.ToJson(request);

        // Mask sensitive fields for logs / cURL
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email", "otp", "fcmToken", "refreshToken" });

        // cURL preview (masked)
        PrintCurl("POST", SignupConfirmUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup confirm) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SignupConfirmUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SignupConfirmUrl} (confirm signup)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text))}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        SignupConfirmResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupConfirmResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                        // Fallback if shape changes unexpectedly
                        if (resp == null) resp = new SignupConfirmResponse { status = true, message = "OK", data = null };

                        // Optionally configure runtime user-auth for subsequent requests
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = resp.data.user != null ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            string access = resp.data.token;
                            long expiresIn = Math.Max(0, resp.data.expiresIn);

                            long nowEpoch = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long accessExpiryEpoch = nowEpoch + expiresIn;

                            // Enable runtime user-auth: auto-refresh on 401s for user endpoints
                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: accessExpiryEpoch
                            );
                        }

                        return resp;
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    string msg = string.IsNullOrWhiteSpace(text) ? req.error : text;
                    throw new Exception($"Confirm signup failed: HTTP {code} - {msg}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Confirm signup cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (confirm signup): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (confirm signup) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown confirm-signup error");
    }

    private static bool IsKnownPlaceholderMac(string mac)
    {
        if (string.IsNullOrWhiteSpace(mac)) return false;
        string trimmed = mac.Trim();
        return string.Equals(trimmed, "00:1A:2B:3C:4D:5E", StringComparison.OrdinalIgnoreCase)
               || string.Equals(trimmed, "00-1A-2B-3C-4D-5E", StringComparison.OrdinalIgnoreCase);
    }

    #endregion

    #region Auth V2: Forgot Password

    // === Endpoints ===
    public static string ForgotPasswordUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/forgot-password";
    public static string ResetPasswordUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/reset-password";

    // === DTOs ===
    [Serializable]
    public class ForgotPasswordRequest
    {
        public string email;
    }

    [Serializable]
    public class ForgotPasswordResponse
    {
        public bool status;
        public string message;
    }

    [Serializable]
    public class ResetPasswordRequest
    {
        public string email;
        public string otp;
        public string newPassword;
    }

    [Serializable]
    public class ResetPasswordResponse
    {
        public bool status;
        public string message;
    }

    /// <summary>
    /// Request a password reset OTP to be sent to the user's email.
    /// </summary>
    public static async Task<ForgotPasswordResponse> ForgotPasswordAsync(
        string email,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));

        var request = new ForgotPasswordRequest { email = email.Trim().ToLowerInvariant() };
        string bodyJson = JsonUtility.ToJson(request);
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "email" });

        PrintCurl("POST", ForgotPasswordUrl,
            new Dictionary<string, string> {
                { "accept", "*/*" },
                { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (forgot-password) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ForgotPasswordUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                    req.uploadHandler = new UploadHandlerRaw(data);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ForgotPasswordUrl} (forgot-password)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"HTTP request timed out ({RequestTimeoutSeconds}s).");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {text}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        var resp = JsonUtility.FromJson<ForgotPasswordResponse>(text);
                        if (resp == null)
                            resp = new ForgotPasswordResponse { status = true, message = "OTP sent successfully." };
                        return resp;
                    }

                    // Parse error response
                    ForgotPasswordResponse errResp = null;
                    try { errResp = JsonUtility.FromJson<ForgotPasswordResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                    if (errResp != null && !errResp.status)
                        return errResp;

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    return new ForgotPasswordResponse { status = false, message = $"Request failed: HTTP {code}" };
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown forgot-password error");
    }

    /// <summary>
    /// Reset password using the OTP received via email.
    /// </summary>
    public static async Task<ResetPasswordResponse> ResetPasswordAsync(
        string email,
        string otp,
        string newPassword,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email is required.", nameof(email));
        if (!IsValidEmail(email))
            throw new ArgumentException("Email format is invalid.", nameof(email));
        if (string.IsNullOrWhiteSpace(otp))
            throw new ArgumentException("OTP is required.", nameof(otp));
        if (string.IsNullOrWhiteSpace(newPassword))
            throw new ArgumentException("New password is required.", nameof(newPassword));
        ValidatePassword(newPassword);

        var request = new ResetPasswordRequest
        {
            email = email.Trim().ToLowerInvariant(),
            otp = otp.Trim(),
            newPassword = newPassword
        };
        string bodyJson = JsonUtility.ToJson(request);
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "email", "otp", "newPassword" });

        PrintCurl("POST", ResetPasswordUrl,
            new Dictionary<string, string> {
                { "accept", "*/*" },
                { "Content-Type", "application/json" }
            },
            maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (reset-password) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ResetPasswordUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(bodyJson);
                    req.uploadHandler = new UploadHandlerRaw(data);
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ResetPasswordUrl} (reset-password)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"HTTP request timed out ({RequestTimeoutSeconds}s).");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";

                    Log($"[HTTP] ← {code} {text}");

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        var resp = JsonUtility.FromJson<ResetPasswordResponse>(text);
                        if (resp == null)
                            resp = new ResetPasswordResponse { status = true, message = "Password reset successfully." };
                        return resp;
                    }

                    // Parse error response
                    ResetPasswordResponse errResp = null;
                    try { errResp = JsonUtility.FromJson<ResetPasswordResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }

                    if (errResp != null && !errResp.status)
                        return errResp;

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    return new ResetPasswordResponse { status = false, message = $"Request failed: HTTP {code}" };
                }
                catch (OperationCanceledException) { throw; }
                catch (TimeoutException tex) { lastErr = tex; if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown reset-password error");
    }

    #endregion

    #region Auth V2: Login (Cognito)

    // === Base URL ===
    public const string API_BASE_URL = "https://api.intelli-verse-x.ai";

    // === Endpoint ===
    public static string LoginUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/login";

    // === DTOs ===
    [Serializable]
    public class LoginRequest
    {
        public string email;
        public string password;
        public string fromDevice;   // e.g. "unity" | "machine" | "android" | "ios" | "web"
        public string macAddress;   // optional
        /// <summary>IntelliVerseX game UUID for multi-tenant auth routing. Defaults from IVXURLs.GameId when omitted.</summary>
        public string gameId;
    }

    /// <summary>Non-retryable HTTP failure from <see cref="LoginAsync"/>; must not be swallowed by generic retry logic.</summary>
    private sealed class LoginHttpFailureException : Exception
    {
        public LoginHttpFailureException(string message) : base(message) { }
    }

    [Serializable]
    public class LoginUser
    {
        public string id;
        public string firstName;
        public string lastName;
        public string userName;
        public string email;
        public string role;
        public string idpUsername;
        public string walletAddress;
        public bool isAdult;
        public string loginType;
        public string fcmToken;
        public string kycStatus;
        public string accountStatus;
        public string createdAt;
        public string updatedAt;
        // plus any extra fields from payload; Unity’s JsonUtility ignores unknowns safely
    }

    [Serializable]
    public class LoginData
    {
        public LoginUser user;
        public string token;         // sometimes present (alias of access token)
        public string accessToken;   // preferred
        public string idToken;
        public string refreshToken;
        public int expiresIn;     // seconds
    }

    [Serializable]
    public class LoginResponse
    {
        public bool status;
        public string message;
        public LoginData data;
    }

    /// <summary>
    /// Authenticates with Cognito and returns tokens.
    /// On success:
    ///  - Configures runtime user-auth (auto refresh via your Refresh endpoint)
    ///  - Persists the full response into UserSessionManager for later use
    /// </summary>
    public static async Task<LoginResponse> LoginAsync(
        LoginRequest req,
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true,
        CancellationToken ct = default)
    {
        if (req == null) throw new ArgumentNullException(nameof(req));

        if (string.IsNullOrWhiteSpace(req.email))
            throw new ArgumentException("Email is required.", nameof(req.email));
        if (!IsValidEmail(req.email))
            throw new ArgumentException("Email format is invalid.", nameof(req.email));

        if (string.IsNullOrWhiteSpace(req.password))
            throw new ArgumentException("Password is required.", nameof(req.password));

        req.fromDevice = string.IsNullOrWhiteSpace(req.fromDevice) ? "unity" : req.fromDevice.Trim();

        if (!string.IsNullOrWhiteSpace(req.macAddress))
        {
            if (!System.Text.RegularExpressions.Regex.IsMatch(req.macAddress.Trim(), @"^([0-9A-Fa-f]{2}:){5}[0-9A-Fa-f]{2}$"))
                throw new ArgumentException("macAddress must be in format 00:11:22:33:44:55.", nameof(req.macAddress));
        }

        if (string.IsNullOrWhiteSpace(req.gameId))
            req.gameId = IVXURLs.GameId;

        string bodyJson = JsonUtility.ToJson(req);
        string maskedJson = MaskSensitiveFields(bodyJson, new[] { "password", "email" });

        // Verbose API logging
        var requestHeaders = new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
        };
        LogAPIRequest("POST", LoginUrl, maskedJson, requestHeaders);

        // Masked cURL preview
        PrintCurl("POST", LoginUrl, requestHeaders, maskedJson);

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (login) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var uwr = new UnityWebRequest(LoginUrl, UnityWebRequest.kHttpVerbPOST))
            {
                float requestStartTime = Time.realtimeSinceStartup;
                try
                {
                    uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                    uwr.SetRequestHeader("accept", "*/*");
                    uwr.SetRequestHeader("Content-Type", "application/json");
                    uwr.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {LoginUrl} (login)");

                    var op = uwr.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            uwr.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = uwr.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = uwr.isNetworkError;
                bool isHttpErr = uwr.isHttpError;
#endif
                    long code = uwr.responseCode;
                    string text = uwr.downloadHandler?.text ?? "";
                    float durationMs = (Time.realtimeSinceStartup - requestStartTime) * 1000f;
                    
                    Log($"[HTTP] ← {code} {(string.IsNullOrEmpty(text) ? "" : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text))}");
                    
                    // Verbose API response logging
                    LogAPIResponse(LoginUrl, code, text, durationMs);

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        LoginResponse resp = null;
                        try { resp = JsonUtility.FromJson<LoginResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }
                        if (resp == null) resp = new LoginResponse { status = true, message = "OK", data = null };

                        // Configure runtime user-auth so other API calls can auto-use/refresh token
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = resp.data.user != null ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            // prefer accessToken if present, else fallback to token
                            string access = !string.IsNullOrWhiteSpace(resp.data.accessToken) ? resp.data.accessToken : resp.data.token;

                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long expEpoch = now + Math.Max(0, resp.data.expiresIn <= 0 ? 1800 : resp.data.expiresIn);

                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: expEpoch
                            );
                        }

                        // Always keep runtime session in memory; persist only when requested.
                        if (resp.data != null)
                        {
                            try
                            {
                                if (persistSession)
                                    UserSessionManager.ApplyLoginResponse(resp, persist: true);
                                else
                                    UserSessionManager.ApplyLoginResponse(resp, persist: false);
                            }
                            catch (Exception ex)
                            {
                                LogError($"[Session] Save/SetTemporary failed: {ex.Message}");
                            }
                        }

                        return resp;
                    }

                    // Retryable?
                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;

                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = uwr.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    if (retryable && attempt < MaxRetries)
                        continue;

                    // Log error for failed requests
                    LogAPIError(LoginUrl, "POST", $"Login failed: HTTP {code}", null);
                    throw new LoginHttpFailureException($"Login failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? uwr.error : text)}");
                }
                catch (LoginHttpFailureException) { throw; }
                catch (OperationCanceledException) { LogError("[HTTP] Login cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogAPIError(LoginUrl, "POST", $"Timeout: {tex.Message}", tex); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogAPIError(LoginUrl, "POST", $"Attempt #{attempt} failed: {ex.Message}", ex); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown login error");
    }

    #endregion

    #region Unique App ID (Auth V2 bearer)

    [Serializable]
    public class UniqueAppIdRequest
    {
        public string gameName;
    }

    [Serializable]
    public class UniqueAppIdData
    {
        public string uniqueAppId;
    }

    [Serializable]
    public class UniqueAppIdResponse
    {
        public bool status;
        public string message;
        public UniqueAppIdData data;
    }

    /// <summary>
    /// Creates a unique App/Game ID for <paramref name="gameName"/> using a user Auth V2 access token.
    /// POST <see cref="IVXURLs.UniqueAppId"/> → <c>data.uniqueAppId</c> (UUID).
    /// </summary>
    /// <param name="gameName">Display name for the new game registration.</param>
    /// <param name="accessToken">
    /// Bearer access token from <see cref="LoginAsync"/>. When null/empty, uses
    /// <see cref="UserSessionManager.AccessToken"/> if a session is present.
    /// </param>
    /// <param name="ct">Cancellation token.</param>
    public static async Task<UniqueAppIdResponse> CreateUniqueAppIdAsync(
        string gameName,
        string accessToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(gameName))
            throw new ArgumentException("gameName is required.", nameof(gameName));

        string token = !string.IsNullOrWhiteSpace(accessToken)
            ? accessToken.Trim()
            : UserSessionManager.AccessToken;

        if (string.IsNullOrWhiteSpace(token))
            throw new InvalidOperationException(
                "Access token required. Sign in with Auth V2 (LoginAsync) before creating a unique App ID.");

        var body = JsonUtility.ToJson(new UniqueAppIdRequest { gameName = gameName.Trim() });
        string raw = await PostJsonAsync(
            IVXURLs.UniqueAppId,
            body,
            "Bearer " + token,
            redactSecretsInBodyLog: false,
            ct);

        UniqueAppIdResponse resp = null;
        try
        {
            resp = JsonUtility.FromJson<UniqueAppIdResponse>(raw);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[APIManager] UniqueAppId JSON parse failed: {ex.Message}");
        }

        if (resp == null)
            throw new Exception("unique-appid response was empty or invalid JSON.");

        if (!resp.status || resp.data == null || string.IsNullOrWhiteSpace(resp.data.uniqueAppId))
        {
            string msg = string.IsNullOrWhiteSpace(resp.message) ? "uniqueAppId was not returned." : resp.message;
            throw new Exception(msg);
        }

        resp.data.uniqueAppId = resp.data.uniqueAppId.Trim();
        return resp;
    }

    [Serializable]
    public class GameIdVerifyResult
    {
        public bool localOk;
        public bool authOk;
        public bool gameAccepted;
        public long httpStatus;
        public string message;
        public bool OverallOk => localOk && authOk && gameAccepted;
    }

    /// <summary>
    /// Authenticated post-create probe: validates UUID, confirms Bearer via <see cref="IVXURLs.GetUserProfile"/>,
    /// then GETs a game-scoped leaderboard probe. 2xx ⇒ platform accepted the Game ID for routing.
    /// </summary>
    public static async Task<GameIdVerifyResult> VerifyGameIdOnlineAsync(
        string gameId,
        string accessToken = null,
        CancellationToken ct = default)
    {
        var result = new GameIdVerifyResult();
        if (string.IsNullOrWhiteSpace(gameId))
        {
            result.message = "Game ID is empty.";
            return result;
        }

        string id = gameId.Trim();
        if (!System.Text.RegularExpressions.Regex.IsMatch(
                id,
                @"^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$"))
        {
            result.message = "Game ID is not a UUID.";
            return result;
        }

        result.localOk = true;

        string token = !string.IsNullOrWhiteSpace(accessToken)
            ? accessToken.Trim()
            : UserSessionManager.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            result.message = "Access token required for online Game ID verify.";
            return result;
        }

        // 1) Auth ping — proves the Bearer is alive
        try
        {
            long meCode = await GetStatusAsync(IVXURLs.GetUserProfile, token, ct);
            result.authOk = meCode >= 200 && meCode < 300;
            if (!result.authOk)
            {
                result.httpStatus = meCode;
                result.message = meCode == 401 || meCode == 403
                    ? "Auth ping failed — sign in again."
                    : $"Auth ping failed (HTTP {meCode}).";
                return result;
            }
        }
        catch (Exception ex)
        {
            result.message = "Auth ping network error: " + ex.Message;
            return result;
        }

        // 2) Game-scoped probe — leaderboard list with gameId (no dedicated games GET exists)
        try
        {
            string probeUrl = IVXURLs.GetGameIdProbeUrl(id);
            long code = await GetStatusAsync(probeUrl, token, ct);
            result.httpStatus = code;
            if (code >= 200 && code < 300)
            {
                result.gameAccepted = true;
                result.message = "Online verify OK — auth live and Game ID accepted for game APIs.";
                return result;
            }

            if (code == 404)
            {
                result.message = "Online verify: Game ID not found on platform (HTTP 404).";
                return result;
            }

            if (code == 401 || code == 403)
            {
                result.authOk = false;
                result.message = "Online verify unauthorized for game probe — refresh token and retry.";
                return result;
            }

            // Some tenants return empty 4xx for brand-new IDs while still routing — treat 400 as soft-accept with warning
            if (code == 400)
            {
                result.gameAccepted = true;
                result.message = "Online verify soft-OK (HTTP 400 on probe). Game ID is set; platform may still be provisioning.";
                return result;
            }

            result.message = $"Online verify inconclusive (HTTP {code}). Local save still applied.";
            return result;
        }
        catch (Exception ex)
        {
            result.message = "Game probe network error: " + ex.Message;
            return result;
        }
    }

    private static async Task<long> GetStatusAsync(string url, string bearerToken, CancellationToken ct)
    {
        using (var uwr = UnityWebRequest.Get(url))
        {
            uwr.SetRequestHeader("accept", "application/json");
            uwr.SetRequestHeader("Authorization", "Bearer " + bearerToken);
            uwr.timeout = Mathf.Clamp(RequestTimeoutSeconds, 5, 30);

            var op = uwr.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > uwr.timeout)
                {
                    uwr.Abort();
                    throw new TimeoutException($"GET timed out: {url}");
                }
                await Task.Yield();
            }

            return uwr.responseCode;
        }
    }

    #endregion

    #region Auth V2: Guest Signup

    public static string GuestSignupUrl = "https://api.intelli-verse-x.ai/api/user/auth_v_2/guest-signup";

    [Serializable] public class GuestSignupRequest { public string role; }  // usually "user"

    [Serializable]
    public class GuestSignupResponse
    {
        public bool status;
        public string message;
        // Response shape is identical to login – reuse LoginData/LoginUser
        public LoginData data;
    }

    /// <summary>
    /// Creates a guest user (anonymous) and returns tokens.
    /// On success:
    ///  - Configures runtime user-auth (auto refresh via Refresh endpoint)
    ///  - Persists the full response into UserSessionManager
    /// </summary>
    public static async Task<GuestSignupResponse> GuestSignupAsync(
        string role = "user",
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true,
        CancellationToken ct = default(CancellationToken))
    {
        if (string.IsNullOrWhiteSpace(role)) role = "user";
        var reqBody = new GuestSignupRequest { role = role.Trim() };

        string bodyJson = JsonUtility.ToJson(reqBody);

        // Masked cURL preview (no secrets here, but keep consistent)
        PrintCurl(
            "POST",
            GuestSignupUrl,
            new Dictionary<string, string> {
            { "accept", "*/*" },
            { "Content-Type", "application/json" }
            },
            bodyJson
        );

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (guest-signup) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var uwr = new UnityWebRequest(GuestSignupUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    uwr.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    uwr.downloadHandler = new DownloadHandlerBuffer();
                    uwr.SetRequestHeader("accept", "*/*");
                    uwr.SetRequestHeader("Content-Type", "application/json");
                    uwr.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {GuestSignupUrl} (guest-signup)");

                    var op = uwr.SendWebRequest();
                    float t0 = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested) { uwr.Abort(); ct.ThrowIfCancellationRequested(); }
                        if (Time.realtimeSinceStartup - t0 > RequestTimeoutSeconds)
                        {
                            uwr.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }
                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = uwr.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = uwr.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = uwr.isNetworkError;
                bool isHttpErr = uwr.isHttpError;
#endif
                    long code = uwr.responseCode;
                    string text = uwr.downloadHandler?.text ?? "";

                    // Avoid complicated escaping in interpolated strings – build the preview plainly
                    string preview = string.IsNullOrEmpty(text)
                        ? ""
                        : (text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text);
                    Log("[HTTP] <- " + code + " " + preview);

                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        GuestSignupResponse resp = null;
                        try { resp = JsonUtility.FromJson<GuestSignupResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }
                        if (resp == null) resp = new GuestSignupResponse { status = true, message = "OK", data = null };

                        // Configure runtime user-auth (so other calls use/refresh token automatically)
                        if (configureUserAuthOnSuccess && resp.data != null)
                        {
                            string idpUser = (resp.data.user != null) ? resp.data.user.idpUsername : null;
                            string refresh = resp.data.refreshToken;
                            string access = !string.IsNullOrWhiteSpace(resp.data.accessToken) ? resp.data.accessToken : resp.data.token;

                            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                            long expEpoch = now + Math.Max(0, (resp.data.expiresIn <= 0 ? 1800 : resp.data.expiresIn));

                            ConfigureUserAuth(
                                useUserAuthToken: true,
                                idpUsername: idpUser,
                                refreshToken: refresh,
                                initialAccessToken: access,
                                accessTokenExpiresInEpoch: expEpoch
                            );
                        }

                        // Persist for later use (or keep runtime-only when remember-me is off)
                        if (resp.data != null)
                        {
                            try
                            {
                                if (persistSession)
                                    UserSessionManager.SaveFromGuestResponse(resp);
                                else
                                {
                                    var asLogin = new LoginResponse
                                    {
                                        status = resp.status,
                                        message = resp.message,
                                        data = resp.data
                                    };
                                    UserSessionManager.ApplyLoginResponse(asLogin, persist: false);
                                }
                            }
                            catch (Exception ex) { LogError("[Session] Save (guest) failed: " + ex.Message); }
                        }

                        return resp;
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = uwr.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log("[HTTP] 429 Rate limited – honoring Retry-After: " + ra + "s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception("Guest-signup failed: HTTP " + code + " - " +
                                        (string.IsNullOrWhiteSpace(text) ? uwr.error : text));
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Guest-signup cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError("[HTTP] Timeout (guest): " + tex.Message);
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError("[HTTP] Attempt (guest) #" + attempt + " failed: " + ex.GetType().Name + " – " + ex.Message);
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown guest-signup error");
    }

    #endregion

    #region Auth V2: Social Login

    // === Endpoint ===
    // Note: this endpoint uses "auth-v2" (hyphen), not "auth_v_2" (underscore).
    public static string SocialLoginUrl =
    "https://api.intelli-verse-x.ai/api/user/auth-v2/social/game-login";


    // === DTOs (V2 exact payload) ===
    [Serializable]
    public class PayloadSocialLoginV2
    {
        public string loginType;       // "apple", "google", etc.
        public string email;
        public string appleKey;        // Apple auth code; null/empty for google
        public string firstName;
        public string lastName;
        public string userName;
        public string profilePicture;  // optional
        public string socialId;        // optional (Apple userId / Google userId)
        public string role;            // "user"
        public string fcmToken;
        public string fromDevice;      // "ios", "android", "unity", etc.
        public string macAddress;      // extra, for your own tracking
        public string password;        // extra / unused for social

        public PayloadSocialLoginV2(
            string loginType,
            string email,
            string firstName,
            string lastName,
            string userName,
            string role,
            string fcmToken,
            string fromDevice,
            string appleKey,
            string macAddress,
            string profilePicture = null,
            string socialId = null,
            string password = null)
        {
            this.loginType = loginType;
            this.email = email;
            this.firstName = firstName;
            this.lastName = lastName;
            this.userName = userName;
            this.role = role;
            this.fcmToken = fcmToken;
            this.fromDevice = fromDevice;
            this.appleKey = appleKey;
            this.macAddress = macAddress;
            this.profilePicture = profilePicture;
            this.socialId = socialId;
            this.password = password;
        }
    }

    [Serializable]
    public class SocialLoginResponse
    {
        public bool status;
        public string message;
        public SocialLoginData data;
        public object other; // ignore if present
    }

    [Serializable]
    public class SocialLoginData
    {
        public LoginUser user;
        public string accessToken;
        public string idToken;
        public string refreshToken;
        public long expiresIn;
        public bool requiresPasswordSetup;
        public bool isNewUser;
    }

    // === Public API (Task-based) ===
    /// <summary>
    /// Social login (Apple/Google/etc) that conforms to the new API payload.
    /// - POSTs PayloadSocialLoginV2
    /// - On success, persists session (if persistSession)
    /// - Optionally configures runtime user-auth for auto refresh
    /// Returns typed <see cref="SocialLoginResponse"/> or normalized fallback.
    /// </summary>
    public static async Task<SocialLoginResponse> SocialLoginAsync(
     string loginType,
     string email,
     string firstName,
     string lastName,
     string userName,
     string password,                     // optional for social; pass null/empty if not used
     string role,
     string fcmToken,
     string appleKey,                     // required for Apple; null/empty otherwise
     bool configureUserAuthOnSuccess = true,
     bool persistSession = true,
     string fromDeviceOverride = null,    // optional override (else auto)
     string macAddressOverride = null,    // optional override (else auto)
     CancellationToken ct = default)
    {
        // ---- Input validation ----
        if (string.IsNullOrWhiteSpace(loginType))
            throw new ArgumentException("loginType is required.", nameof(loginType));

        // Resolve device info
        DeviceInfoHelper.GetLoginDeviceFields(out string fromDeviceAuto, out string macAuto);

        // fromDevice: override > auto > "unity"
        string fromDevice = string.IsNullOrWhiteSpace(fromDeviceOverride)
            ? (string.IsNullOrWhiteSpace(fromDeviceAuto) ? "unity" : fromDeviceAuto)
            : fromDeviceOverride;

        // macAddress: override > auto
        string macAddress = string.IsNullOrWhiteSpace(macAddressOverride)
            ? macAuto
            : macAddressOverride;

        // fcm fallback to mac if empty
        if (string.IsNullOrWhiteSpace(fcmToken))
            fcmToken = macAddress;

        // Build V2 game-login payload
        var payload = new PayloadSocialLoginV2(
            loginType: loginType,
            email: email,
            firstName: firstName,
            lastName: lastName,
            userName: userName,
            role: string.IsNullOrWhiteSpace(role) ? "user" : role.Trim(),
            fcmToken: fcmToken,
            fromDevice: fromDevice,
            appleKey: appleKey,
            macAddress: macAddress,
            profilePicture: null,
            socialId: null,
            password: password      // usually null/empty for social
        );

        string bodyJson = JsonUtility.ToJson(payload);

        // Mask sensitive fields in logs
        string masked = MaskSensitiveFields(
            bodyJson,
            new[] { "password", "appleKey", "fcmToken", "refreshToken" }
        );

        // Log masked cURL
        PrintCurl(
            "POST",
            SocialLoginUrl, // make sure this is set to /api/user/auth-v2/social/game-login
            new Dictionary<string, string>
            {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
            },
            masked
        );

        Exception lastErr = null;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (social-login) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(SocialLoginUrl, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                    req.downloadHandler = new DownloadHandlerBuffer();
                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Content-Type", "application/json");
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {SocialLoginUrl} (social-login)");

                    var op = req.SendWebRequest();
                    float start = Time.realtimeSinceStartup;

                    while (!op.isDone)
                    {
                        if (ct.IsCancellationRequested)
                        {
                            req.Abort();
                            ct.ThrowIfCancellationRequested();
                        }

                        if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                        {
                            req.Abort();
                            throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                        }

                        await Task.Yield();
                    }

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif
                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    string preview = text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Persist / configure via tolerant builder (handles social + normal login shapes)
                        if (!TryBuildAndSaveSessionFromLoginResponse(
                                text,
                                out var _,
                                out var buildErr,
                                configureUserAuthOnSuccess,
                                persistSession))
                        {
                            throw new Exception($"[SocialLogin] Parse/persist failed: {buildErr ?? "Unknown shape"}");
                        }

                        // Try direct SocialLoginResponse
                        SocialLoginResponse social = null;
                        try { social = JsonUtility.FromJson<SocialLoginResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

                        if (social != null && social.data != null)
                            return social;

                        // If backend returned normal LoginResponse, normalize into SocialLoginResponse
                        LoginResponse lr = null;
                        try { lr = JsonUtility.FromJson<LoginResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

                        if (lr != null && lr.data != null)
                        {
                            return new SocialLoginResponse
                            {
                                status = lr.status,
                                message = lr.message,
                                data = new SocialLoginData
                                {
                                    user = lr.data.user,
                                    accessToken = string.IsNullOrWhiteSpace(lr.data.accessToken)
                                        ? lr.data.token
                                        : lr.data.accessToken,
                                    idToken = lr.data.idToken,
                                    refreshToken = lr.data.refreshToken,
                                    expiresIn = Math.Max(0, lr.data.expiresIn),
                                    isNewUser = false,
                                    requiresPasswordSetup = false
                                }
                            };
                        }

                        // Fallback: success but unknown format
                        return new SocialLoginResponse { status = true, message = "OK", data = null };
                    }

                    // Honor Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int ra) && ra > 0)
                        {
                            Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                            await Task.Delay(TimeSpan.FromSeconds(ra), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries)
                        continue;

                    throw new Exception(
                        $"Social login failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Social-login cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError($"[HTTP] Timeout (social-login): {tex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (social-login) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown social-login error");
    }


    // === Public API (callback-friendly shim) ===
    /// <summary>
    /// Callback wrapper around <see cref="SocialLoginAsync"/> for parity with older code.
    /// onSuccess receives raw response; onError receives a message.
    /// </summary>
    public static async void SocialLogin(
        string loginType,
        string email,
        string firstName,
        string lastName,
        string userName,
        string password,
        string role,
        string fcmToken,
        string appleKey,
        Action<string> onSuccess,
        Action<string> onError)
    {
        try
        {
            bool persist = PlayerPrefs.GetInt("IVX_auth.remember", 1) == 1;

            // Resolve device fields
            DeviceInfoHelper.GetLoginDeviceFields(out string fromDevice, out string macAddress);
            string resolvedFromDevice = string.IsNullOrWhiteSpace(fromDevice) ? "unity" : fromDevice;
            string resolvedFcm = string.IsNullOrWhiteSpace(fcmToken) ? macAddress : fcmToken;

            var payload = new PayloadSocialLoginV2(
                email: email,
                firstName: firstName,
                lastName: lastName,
                password: password,
                userName: userName,
                role: string.IsNullOrWhiteSpace(role) ? "user" : role.Trim(),
                fcmToken: resolvedFcm,
                fromDevice: resolvedFromDevice,
                appleKey: appleKey,
                macAddress: macAddress,
                loginType: loginType
            );

            string bodyJson = JsonUtility.ToJson(payload);
            string masked = MaskSensitiveFields(bodyJson, new[] { "password", "appleKey", "fcmToken", "refreshToken" });
            PrintCurl("POST", SocialLoginUrl,
                new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Content-Type", "application/json" }
                },
                masked
            );

            Exception lastErr = null;
            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                    float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                    Log($"[HTTP] Retry (social shim) #{attempt} after backoff {delaySec:0.00}s …");
                    await Task.Delay(TimeSpan.FromSeconds(delaySec));
                }

                using (var req = new UnityWebRequest(SocialLoginUrl, UnityWebRequest.kHttpVerbPOST))
                {
                    try
                    {
                        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
                        req.downloadHandler = new DownloadHandlerBuffer();
                        req.SetRequestHeader("Accept", "application/json");
                        req.SetRequestHeader("Content-Type", "application/json");
                        req.timeout = RequestTimeoutSeconds;

                        var op = req.SendWebRequest();
                        float start = Time.realtimeSinceStartup;

                        while (!op.isDone)
                        {
                            if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                            {
                                req.Abort();
                                throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                            }
                            await Task.Yield();
                        }

#if UNITY_2020_1_OR_NEWER
                        bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                        bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                    bool isNetErr  = req.isNetworkError;
                    bool isHttpErr = req.isHttpError;
#endif
                        long code = req.responseCode;
                        string text = req.downloadHandler?.text ?? "";
                        string preview = text.Length > 1500 ? text.Substring(0, 1500) + "...(truncated)" : text;
                        Log($"[HTTP] ← {code} {preview}");

                        if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                        {
                            if (TryBuildAndSaveSessionFromLoginResponse(
                                    text,
                                    out var s,
                                    out var buildErr,
                                    configureUserAuthOnSuccess: true,
                                    persistSession: persist))
                            {
                                Log($"[SOCIAL LOGIN] Session persisted ({loginType}). token={PreviewToken(s?.accessToken)}");
                                onSuccess?.Invoke(text);
                                return;
                            }
                            else
                            {
                                LogError($"[SOCIAL LOGIN] Could not store session: {buildErr}");
                                onError?.Invoke(buildErr ?? "Invalid social login response.");
                                return;
                            }
                        }

                        if (code == 429 && attempt < MaxRetries)
                        {
                            var retryAfter = req.GetResponseHeader("Retry-After");
                            if (int.TryParse(retryAfter, out int ra) && ra > 0)
                            {
                                Log($"[HTTP] 429 Rate limited – honoring Retry-After: {ra}s");
                                await Task.Delay(TimeSpan.FromSeconds(ra));
                                continue;
                            }
                        }

                        bool retryable = code == 408 || code == 429 || (code >= 500 && code <= 599) || isNetErr;
                        if (retryable && attempt < MaxRetries) continue;

                        onError?.Invoke($"HTTP {code}: {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                        return;
                    }
                    catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (social shim): {tex.Message}"); if (attempt >= MaxRetries) { onError?.Invoke(tex.Message); return; } }
                    catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (social shim) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) { onError?.Invoke(ex.Message); return; } }
                }
            }

            onError?.Invoke(lastErr?.Message ?? "Unknown social login error.");
        }
        catch (Exception e)
        {
            onError?.Invoke(e.Message);
        }
    }

    // === Tolerant session builder ===
    /// <summary>
    /// Accepts either the normal LoginResponse JSON OR the SocialLoginResponse JSON.
    /// - Persists to UserSessionManager
    /// - Optionally configures runtime user-auth
    /// </summary>
    public static bool TryBuildAndSaveSessionFromLoginResponse(
        string responseJson,
        out UserSessionManager.UserSession session,
        out string error,
        bool configureUserAuthOnSuccess = true,
        bool persistSession = true)
    {
        session = null;
        error = null;

        if (string.IsNullOrWhiteSpace(responseJson))
        {
            error = "Empty response.";
            return false;
        }

        try
        {
            // Try normal login shape first
            LoginResponse login = null;
            try { login = JsonUtility.FromJson<LoginResponse>(responseJson); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

            if (login != null && login.data != null && login.data.user != null &&
                (!string.IsNullOrEmpty(login.data.accessToken) || !string.IsNullOrEmpty(login.data.token)))
            {
                if (persistSession)
                {
                    try { UserSessionManager.SaveFromLoginResponse(login); }
                    catch (Exception e) { error = "Persist failed: " + e.Message; return false; }
                }

                if (configureUserAuthOnSuccess)
                {
                    string idpUser = login.data.user?.idpUsername;
                    string refresh = login.data.refreshToken;
                    string access = !string.IsNullOrWhiteSpace(login.data.accessToken) ? login.data.accessToken : login.data.token;
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long expEpoch = now + Math.Max(0, (login.data.expiresIn <= 0 ? 1800 : login.data.expiresIn));
                    if (!string.IsNullOrWhiteSpace(idpUser) && !string.IsNullOrWhiteSpace(refresh) && !string.IsNullOrWhiteSpace(access))
                    {
                        ConfigureUserAuth(true, idpUser, refresh, access, expEpoch);
                    }
                }

                session = UserSessionManager.Current;
                return true;
            }

            // Try social envelope
            SocialLoginResponse social = null;
            try { social = JsonUtility.FromJson<SocialLoginResponse>(responseJson); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

            if (social != null && social.data != null && social.data.user != null)
            {
                // Normalize to LoginResponse shape used by UserSessionManager
                var normalized = new LoginResponse
                {
                    status = social.status,
                    message = social.message,
                    data = new LoginData
                    {
                        user = social.data.user,
                        accessToken = social.data.accessToken,
                        idToken = social.data.idToken,
                        refreshToken = social.data.refreshToken,
                        // clamp to int safely
                        expiresIn = (int)Math.Max(0, Math.Min(int.MaxValue, social.data.expiresIn))
                    }
                };

                if (persistSession)
                {
                    try { UserSessionManager.SaveFromLoginResponse(normalized); }
                    catch (Exception e) { error = "Persist failed: " + e.Message; return false; }
                }

                if (configureUserAuthOnSuccess)
                {
                    string idpUser = normalized.data.user?.idpUsername;
                    string refresh = normalized.data.refreshToken;
                    string access = !string.IsNullOrWhiteSpace(normalized.data.accessToken) ? normalized.data.accessToken : normalized.data.token;
                    long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    long expEpoch = now + Math.Max(0, (normalized.data.expiresIn <= 0 ? 1800 : normalized.data.expiresIn));
                    if (!string.IsNullOrWhiteSpace(idpUser) && !string.IsNullOrWhiteSpace(refresh) && !string.IsNullOrWhiteSpace(access))
                    {
                        ConfigureUserAuth(true, idpUser, refresh, access, expEpoch);
                    }
                }

                session = UserSessionManager.Current;
                return true;
            }

            error = "Unrecognized response format.";
            return false;
        }
        catch (Exception ex)
        {
            error = "Parse error: " + ex.Message;
            return false;
        }
    }

    // === Local helpers (region-scoped) ===
    private static string PreviewToken(string token)
    {
        if (string.IsNullOrEmpty(token)) return "<null>";
        return token.Length <= 12 ? token : token.Substring(0, 6) + "..." + token.Substring(token.Length - 4);
    }

    #endregion

    #region OAuth
    private static async Task<string> EnsureTokenAsync(CancellationToken ct)
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc.AddSeconds(-30)) return _accessToken;
        await _tokenLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiryUtc.AddSeconds(-30)) return _accessToken;
            return await FetchAccessTokenAsync(ct);
        }
        finally { _tokenLock.Release(); }
    }

    private static async Task<string> FetchAccessTokenAsync(CancellationToken ct)
    {
        var creds = new OAuthClientCredentials { client_id = ClientId, client_secret = ClientSecret };
        string bodyJson = JsonUtility.ToJson(creds);
        using (var req = new UnityWebRequest(OAuthTokenUrl, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(bodyJson));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");
            req.SetRequestHeader("accept", "application/json");
            string basic = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ClientId}:{ClientSecret}"));
            req.SetRequestHeader("Authorization", "Basic " + basic);
            //  if (!string.IsNullOrEmpty(UserAgent)) req.SetRequestHeader("User-Agent", UserAgent);
            req.timeout = RequestTimeoutSeconds;

            PrintCurl("POST", OAuthTokenUrl, new Dictionary<string, string> {
                { "Content-Type", "application/json" }, { "accept", "application/json" }, { "Authorization", "Basic ****" }
            }, bodyJson);

            var op = req.SendWebRequest();
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                await Task.Yield();
            }

#if UNITY_2020_1_OR_NEWER
            bool ok = (req.result == UnityWebRequest.Result.Success) && req.responseCode >= 200 && req.responseCode < 300;
#else
            bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
            if (!ok) throw new Exception($"HTTP {(int)req.responseCode} : {req.error} : {req.downloadHandler?.text}");
            var text = req.downloadHandler.text ?? "{}";
            var tokenResp = JsonUtility.FromJson<OAuthTokenResponse>(text);
            if (tokenResp == null || string.IsNullOrEmpty(tokenResp.access_token)) throw new Exception("Token parse failed: " + text);
            _accessToken = tokenResp.access_token;
            var seconds = (tokenResp.expires_in > 0 ? tokenResp.expires_in : 3300);
            _tokenExpiryUtc = DateTime.UtcNow.AddSeconds(seconds);
            Log("[OAUTH] token acquired; expires_in=" + seconds + "s");
            return _accessToken;
        }
    }

    public static async Task<string> EnsureTokenForExternalUseAsync(CancellationToken ct = default) => await EnsureTokenAsync(ct);






    #endregion

    #region Refresh Token 

    public static void ConfigureUserAuth(
    bool useUserAuthToken,
    string idpUsername,
    string refreshToken,
    string initialAccessToken = null,
    long? accessTokenExpiresInEpoch = null)
    {
        _useUserAuthToken = useUserAuthToken;
        _userIdpUsername = string.IsNullOrWhiteSpace(idpUsername) ? null : idpUsername.Trim();
        _userRefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken.Trim();

        _userAccessToken = string.IsNullOrWhiteSpace(initialAccessToken) ? null : initialAccessToken.Trim();
        if (accessTokenExpiresInEpoch.HasValue && accessTokenExpiresInEpoch.Value > 0)
        {
            try { _userAccessExpiryUtc = DateTimeOffset.FromUnixTimeSeconds(accessTokenExpiresInEpoch.Value).UtcDateTime; }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                _userAccessExpiryUtc = DateTime.UtcNow.AddMinutes(30);
            }
        }
        else
        {
            _userAccessExpiryUtc = DateTime.UtcNow;
        }
    }

    public static void SetUserRefreshCredentials(string idpUsername, string refreshToken)
    {
        _userIdpUsername = string.IsNullOrWhiteSpace(idpUsername) ? null : idpUsername.Trim();
        _userRefreshToken = string.IsNullOrWhiteSpace(refreshToken) ? null : refreshToken.Trim();
        _userAccessToken = null;
        _userAccessExpiryUtc = DateTime.UtcNow;
    }

    public static void ClearUserAuth()
    {
        _useUserAuthToken = false;
        _userIdpUsername = _userRefreshToken = _userAccessToken = null;
        _userAccessExpiryUtc = DateTime.MinValue;
    }

    private static bool UserAuthConfigured =>
        _useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken);


    private static async Task<string> EnsureUserAccessTokenAsync(CancellationToken ct)
    {
        if (!UserAuthConfigured)
            throw new InvalidOperationException("[UserAuth] idp-username and refreshToken must be configured via ConfigureUserAuth/SetUserRefreshCredentials.");

        await _userAuthLock.WaitAsync(ct);
        try
        {
            if (!string.IsNullOrEmpty(_userAccessToken) && DateTime.UtcNow < _userAccessExpiryUtc.AddSeconds(-30))
                return _userAccessToken;

            await RefreshUserTokenAsync(ct);
            if (string.IsNullOrEmpty(_userAccessToken))
                throw new Exception("[UserAuth] Refresh returned empty access token.");
            return _userAccessToken;
        }
        finally { _userAuthLock.Release(); }
    }


    public static Task RefreshUserTokenAsync(CancellationToken ct = default)
    {
        // Backwards-compat: some call sites (or older branches) used RefreshTokenInternalAsync.
        // Keep a single implementation to avoid drift.
        return RefreshTokenInternalAsync(ct);
    }

    /// <summary>
    /// Returns the current user access token, refreshing when needed.
    /// Requires prior <see cref="ConfigureUserAuth"/> / successful login with user-auth enabled.
    /// </summary>
    public static Task<string> GetUserAccessTokenAsync(CancellationToken ct = default)
    {
        return EnsureUserAccessTokenAsync(ct);
    }

    // NOTE: Unity error reports referenced this symbol in some versions of the file.
    // Keep it to prevent "missing method" compile failures across merges.
    private static async Task RefreshTokenInternalAsync(CancellationToken ct = default)
    {
        if (!UserAuthConfigured)
            throw new InvalidOperationException("[UserAuth] idp-username and refreshToken must be configured.");

        string url = RefreshTokenUrl(_userIdpUsername);
        var reqObj = new RefreshRequest { refreshToken = _userRefreshToken };
        string json = JsonUtility.ToJson(reqObj);

        PrintCurl("POST", url, new Dictionary<string, string> {
        { "accept", "application/json" }, { "Content-Type", "application/json" }
    }, MaskSensitiveFields(json, new[] { "refreshToken" }));

        using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
        {
            req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(json));
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("accept", "application/json");
            req.SetRequestHeader("Content-Type", "application/json");
            req.timeout = RequestTimeoutSeconds;

            var op = req.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds)
                {
                    req.Abort();
                    throw new TimeoutException($"Request timed out after {RequestTimeoutSeconds}s");
                }
                await Task.Yield();
            }

#if UNITY_2020_1_OR_NEWER
            bool ok = (req.result == UnityWebRequest.Result.Success) && req.responseCode >= 200 && req.responseCode < 300;
#else
        bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
            string text = req.downloadHandler?.text ?? "";
            Log($"[UserAuth] refresh ← {req.responseCode} {(text.Length > 0 ? "(body redacted)" : "")}");

            if (!ok) throw new Exception($"Refresh failed: HTTP {req.responseCode} - {req.error} - {text}");

            RefreshResponse resp = null;
            try { resp = JsonUtility.FromJson<RefreshResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }
            if (resp == null || resp.data == null || string.IsNullOrEmpty(resp.data.accessToken))
                throw new Exception("[UserAuth] Unexpected refresh response: " + text);

            _userAccessToken = resp.data.accessToken?.Trim();
            if (!string.IsNullOrWhiteSpace(resp.data.refreshToken))
                _userRefreshToken = resp.data.refreshToken.Trim();

            try { _userAccessExpiryUtc = DateTimeOffset.FromUnixTimeSeconds(resp.data.accessTokenExpiresIn).UtcDateTime; }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                _userAccessExpiryUtc = DateTime.UtcNow.AddMinutes(30);
            }
        }
    }



    #endregion

    #region AI Quiz
    public static async Task<List<AIQuizItem>> GenerateQuestionsAsync(
     string category,
     string topic,
     string difficulty,
     int count,
     CancellationToken ct = default,
     string language = null)
    {
        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
            throw new InvalidOperationException("APIManager.ClientId/ClientSecret not set.");

        category = string.IsNullOrWhiteSpace(category) ? "General Knowledge" : category.Trim();
        topic = string.IsNullOrWhiteSpace(topic) ? category : topic.Trim();
        difficulty = string.IsNullOrWhiteSpace(difficulty) ? "easy" : difficulty.Trim().ToLower();
        count = Mathf.Clamp(count, 1, 50);

        var reqObj = new AIQuizRequest(
            BuildPrompt(category, topic, difficulty, count, language),
            BuildReturnFormat(count),
            string.IsNullOrWhiteSpace(DefaultModel) ? "deepseek/deepseek-chat" : DefaultModel
        );

        var aggregated = new List<AIQuizItem>(count);
        var sessionSeen = new HashSet<string>(StringComparer.Ordinal);

        // ---- Local helpers (self-contained; no external dependencies needed) ----
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableToken(string token) => !string.IsNullOrWhiteSpace(token);

        async Task<string> GetBearerTokenResilientlyAsync(bool forceOAuth, CancellationToken tokenCt)
        {
            // If a previous attempt told us to force OAuth, skip user token path.
            if (!forceOAuth)
            {
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(null, tokenCt); // your existing resolver
                        if (IsUsableToken(userTok))
                        {
                            Log($"[Auth] Using user/provided token (attempt {i + 1}/{MaxResolveTries}).");
                            return userTok;
                        }
                    }
                    catch (Exception ex)
                    {
                        Log($"[Auth] Resolve attempt {i + 1}/{MaxResolveTries} failed: {ex.Message}");
                    }

                    if (_useUserAuthToken && UserAuthConfigured)
                    {
                        try
                        {
                            Log("[Auth] Refreshing user token …");
                            await RefreshUserTokenAsync(tokenCt);
                        }
                        catch (Exception rex)
                        {
                            Log($"[Auth] Refresh failed: {rex.Message}");
                        }
                    }
                }
            }

            // OAuth client-credentials fallback (thread-safe Acquire)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (!IsUsableToken(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        async Task FetchOnceOrThrow()
        {
            Exception lastErr = null;
            bool forceOAuthNextAttempt = false;

            for (int attempt = 0; attempt <= MaxRetries; attempt++)
            {
                if (attempt > 0)
                {
                    var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                    float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                    Log($"[AI QUIZ] Retry #{attempt} after backoff {delayS:0.00}s …");
                    await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
                }

                try
                {
                    // One call handles user token -> refresh -> OAuth fallback.
                    var bearer = await GetBearerTokenResilientlyAsync(forceOAuthNextAttempt, ct);

                    var raw = await PostJsonAsync(
                        AIPromptUrl,
                        JsonUtility.ToJson(reqObj),
                        $"Bearer {bearer}",
                        /*logBody*/ false,
                        ct);

                    // Try batch first
                    var batch = TryParseBatch(raw, category, difficulty, ct);
                    if (batch != null && batch.Count > 0)
                    {
                        AppendValidated(aggregated, sessionSeen, batch, count);
                        return;
                    }

                    // Try single
                    var single = TryParseSingle(raw, category, difficulty);
                    if (single != null)
                    {
                        AppendValidated(aggregated, sessionSeen, new List<AIQuizItem> { single }, count);
                        return;
                    }

                    // Try extracting inner JSON once (providers that wrap JSON in text)
                    var inner = TryExtractInnerJson(raw);
                    if (!string.Equals(inner, raw, StringComparison.Ordinal))
                    {
                        var batch2 = TryParseBatch(inner, category, difficulty, ct);
                        if (batch2 != null && batch2.Count > 0)
                        {
                            AppendValidated(aggregated, sessionSeen, batch2, count);
                            return;
                        }

                        var single2 = TryParseSingle(inner, category, difficulty);
                        if (single2 != null)
                        {
                            AppendValidated(aggregated, sessionSeen, new List<AIQuizItem> { single2 }, count);
                            return;
                        }
                    }

                    throw new Exception("Unrecognized AI response shape.");
                }
                catch (OperationCanceledException) { throw; }
                catch (Exception ex)
                {
                    lastErr = ex;

                    // If it smells like auth, force next attempt to go straight to OAuth token.
                    if (IsAuthError(ex))
                    {
                        Log("[AI QUIZ] Auth error detected; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;
                    }

                    if (attempt < MaxRetries) continue;
                    throw;
                }
            }

            if (lastErr != null) throw lastErr;
            throw new Exception("Fetch failed without specific error.");
        }

        // ---- First fetch ----
        await FetchOnceOrThrow();

        // Keep fetching until we have 'count' unique items
        while (aggregated.Count < count)
        {
            ct.ThrowIfCancellationRequested();
            await Task.Delay(TimeSpan.FromSeconds(BetweenCallsDelaySeconds), ct);
            await FetchOnceOrThrow();
        }

        if (aggregated.Count > count)
            aggregated.RemoveRange(count, aggregated.Count - count);

        Log($"[AI QUIZ] OK - returning {aggregated.Count} items.");
        return aggregated;
    }


    #endregion

}
