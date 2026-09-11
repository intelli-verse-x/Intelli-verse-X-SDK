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
    #region Notes API
    public static async Task<NotesCreateResponse> CreateNotesJobAsync(
     string filePath,
     string type,
     string difficulty,
     string bearerToken,
     CancellationToken ct = default,
     string language = null) // <- keep parameter for future use
    {
        if (string.IsNullOrEmpty(filePath) || !System.IO.File.Exists(filePath))
            throw new ArgumentException("File path is null or does not exist.", nameof(filePath));

        string ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant();
        string autoType = (ext == ".docx") ? "docx" : (ext == ".pdf") ? "pdf" : null;
        type = string.IsNullOrWhiteSpace(type) ? (autoType ?? "pdf") : type.Trim().ToLowerInvariant();

        difficulty = string.IsNullOrWhiteSpace(difficulty) ? "beginner" : difficulty.Trim().ToLowerInvariant();
        if (difficulty != "beginner" && difficulty != "intermediate" && difficulty != "advanced")
            difficulty = "beginner";

        string mime = (ext == ".pdf") ? "application/pdf"
                   : (ext == ".docx") ? "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
                   : "application/octet-stream";

        string fileName = System.IO.Path.GetFileName(filePath);
        byte[] fileBytes = System.IO.File.ReadAllBytes(filePath);

        // -------- Build form (NO language field in body) --------
        var form = new WWWForm();
        form.AddField("type", type);
        form.AddField("difficulty", difficulty);
        form.AddBinaryData("file", fileBytes, fileName, mime);

        // -------- cURL debug (mask auth) --------
        var curlHeaders = new Dictionary<string, string>
    {
        { "accept", "application/json" },
        { "Authorization", "Bearer ****" }
    };
        if (!string.IsNullOrWhiteSpace(language))
        {
            // Express language preference via header instead of body field
            curlHeaders["Accept-Language"] = language.Trim();
        }

        var parts = new List<(string name, string value, bool isFile, string filePathPart, string contentType)>
    {
        ("type", type, false, null, null),
        ("difficulty", difficulty, false, null, null),
        ("file", null, true, filePath, mime)
    };

        PrintCurlMultipart(NotesCreateUrl, curlHeaders, parts);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;

            // If token doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0;
            foreach (var c in token)
                if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] IsUsableJwt parse failed: {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it first.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries)
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
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

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (multipart) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            UnityWebRequest req = null;
            try
            {
                var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                var sub = TryGetJwtSub(authToken);
                if (!string.IsNullOrEmpty(sub))
                    Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                req = UnityWebRequest.Post(NotesCreateUrl, form);

#if !UNITY_2022_3_OR_NEWER
            // Some proxies/backends reject chunked multipart; send Content-Length instead.
            req.chunkedTransfer = false;
#endif

                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + authToken);

                // Express language via header (server currently rejects body property "language")
                if (!string.IsNullOrWhiteSpace(language))
                    req.SetRequestHeader("Accept-Language", language.Trim());

                req.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] POST {NotesCreateUrl} (multipart) file={fileName}, type={type}, difficulty={difficulty}");

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

                long code = req.responseCode;
                string text = req.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
            bool isSuccess = !req.isNetworkError && !req.isHttpError && code >= 200 && code < 300;
#endif

                if (isSuccess)
                {
                    Exception parseEx;
                    var resp = TryFromJson<NotesCreateResponse>(text, out parseEx)
                               ?? new NotesCreateResponse { jobId = "", status = "unknown", message = text };

                    // If server says failed or invalid input, bubble payload so UI can show message.
                    var lower = text.ToLowerInvariant();
                    if (lower.Contains("\"status\":\"failed\"") || lower.Contains("invalid input syntax for type uuid"))
                    {
                        return resp;
                    }

                    return resp;
                }

                // Auth: force OAuth next attempt on 401
                if (code == 401)
                {
                    Log("[HTTP] 401 on notes upload; forcing OAuth token on next attempt.");
                    forceOAuthNextAttempt = true;

                    // Re-acquire OAuth immediately once if we already were using OAuth
                    if (attempt < MaxRetries)
                    {
                        try { await EnsureTokenAsync(ct); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }
                        continue;
                    }
                }

                // Handle retryable statuses
                if (code == 429 && attempt < MaxRetries)
                {
                    var retryAfter = req.GetResponseHeader("Retry-After");
                    if (int.TryParse(retryAfter, out int secs))
                    {
                        Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                        await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                        continue;
                    }
                }

                bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                if (retryable && attempt < MaxRetries) continue;

                if ((code >= 400 && code < 500) && code != 408 && code != 429)
                    throw new Exception($"Upload failed (client error): HTTP {code} - {req.error} - {text}");

                throw new Exception($"Upload failed: HTTP {code} - {req.error} - {text}");
            }
            catch (OperationCanceledException)
            {
                req?.Abort();
                LogError("[HTTP] Multipart upload cancelled.");
                throw;
            }
            catch (TimeoutException tex)
            {
                lastErr = tex;
                LogError($"[HTTP] Timeout: {tex.Message}");
                if (attempt >= MaxRetries) throw;
            }
            catch (Exception ex)
            {
                lastErr = ex;
                LogError($"[HTTP] Attempt (multipart) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                if (IsAuthError(ex))
                {
                    Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                    forceOAuthNextAttempt = true;
                }
                if (attempt >= MaxRetries) throw;
            }
            finally
            {
                req?.Dispose();
            }
        }

        throw lastErr ?? new Exception("Unknown multipart upload error");
    }

    public static async Task<NotesJobStatusResponse> GetNotesJobStatusAsync(
    string jobId,
    string bearerToken,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(jobId))
            throw new ArgumentException("jobId is required.", nameof(jobId));

        string url = NotesJobStatusUrl(jobId);

        PrintCurl("GET", url, new Dictionary<string, string> {
        { "Accept", "application/json" }, { "Authorization", "Bearer ****" }
    }, null);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // If it doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries).
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
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

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (status) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = UnityWebRequest.Get(url))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {url} (status)");

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

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool ok = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
                bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

                    if (ok)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<NotesJobStatusResponse>(text, out parseEx)
                                   ?? new NotesJobStatusResponse { jobId = jobId, status = "", message = text };

                        // Return parsed payload even if server indicates failure—UI can inspect status/message.
                        return resp;
                    }

                    // Auth: force OAuth next attempt on 401
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on status; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        if (attempt < MaxRetries)
                        {
                            // If we were already on OAuth, reacquire once and retry.
                            try { await EnsureTokenAsync(ct); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }
                            continue;
                        }
                    }

                    // Respect Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                    if (retryable && attempt < MaxRetries) continue;

                    if ((code >= 400 && code < 500) && code != 408 && code != 429)
                        throw new Exception($"Status failed (client error): HTTP {code} - {req.error} - {text}");

                    throw new Exception($"Status failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Status request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (status): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (status) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown status request error");
    }

    /// <summary>
    /// Unified note creation supporting all note types (file, URL, text).
    /// Supports: pdf, docx, pptx, xls/xlsx, csv, audio, video, image, srt, text, chatgpt, youtube, website
    /// </summary>
    public static async Task<NotesCreateResponse> CreateNoteUnifiedAsync(
        CreateNoteRequest request,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        string typeStr = request.type.ToString().ToLowerInvariant();
        string difficulty = string.IsNullOrWhiteSpace(request.difficulty) ? "beginner" : request.difficulty.Trim().ToLowerInvariant();
        if (difficulty != "beginner" && difficulty != "intermediate" && difficulty != "advanced")
            difficulty = "beginner";

        // Validate input based on type
        ValidateNoteInput(request);

        // Build form
        var form = new WWWForm();
        form.AddField("type", typeStr);
        form.AddField("difficulty", difficulty);
        
        // Skip duplicate check to allow reprocessing same URLs
        form.AddField("skipDuplicateCheck", "true");

        if (!string.IsNullOrEmpty(request.title))
            form.AddField("title", request.title);
        if (!string.IsNullOrEmpty(request.folderId))
            form.AddField("folderId", request.folderId);
        
        // Add language support for multilingual content extraction and generation
        string finalLanguage = !string.IsNullOrEmpty(request.language) ? request.language : (language ?? "en");
        form.AddField("language", finalLanguage);
        
        // Auto-generate flashcards and quiz (default: true for seamless experience)
        form.AddField("autoGenerateStudyMaterials", request.autoGenerateStudyMaterials ? "true" : "false");

        // Handle different input types
        bool hasFile = !string.IsNullOrEmpty(request.filePath) && System.IO.File.Exists(request.filePath);

        if (hasFile)
        {
            string fileName = System.IO.Path.GetFileName(request.filePath);
            byte[] fileBytes = System.IO.File.ReadAllBytes(request.filePath);
            string mime = GetMimeType(request.filePath, request.type);
            form.AddBinaryData("file", fileBytes, fileName, mime);
        }
        else if (request.type == NoteType.youtube)
        {
            string ytUrl = !string.IsNullOrEmpty(request.youtubeUrl) ? request.youtubeUrl : request.url;
            if (string.IsNullOrEmpty(ytUrl))
                throw new ArgumentException("YouTube URL is required for youtube type.");
            form.AddField("youtubeUrl", ytUrl);
        }
        else if (request.type == NoteType.website)
        {
            if (string.IsNullOrEmpty(request.url))
                throw new ArgumentException("URL is required for website type.");
            form.AddField("url", request.url);
        }
        else if (request.type == NoteType.gdrive)
        {
            if (string.IsNullOrEmpty(request.url))
                throw new ArgumentException("Google Drive URL is required for gdrive type.");
            form.AddField("url", request.url);
        }
        else if (request.type == NoteType.text || request.type == NoteType.chatgpt)
        {
            if (string.IsNullOrEmpty(request.text))
                throw new ArgumentException("Text content is required for text/chatgpt type.");
            form.AddField("text", request.text);
        }
        else if (!string.IsNullOrEmpty(request.s3Url))
        {
            form.AddField("s3Url", request.s3Url);
        }
        else if (!string.IsNullOrEmpty(request.url))
        {
            form.AddField("url", request.url);
        }

        // Use the shared upload logic
        return await UploadNotesFormAsync(form, typeStr, difficulty, bearerToken, ct, language);
    }

    private static void ValidateNoteInput(CreateNoteRequest request)
    {
        bool hasFile = !string.IsNullOrEmpty(request.filePath) && System.IO.File.Exists(request.filePath);
        bool hasUrl = !string.IsNullOrEmpty(request.url);
        bool hasS3Url = !string.IsNullOrEmpty(request.s3Url);
        bool hasYoutubeUrl = !string.IsNullOrEmpty(request.youtubeUrl);
        bool hasText = !string.IsNullOrEmpty(request.text);

        switch (request.type)
        {
            case NoteType.pdf:
            case NoteType.docx:
            case NoteType.pptx:
            case NoteType.xls:
            case NoteType.xlsx:
            case NoteType.csv:
            case NoteType.audio:
            case NoteType.video:
            case NoteType.image:
            case NoteType.srt:
                if (!hasFile && !hasUrl && !hasS3Url)
                    throw new ArgumentException($"File, URL, or S3 URL is required for {request.type} type.");
                break;

            case NoteType.youtube:
                if (!hasYoutubeUrl && !hasUrl)
                    throw new ArgumentException("YouTube URL is required for youtube type.");
                break;

            case NoteType.website:
                if (!hasUrl)
                    throw new ArgumentException("URL is required for website type.");
                break;

            case NoteType.gdrive:
                if (!hasUrl)
                    throw new ArgumentException("Google Drive/Docs URL is required for gdrive type.");
                break;

            case NoteType.text:
            case NoteType.chatgpt:
                if (!hasText)
                    throw new ArgumentException("Text content is required for text/chatgpt type.");
                break;
        }
    }

    private static string GetMimeType(string filePath, NoteType type)
    {
        string ext = System.IO.Path.GetExtension(filePath)?.ToLowerInvariant() ?? "";
        
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".xls" => "application/vnd.ms-excel",
            ".csv" => "text/csv",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".m4a" => "audio/m4a",
            ".ogg" => "audio/ogg",
            ".flac" => "audio/flac",
            ".mp4" => "video/mp4",
            ".mov" => "video/quicktime",
            ".avi" => "video/x-msvideo",
            ".mkv" => "video/x-matroska",
            ".webm" => "video/webm",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".heic" => "image/heic",
            ".srt" => "text/plain",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }

    private static async Task<NotesCreateResponse> UploadNotesFormAsync(
        WWWForm form,
        string type,
        string difficulty,
        string bearerToken,
        CancellationToken ct,
        string language)
    {
        // Local auth helpers
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            int dots = 0;
            foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] IsUsableJwt parse failed (v2): {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
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

            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // Main request loop
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (unified) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            UnityWebRequest req = null;
            try
            {
                var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);
                req = UnityWebRequest.Post(NotesCreateUrl, form);

#if !UNITY_2022_3_OR_NEWER
                req.chunkedTransfer = false;
#endif

                req.SetRequestHeader("Accept", "application/json");
                req.SetRequestHeader("Authorization", "Bearer " + authToken);

                if (!string.IsNullOrWhiteSpace(language))
                    req.SetRequestHeader("Accept-Language", language.Trim());

                req.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] POST {NotesCreateUrl} (unified) type={type}, difficulty={difficulty}");

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

                long code = req.responseCode;
                string text = req.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                bool isSuccess = (req.result == UnityWebRequest.Result.Success) && code >= 200 && code < 300;
#else
                bool isSuccess = !req.isNetworkError && !req.isHttpError && code >= 200 && code < 300;
#endif

                if (isSuccess)
                {
                    Exception parseEx;
                    var resp = TryFromJson<NotesCreateResponse>(text, out parseEx)
                               ?? new NotesCreateResponse { jobId = "", status = "unknown", message = text };
                    return resp;
                }

                if (code == 401)
                {
                    Log("[HTTP] 401 on notes upload; forcing OAuth token on next attempt.");
                    forceOAuthNextAttempt = true;
                    if (attempt < MaxRetries)
                    {
                        try { await EnsureTokenAsync(ct); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Token refresh failed: {ex.Message}"); }
                        continue;
                    }
                }

                if (code == 429 && attempt < MaxRetries)
                {
                    var retryAfter = req.GetResponseHeader("Retry-After");
                    if (int.TryParse(retryAfter, out int secs))
                    {
                        Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                        await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                        continue;
                    }
                }

                bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                if (retryable && attempt < MaxRetries) continue;

                if ((code >= 400 && code < 500) && code != 408 && code != 429)
                    throw new Exception($"Upload failed (client error): HTTP {code} - {req.error} - {text}");

                throw new Exception($"Upload failed: HTTP {code} - {req.error} - {text}");
            }
            catch (OperationCanceledException)
            {
                req?.Abort();
                LogError("[HTTP] Unified upload cancelled.");
                throw;
            }
            catch (TimeoutException tex)
            {
                lastErr = tex;
                LogError($"[HTTP] Timeout: {tex.Message}");
                if (attempt >= MaxRetries) throw;
            }
            catch (Exception ex)
            {
                lastErr = ex;
                LogError($"[HTTP] Attempt (unified) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                if (IsAuthError(ex))
                {
                    Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                    forceOAuthNextAttempt = true;
                }
                if (attempt >= MaxRetries) throw;
            }
            finally
            {
                req?.Dispose();
            }
        }

        throw lastErr ?? new Exception("Unknown unified upload error");
    }

    /// <summary>
    /// Quick helper to create a note from YouTube URL
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromYouTubeAsync(
        string youtubeUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.youtube,
            youtubeUrl = youtubeUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from website URL
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromWebsiteAsync(
        string websiteUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.website,
            url = websiteUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from Google Drive/Docs URL
    /// Supports: Google Docs, Sheets, Slides, and Drive file links
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromGoogleDriveAsync(
        string gdriveUrl,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.gdrive,
            url = gdriveUrl,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from plain text
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromTextAsync(
        string text,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = NoteType.text,
            text = text,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from S3 URL (auto-detects type from URL extension)
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromS3UrlAsync(
        string s3Url,
        NoteType? type = null,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        // Auto-detect type from URL extension if not specified
        NoteType detectedType = type ?? DetectTypeFromUrl(s3Url);

        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = detectedType,
            s3Url = s3Url,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    /// <summary>
    /// Quick helper to create a note from any URL (auto-detects type)
    /// </summary>
    public static Task<NotesCreateResponse> CreateNoteFromUrlAsync(
        string url,
        NoteType? type = null,
        string difficulty = "beginner",
        string title = null,
        string bearerToken = null,
        CancellationToken ct = default,
        string language = null)
    {
        // Auto-detect type from URL
        NoteType detectedType = type ?? DetectTypeFromUrl(url);

        return CreateNoteUnifiedAsync(new CreateNoteRequest
        {
            type = detectedType,
            url = url,
            difficulty = difficulty,
            title = title
        }, bearerToken, ct, language);
    }

    private static NoteType DetectTypeFromUrl(string url)
    {
        if (string.IsNullOrEmpty(url)) return NoteType.pdf;

        string lowerUrl = url.ToLowerInvariant();

        // Check for YouTube - comprehensive detection including shorts, embeds, live, mobile
        if (lowerUrl.Contains("youtube.com") || lowerUrl.Contains("youtu.be"))
            return NoteType.youtube;

        // Check for Google Drive/Docs - use gdrive type for server-side handling
        if (lowerUrl.Contains("docs.google.com") || lowerUrl.Contains("drive.google.com"))
        {
            return NoteType.gdrive;
        }

        // Get extension from URL path (before query params)
        try
        {
            var uri = new Uri(url);
            string path = uri.AbsolutePath.ToLowerInvariant().Split('?')[0];
            
            if (path.EndsWith(".pdf")) return NoteType.pdf;
            if (path.EndsWith(".docx") || path.EndsWith(".doc")) return NoteType.docx;
            if (path.EndsWith(".pptx") || path.EndsWith(".ppt")) return NoteType.pptx;
            if (path.EndsWith(".xlsx") || path.EndsWith(".xls")) return NoteType.xlsx;
            if (path.EndsWith(".csv")) return NoteType.csv;
            if (path.EndsWith(".mp3") || path.EndsWith(".wav") || path.EndsWith(".m4a") || path.EndsWith(".ogg") || path.EndsWith(".flac")) return NoteType.audio;
            if (path.EndsWith(".mp4") || path.EndsWith(".mov") || path.EndsWith(".avi") || path.EndsWith(".mkv") || path.EndsWith(".webm")) return NoteType.video;
            if (path.EndsWith(".jpg") || path.EndsWith(".jpeg") || path.EndsWith(".png") || path.EndsWith(".gif") || path.EndsWith(".webp") || path.EndsWith(".heic")) return NoteType.image;
            if (path.EndsWith(".srt")) return NoteType.srt;
            if (path.EndsWith(".txt")) return NoteType.text;
        }
        catch (Exception ex) { Debug.LogWarning($"[APIManager] Note type detection failed: {ex.Message}"); }

        // Default to website for unknown URLs
        return NoteType.website;
    }

    public static async Task<NotesContentResponse> GetNoteContentByIdAsync(
    string noteId,
    string bearerToken,
    CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        if (!LooksLikeGuid(noteId))
            throw new ArgumentException($"noteId must be a UUID. Got '{noteId}'.", nameof(noteId));

        string url = $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}";

        PrintCurl("GET", url, new Dictionary<string, string> {
        { "Accept", "application/json" }, { "Authorization", "Bearer ****" }
    }, null);

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // If it doesn't look like a JWT, treat as usable (opaque tokens supported).
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) If caller provided a token and it's usable, use it immediately.
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token (with refresh & retries).
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt); // your resolver
                        if (IsUsableJwt(userTok))
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

            // 3) OAuth client-credentials fallback (thread-safe)
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (note) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = UnityWebRequest.Get(url))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    req.SetRequestHeader("Accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {url} (note content)");

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

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
                bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

                    if (ok)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<NotesContentResponse>(text, out parseEx)
                                   ?? new NotesContentResponse { noteId = noteId, content = text };
                        return resp;
                    }

                    // Auth: force OAuth next attempt on 401
                    if (code == 401)
                    {
                        Log("[HTTP] 401 on note content; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        if (attempt < MaxRetries)
                        {
                            // If already using OAuth, reacquire once and retry.
                            try { await EnsureTokenAsync(ct); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }
                            continue;
                        }
                    }

                    // Respect Retry-After for 429
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                    }

                    bool retryable = code == 408 || code == 423 || code == 425 || (code >= 500 && code <= 599);
                    if (retryable && attempt < MaxRetries) continue;

                    if ((code >= 400 && code < 500) && code != 408 && code != 429)
                        throw new Exception($"Get note failed (client error): HTTP {code} - {req.error} - {text}");

                    throw new Exception($"Get note failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Note content request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (note): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (note) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown note content request error");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // NOTES LIST, DETAILS & MANAGEMENT API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get paginated list of user's notes with optional filtering
    /// </summary>
    public static async Task<NotesListResponse> GetNotesListAsync(
        int page = 1,
        int pageSize = 20,
        string type = null,
        string search = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesListUrl(page, pageSize, type, search);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            PrintCurl("GET", url, new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Authorization", "Bearer ****" }
            }, null);

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetNotesListAsync] Error: {request.error}");
                return new NotesListResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            Log($"[GetNotesListAsync] Response: {responseText}");

            try
            {
                return JsonUtility.FromJson<NotesListResponse>(responseText);
            }
            catch (Exception ex)
            {
                Log($"[GetNotesListAsync] Parse error: {ex.Message}");
                return new NotesListResponse { status = false, message = "Failed to parse response" };
            }
        }
    }

    /// <summary>
    /// Get recently updated notes for the user
    /// </summary>
    public static async Task<NotesListResponse> GetRecentNotesAsync(
        int limit = 10,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesRecentUrl(limit);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetRecentNotesAsync] Error: {request.error}");
                return new NotesListResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            return JsonUtility.FromJson<NotesListResponse>(responseText);
        }
    }

    /// <summary>
    /// Get detailed note information including flashcards and quiz
    /// </summary>
    public static async Task<NoteDetailsResponse> GetNoteDetailsAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDetailsUrl(noteId);
        
        // Local auth helpers for resilient token handling
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            int dots = 0;
            foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s)
                {
                    s = s.Replace('-', '+').Replace('_', '/');
                    switch (s.Length % 4)
                    {
                        case 2: s += "=="; break;
                        case 3: s += "="; break;
                    }
                    return s;
                }

                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;

                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;

                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
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
                        catch (Exception ex) { Debug.LogWarning($"[APIManager] Token refresh failed: {ex.Message}"); }
                    }
                }
            }

            // Fall back to OAuth
            Log("[Auth] Falling back to OAuth token …");
            return await EnsureTokenAsync(tokenCt);
        }

        // First attempt
        string authToken = await GetBearerTokenResilientlyAsync(bearerToken, false, ct);
        
        async Task<NoteDetailsResponse> ExecuteRequest(string token)
        {
            using (var request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", $"Bearer {token}");
                request.SetRequestHeader("Accept", "application/json");
                request.timeout = RequestTimeoutSeconds;

                Log($"[HTTP] GET {url} (note details)");

                var op = request.SendWebRequest();
                float start = Time.realtimeSinceStartup;
                while (!op.isDone)
                {
                    if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                    if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                    await Task.Yield();
                }

                long code = request.responseCode;
                string responseText = request.downloadHandler?.text ?? "";
                Log($"[HTTP] ← {code} {responseText.Substring(0, Math.Min(200, responseText.Length))}");

                if (request.result != UnityWebRequest.Result.Success)
                {
                    throw new Exception($"HTTP {code} - {request.error}");
                }

                return JsonUtility.FromJson<NoteDetailsResponse>(responseText);
            }
        }

        try
        {
            return await ExecuteRequest(authToken);
        }
        catch (Exception ex) when (IsAuthError(ex))
        {
            // Retry with forced OAuth
            Log($"[GetNoteDetailsAsync] Auth error, retrying with OAuth: {ex.Message}");
            authToken = await GetBearerTokenResilientlyAsync(bearerToken, true, ct);
            try
            {
                return await ExecuteRequest(authToken);
            }
            catch (Exception retryEx)
            {
                Log($"[GetNoteDetailsAsync] Retry failed: {retryEx.Message}");
                return new NoteDetailsResponse { status = false, message = retryEx.Message };
            }
        }
        catch (Exception ex)
        {
            Log($"[GetNoteDetailsAsync] Error: {ex.Message}");
            return new NoteDetailsResponse { status = false, message = ex.Message };
        }
    }

    /// <summary>
    /// Get statistics overview for user's notes
    /// </summary>
    public static async Task<NotesStatsResponse> GetNotesStatsAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesStatsUrl;
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GetNotesStatsAsync] Error: {request.error}");
                return new NotesStatsResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            return JsonUtility.FromJson<NotesStatsResponse>(responseText);
        }
    }

    /// <summary>
    /// Delete a note and all associated data
    /// </summary>
    public static async Task<DeleteNoteResponse> DeleteNoteAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDeleteUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = UnityWebRequest.Delete(url))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.timeout = RequestTimeoutSeconds;

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > RequestTimeoutSeconds) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[DeleteNoteAsync] Error: {request.error}");
                return new DeleteNoteResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            try
            {
                return JsonUtility.FromJson<DeleteNoteResponse>(responseText);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] DeleteNoteAsync JSON parse failed: {ex.Message}");
                return new DeleteNoteResponse { status = true, message = "Note deleted successfully" };
            }
        }
    }

    /// <summary>
    /// Generate flashcards and quiz for a note
    /// </summary>
    public static async Task<FlashcardsQuizResponse> GenerateFlashcardsQuizzesAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesFlashcardsQuizzesUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            request.SetRequestHeader("Authorization", $"Bearer {authToken}");
            request.SetRequestHeader("Accept", "application/json");
            request.SetRequestHeader("Content-Type", "application/json");
            request.downloadHandler = new DownloadHandlerBuffer();
            int aiTimeout = 120; // Longer timeout for AI generation

            PrintCurl("POST", url, new Dictionary<string, string> {
                { "Accept", "application/json" },
                { "Authorization", "Bearer ****" }
            }, null);

            var op = request.SendWebRequest();
            float start = Time.realtimeSinceStartup;
            while (!op.isDone)
            {
                if (ct.IsCancellationRequested) { request.Abort(); ct.ThrowIfCancellationRequested(); }
                if (Time.realtimeSinceStartup - start > aiTimeout) { request.Abort(); break; }
                await Task.Yield();
            }

            if (request.result != UnityWebRequest.Result.Success)
            {
                Log($"[GenerateFlashcardsQuizzesAsync] Error: {request.error}");
                return new FlashcardsQuizResponse { status = false, message = request.error };
            }

            string responseText = request.downloadHandler.text;
            Log($"[GenerateFlashcardsQuizzesAsync] Response: {responseText}");

            return JsonUtility.FromJson<FlashcardsQuizResponse>(responseText);
        }
    }

    /// <summary>
    /// Poll for job status until completed or failed
    /// </summary>
    public static async Task<NotesJobStatusResponse> GetNoteJobStatusAsync(
        string jobId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        return await GetNotesJobStatusAsync(jobId, bearerToken, ct);
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // CHAT WITH NOTES API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Create a new chat session for a note
    /// </summary>
    public static async Task<CreateChatResponse> CreateNoteChatAsync(
        string noteId,
        string title = "New Chat",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesChatCreateUrl(noteId);
        string jsonBody = JsonUtility.ToJson(new { title = title ?? "New Chat" });

        var headers = new Dictionary<string, string>
        {
            { "Accept", "application/json" },
            { "Content-Type", "application/json" }
        };

        PrintCurl("POST", url, headers, jsonBody);

        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] POST {url} (create chat)");

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

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<CreateChatResponse>(text, out parseEx);
        }

        throw new Exception($"Create chat failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get chat history for a chat session
    /// </summary>
    public static async Task<ChatHistoryResponse> GetChatHistoryAsync(
        string chatId,
        int page = 1,
        int pageSize = 20,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesChatHistoryUrl(chatId, page, pageSize);

        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (chat history)");

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

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<ChatHistoryResponse>(text, out parseEx);
        }

        throw new Exception($"Get chat history failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Send a message and receive streaming response via SSE.
    /// Use the onChunk callback to receive streaming content.
    /// </summary>
    public static async Task SendChatMessageStreamingAsync(
        string chatId,
        string message,
        Action<string> onChunk,
        Action<ChatSource[]> onSourcesReceived = null,
        Action onComplete = null,
        Action<string> onError = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("message is required.", nameof(message));

        string url = NotesChatStreamUrl(chatId, message);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        Log($"[HTTP] SSE {url} (chat stream)");

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "text/event-stream");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 120; // Longer timeout for streaming

        // Use custom download handler for streaming
        var downloadHandler = new DownloadHandlerBuffer();
        req.downloadHandler = downloadHandler;

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;
        int lastProcessedLength = 0;
        bool sourcesReceived = false;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested)
            {
                req.Abort();
                ct.ThrowIfCancellationRequested();
            }

            if (Time.realtimeSinceStartup - start > 120f)
            {
                req.Abort();
                onError?.Invoke("Stream timed out");
                return;
            }

            // Process any new data that has arrived
            string currentText = downloadHandler.text ?? "";
            if (currentText.Length > lastProcessedLength)
            {
                string newData = currentText.Substring(lastProcessedLength);
                lastProcessedLength = currentText.Length;

                // Parse SSE events
                var lines = newData.Split('\n');
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;

                    if (line.StartsWith("data:"))
                    {
                        string jsonData = line.Substring(5).Trim();
                        if (!string.IsNullOrEmpty(jsonData))
                        {
                            try
                            {
                                Exception parseEx;
                                var chunk = TryFromJson<StreamChunk>(jsonData, out parseEx);
                                if (chunk != null)
                                {
                                    if (!string.IsNullOrEmpty(chunk.content))
                                    {
                                        onChunk?.Invoke(chunk.content);
                                    }
                                    if (chunk.sources != null && chunk.sources.Length > 0 && !sourcesReceived)
                                    {
                                        sourcesReceived = true;
                                        onSourcesReceived?.Invoke(chunk.sources);
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                Log($"[SSE] Parse error: {ex.Message}");
                            }
                        }
                    }
                }
            }

            await Task.Yield();
        }

        // Process any remaining data
        string finalText = downloadHandler.text ?? "";
        if (finalText.Length > lastProcessedLength)
        {
            string remainingData = finalText.Substring(lastProcessedLength);
            var lines = remainingData.Split('\n');
            foreach (var line in lines)
            {
                if (line.StartsWith("data:"))
                {
                    string jsonData = line.Substring(5).Trim();
                    if (!string.IsNullOrEmpty(jsonData))
                    {
                        try
                        {
                            Exception parseEx;
                            var chunk = TryFromJson<StreamChunk>(jsonData, out parseEx);
                            if (chunk?.content != null)
                            {
                                onChunk?.Invoke(chunk.content);
                            }
                        }
                        catch (Exception ex) { Debug.LogWarning($"[APIManager] JSON parse failed: {ex.Message}"); }
                    }
                }
            }
        }

        long code = req.responseCode;

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            onComplete?.Invoke();
        }
        else
        {
            onError?.Invoke($"Stream failed: HTTP {code} - {req.error}");
        }
    }

    /// <summary>
    /// Simple non-streaming chat message (polls for complete response)
    /// </summary>
    public static async Task<string> SendChatMessageAsync(
        string chatId,
        string message,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var fullResponse = new System.Text.StringBuilder();
        var tcs = new TaskCompletionSource<string>();

        await SendChatMessageStreamingAsync(
            chatId,
            message,
            chunk => fullResponse.Append(chunk),
            onComplete: () => tcs.TrySetResult(fullResponse.ToString()),
            onError: error => tcs.TrySetException(new Exception(error)),
            bearerToken: bearerToken,
            ct: ct
        );

        return await tcs.Task;
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // AI DEBATE API
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get debatable topics from a note's content
    /// </summary>
    public static async Task<DebateTopicsResponse> GetDebateTopicsAsync(
        string noteId,
        string difficulty = "intermediate",
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesDebateTopicsUrl(noteId, difficulty);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (debate topics)");

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

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateTopicsResponse>(text, out parseEx);
        }

        throw new Exception($"Get debate topics failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a debate session with AI taking the opposing position
    /// </summary>
    public static async Task<DebateStartResponse> StartDebateAsync(
        string noteId,
        string topic,
        string userPosition, // "for" or "against"
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition))
            throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateStartUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{userPosition}\"}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] POST {url} (start debate)");

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

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateStartResponse>(text, out parseEx);
        }

        throw new Exception($"Start debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Score a completed debate session
    /// </summary>
    public static async Task<DebateScoreResponse> ScoreDebateAsync(
        string chatId,
        string topic,
        string userPosition = null,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId))
            throw new ArgumentException("chatId is required.", nameof(chatId));
        if (string.IsNullOrWhiteSpace(topic))
            throw new ArgumentException("topic is required.", nameof(topic));

        string url = NotesDebateScoreUrl(chatId);
        string jsonBody = userPosition != null
            ? $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"userPosition\":\"{EscapeJsonString(userPosition)}\"}}"
            : $"{{\"topic\":\"{EscapeJsonString(topic)}\"}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 60; // Longer timeout for AI scoring

        Log($"[HTTP] POST {url} (score debate)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > 60f)
            {
                req.Abort();
                throw new TimeoutException("Debate scoring timed out after 60s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<DebateScoreResponse>(text, out parseEx);
        }

        throw new Exception($"Score debate failed: HTTP {code} - {req.error} - {text}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // ADVANCED DEBATE MODES
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Get available debate modes with descriptions
    /// </summary>
    public static async Task<DebateModesResponse> GetDebateModesAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        string url = NotesDebateModesUrl;
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (get debate modes)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<DebateModesResponse>(text, out _);
        throw new Exception($"Get debate modes failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a timed debate with time constraints per argument
    /// </summary>
    public static async Task<TimedDebateStartResponse> StartTimedDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int timeLimitSeconds = 120,
        int totalTimeLimitSeconds = 600,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateTimedUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"timeLimitSeconds\":{timeLimitSeconds},\"totalTimeLimitSeconds\":{totalTimeLimitSeconds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start timed debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<TimedDebateStartResponse>(text, out _);
        throw new Exception($"Start timed debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a multi-round debate tournament
    /// </summary>
    public static async Task<MultiRoundDebateStartResponse> StartMultiRoundDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int totalRounds = 3,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateMultiRoundUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"totalRounds\":{totalRounds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start multi-round debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<MultiRoundDebateStartResponse>(text, out _);
        throw new Exception($"Start multi-round debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Start a rapid-fire debate with short argument limits
    /// </summary>
    public static async Task<RapidFireDebateStartResponse> StartRapidFireDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        int maxArgumentLength = 280,
        int timeLimitSeconds = 30,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateRapidFireUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"maxArgumentLength\":{maxArgumentLength},\"timeLimitSeconds\":{timeLimitSeconds}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start rapid-fire debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<RapidFireDebateStartResponse>(text, out _);
        throw new Exception($"Start rapid-fire debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Advance to next round in multi-round debate
    /// </summary>
    public static async Task<NextRoundResponse> AdvanceToNextRoundAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateNextRoundUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (advance to next round)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<NextRoundResponse>(text, out _);
        throw new Exception($"Advance to next round failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Check status of a timed debate
    /// </summary>
    public static async Task<TimedDebateStatusResponse> CheckTimedDebateStatusAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateTimedStatusUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (check timed debate status)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<TimedDebateStatusResponse>(text, out _);
        throw new Exception($"Check timed debate status failed: HTTP {code} - {req.error} - {text}");
    }

    // ═══════════════════════════════════════════════════════════════════════════
    // OXFORD DEBATE API METHODS
    // ═══════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Start an Oxford-style formal debate with structured phases:
    /// Opening Statements → Rebuttals → Cross-Examination (optional) → Closing Statements
    /// </summary>
    public static async Task<OxfordDebateStartResponse> StartOxfordDebateAsync(
        string noteId,
        string topic,
        string userPosition,
        bool includesCrossExamination = true,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId)) throw new ArgumentException("noteId is required.", nameof(noteId));
        if (string.IsNullOrWhiteSpace(topic)) throw new ArgumentException("topic is required.", nameof(topic));
        if (string.IsNullOrWhiteSpace(userPosition)) throw new ArgumentException("userPosition is required.", nameof(userPosition));

        string url = NotesDebateOxfordStartUrl(noteId);
        string jsonBody = $"{{\"topic\":\"{EscapeJsonString(topic)}\",\"position\":\"{EscapeJsonString(userPosition)}\",\"includesCrossExamination\":{(includesCrossExamination ? "true" : "false")}}}";
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(jsonBody));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (start Oxford debate)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordDebateStartResponse>(text, out _);
        throw new Exception($"Start Oxford debate failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Advance to the next phase in an Oxford-style debate.
    /// Scores the current phase and transitions to the next.
    /// </summary>
    public static async Task<OxfordPhaseAdvanceResponse> AdvanceOxfordPhaseAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateOxfordAdvanceUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 30;

        Log($"[HTTP] POST {url} (advance Oxford phase)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordPhaseAdvanceResponse>(text, out _);
        throw new Exception($"Advance Oxford phase failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get the status of an Oxford-style debate including current phase, scores, etc.
    /// </summary>
    public static async Task<OxfordDebateStatusResponse> GetOxfordDebateStatusAsync(
        string chatId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(chatId)) throw new ArgumentException("chatId is required.", nameof(chatId));

        string url = NotesDebateOxfordStatusUrl(chatId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = UnityWebRequest.Get(url);
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = RequestTimeoutSeconds;

        Log($"[HTTP] GET {url} (get Oxford debate status)");

        var op = req.SendWebRequest();
        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok) return TryFromJson<OxfordDebateStatusResponse>(text, out _);
        throw new Exception($"Get Oxford debate status failed: HTTP {code} - {req.error} - {text}");
    }

    /// <summary>
    /// Get flashcards for a note (for link preview display)
    /// </summary>
    public static async Task<FlashcardsQuizResponse> GetFlashcardsAsync(
        string noteId,
        string bearerToken = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("noteId is required.", nameof(noteId));

        string url = NotesFlashcardsQuizzesUrl(noteId);
        string authToken = await ResolveBearerTokenAsync(bearerToken, ct);

        using var req = new UnityWebRequest(url, "POST");
        req.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes("{}"));
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Accept", "application/json");
        req.SetRequestHeader("Content-Type", "application/json");
        req.SetRequestHeader("Authorization", "Bearer " + authToken);
        req.timeout = 60; // Longer timeout for AI generation

        Log($"[HTTP] POST {url} (generate flashcards)");

        var op = req.SendWebRequest();
        float start = Time.realtimeSinceStartup;

        while (!op.isDone)
        {
            if (ct.IsCancellationRequested) { req.Abort(); ct.ThrowIfCancellationRequested(); }
            if (Time.realtimeSinceStartup - start > 60f)
            {
                req.Abort();
                throw new TimeoutException("Flashcard generation timed out after 60s");
            }
            await Task.Yield();
        }

        long code = req.responseCode;
        string text = req.downloadHandler?.text ?? "";
        Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
        bool ok = req.result == UnityWebRequest.Result.Success && code >= 200 && code < 300;
#else
        bool ok = !req.isHttpError && !req.isNetworkError && code >= 200 && code < 300;
#endif

        if (ok)
        {
            Exception parseEx;
            return TryFromJson<FlashcardsQuizResponse>(text, out parseEx);
        }

        throw new Exception($"Get flashcards failed: HTTP {code} - {req.error} - {text}");
    }

    private static string EscapeJsonString(string str)
    {
        if (string.IsNullOrEmpty(str)) return str;
        return str
            .Replace("\\", "\\\\")
            .Replace("\"", "\\\"")
            .Replace("\n", "\\n")
            .Replace("\r", "\\r")
            .Replace("\t", "\\t");
    }

    #endregion

    #region Notes: Generate Flashcards & Quizzes

    [Serializable]
    public class FlashcardData
    {
        public string question;
        public string answer;
    }

    [Serializable]
    public class QuizQuestion
    {
        public string question;
        public string[] options;
        public string answer;
        public string explanation;
        public string type;
        public int points;
        public int order;
        public float source;  // Can be timestamp (float) or index
        public string sourceType;
    }

    [Serializable]
    public class QuizData
    {
        public string title;
        public string description;
        public string difficulty;
        public int timeLimit;
        public QuizQuestion[] questions;
    }

    [Serializable]
    public class FlashcardsAndQuizResponse
    {
        public bool status;
        public string message;
        public FlashcardsAndQuizInnerData data;

        [Serializable]
        public class FlashcardsAndQuizInnerData
        {
            public FlashcardData[] flashcards;
            public QuizData quiz;
        }
    }

    public static async Task<FlashcardsAndQuizResponse> GenerateFlashcardsAndQuizAsync(
        string noteId,
        string bearerToken,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(noteId))
            throw new ArgumentException("NoteId cannot be empty.", nameof(noteId));

        // Backend requires UUID format in the path
        if (!LooksLikeGuid(noteId))
            throw new ArgumentException($"noteId must be a UUID. Got '{noteId}'.", nameof(noteId));

        string url = $"https://ai.intelli-verse-x.ai/api/ai/notes/{noteId}/generate-flashcards-quizzes";

        // Masked cURL (useful in logs)
        PrintCurl("POST", url, new Dictionary<string, string> {
        { "Accept", "*/*" },
        { "Content-Type", "application/json" },
        { "Authorization", "Bearer ****" }
    }, "{}");

        // -------- Local auth helpers (self-contained) --------
        const int MaxResolveTries = 3;

        static bool IsAuthError(Exception ex)
        {
            var m = ex?.Message ?? string.Empty;
            return m.IndexOf("401", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   m.IndexOf("unauthorized", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        static bool IsUsableJwt(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return false;
            // Allow opaque tokens; if not JWT-like, treat as usable.
            int dots = 0; foreach (var c in token) if (c == '.') dots++;
            if (dots != 2) return true;

            try
            {
                string Pad(string s) { s = s.Replace('-', '+').Replace('_', '/'); switch (s.Length % 4) { case 2: s += "=="; break; case 3: s += "="; break; } return s; }
                var parts = token.Split('.');
                var payloadJson = Encoding.UTF8.GetString(Convert.FromBase64String(Pad(parts[1])));
                var expIdx = payloadJson.IndexOf("\"exp\":", StringComparison.OrdinalIgnoreCase);
                if (expIdx < 0) return true;
                var tail = payloadJson.Substring(expIdx + 6);
                var end = tail.IndexOfAny(new[] { ',', '}', ' ' });
                var numStr = end >= 0 ? tail.Substring(0, end) : tail;
                if (!long.TryParse(new string(numStr.Where(char.IsDigit).ToArray()), out var exp)) return true;
                var expUtc = DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;
                return DateTime.UtcNow < expUtc.AddSeconds(-30);
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}");
                return true;
            }
        }

        async Task<string> GetBearerTokenResilientlyAsync(string provided, bool forceOAuth, CancellationToken tokenCt)
        {
            if (!forceOAuth)
            {
                // 1) Respect explicit provided token if usable
                if (!string.IsNullOrWhiteSpace(provided) && IsUsableJwt(provided))
                {
                    Log("[Auth] Using explicit provided bearer token.");
                    return provided;
                }

                // 2) Resolve user token with refresh+retries
                for (int i = 0; i < MaxResolveTries; i++)
                {
                    try
                    {
                        var userTok = await ResolveBearerTokenAsync(provided, tokenCt);
                        if (IsUsableJwt(userTok))
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

            // 3) OAuth client-credentials fallback
            Log("[Auth] Falling back to OAuth client-credentials token.");
            var oauth = await EnsureTokenAsync(tokenCt);
            if (string.IsNullOrWhiteSpace(oauth))
                throw new Exception("OAuth fallback token acquisition returned empty token.");
            return oauth;
        }

        // -------- Main request loop --------
        Exception lastErr = null;
        bool forceOAuthNextAttempt = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delayS = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (flashcards/quizzes) #{attempt} after backoff {delayS:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delayS), ct);
            }

            using (var req = new UnityWebRequest(url, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    var authToken = await GetBearerTokenResilientlyAsync(bearerToken, forceOAuthNextAttempt, ct);

#if UNITY_EDITOR
                    var sub = TryGetJwtSub(authToken);
                    if (!string.IsNullOrEmpty(sub))
                        Log($"[Auth] JWT sub: {sub}  (GUID? {LooksLikeGuid(sub)})");
#endif

                    // Some servers reject zero-length bodies; send "{}"
                    var bodyBytes = System.Text.Encoding.UTF8.GetBytes("{}");
                    req.uploadHandler = new UploadHandlerRaw(bodyBytes);
                    req.downloadHandler = new DownloadHandlerBuffer();

                    // Match cURL as closely as possible
                    req.SetRequestHeader("Accept", "*/*");
                    req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                    req.SetRequestHeader("Authorization", "Bearer " + authToken);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {url} (generate flashcards & quizzes)");

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

                    long code = req.responseCode;
                    string text = req.downloadHandler?.text ?? "";
                    Log($"[HTTP] ← {code} {text}");

#if UNITY_2020_1_OR_NEWER
                    bool isNetErr = req.result == UnityWebRequest.Result.ConnectionError;
                    bool isHttpErr = req.result == UnityWebRequest.Result.ProtocolError;
#else
                bool isNetErr  = req.isNetworkError;
                bool isHttpErr = req.isHttpError;
#endif

                    // 2xx — return parsed payload (even if status:false) so UI can show message
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        Exception parseEx;
                        var resp = TryFromJson<FlashcardsAndQuizResponse>(text, out parseEx)
                                   ?? new FlashcardsAndQuizResponse { status = false, message = text, data = null };

                        return resp;
                    }

                    // 202 Accepted — treat as retryable (server is still working)
                    if (code == 202)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs) && attempt < MaxRetries)
                        {
                            Log($"[HTTP] 202 Accepted. Retrying after {secs}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs), ct);
                            continue;
                        }
                        // else fall through to normal backoff loop
                    }

                    // 401 — force OAuth next attempt
                    if (code == 401 && attempt < MaxRetries)
                    {
                        Log("[HTTP] 401 on generate; forcing OAuth token on next attempt.");
                        forceOAuthNextAttempt = true;

                        // If already on OAuth, reacquire once and retry
                        try { await EnsureTokenAsync(ct); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }
                        continue;
                    }

                    // 429 — respect Retry-After if present
                    if (code == 429 && attempt < MaxRetries)
                    {
                        var retryAfter = req.GetResponseHeader("Retry-After");
                        if (int.TryParse(retryAfter, out int secs429))
                        {
                            Log($"[HTTP] 429 Too Many Requests. Retrying after {secs429}s …");
                            await Task.Delay(TimeSpan.FromSeconds(secs429), ct);
                            continue;
                        }
                        continue; // let outer backoff handle if header missing
                    }

                    // Other transient errors: 408/423/425/5xx or network errors
                    bool retryable = code == 408 || code == 423 || code == 425
                                     || (code >= 500 && code <= 599) || isNetErr;
                    if (retryable && attempt < MaxRetries) continue;

                    // Non-retryable 4xx — try to return server JSON so UI can show the message
                    if (code >= 400 && code < 500)
                    {
                        Exception parseClientEx;
                        var clientResp = TryFromJson<FlashcardsAndQuizResponse>(text, out parseClientEx)
                                         ?? new FlashcardsAndQuizResponse { status = false, message = text, data = null };
                        return clientResp;
                    }

                    throw new Exception($"Generate flashcards/quizzes failed: HTTP {code} - {req.error} - {text}");
                }
                catch (OperationCanceledException)
                {
                    LogError("[HTTP] Flashcards/quizzes request cancelled.");
                    throw;
                }
                catch (TimeoutException tex)
                {
                    lastErr = tex;
                    LogError($"[HTTP] Timeout (flashcards/quizzes): {tex.Message}");
                    if (attempt >= MaxRetries) throw;
                }
                catch (Exception ex)
                {
                    lastErr = ex;
                    LogError($"[HTTP] Attempt (flashcards/quizzes) #{attempt} failed: {ex.GetType().Name} – {ex.Message}");
                    if (IsAuthError(ex))
                    {
                        Log("[Auth] Auth-like error detected; will force OAuth on next attempt.");
                        forceOAuthNextAttempt = true;
                    }
                    if (attempt >= MaxRetries) throw;
                }
            }
        }

        throw lastErr ?? new Exception("Unknown flashcards/quizzes request error");
    }


    #endregion

    #region User Referral API

    // === Endpoint ===
    public static string ReferralUrlEndpoint = "https://api.intelli-verse-x.ai/api/user/referral/url";

    // === DTOs ===
    [Serializable]
    public class ReferralUrlData
    {
        public string referralCode;
        public string referralUrl;
    }

    [Serializable]
    public class ReferralUrlResponse
    {
        public bool status;
        public string message;
        public ReferralUrlData data;
        public object other; // ignored if present
    }

    /// <summary>
    /// Returns the current user's referral URL/code. Uses:
    ///  - Provided bearer token (if not null),
    ///  - Otherwise your runtime user-auth (auto-refresh),
    ///  - Otherwise the persisted token from UserSessionManager.Current.
    /// Robust against 401/408/429/5xx with exponential backoff and Retry-After.
    /// </summary>
    public static async Task<ReferralUrlResponse> GetReferralUrlAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        // Allow calling without a token: use persisted token if runtime user-auth is off.
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        // Masked cURL preview
        PrintCurl("GET", ReferralUrlEndpoint,
            new Dictionary<string, string> {
            { "accept", "application/json" },
            { "Authorization", "Bearer ****" }
            },
            null);

        Exception lastErr = null;
        bool triedAutoEnableFromSession = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (referral) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = UnityWebRequest.Get(ReferralUrlEndpoint))
            {
                try
                {
                    // Resolve bearer (supports runtime user-auth auto-refresh path)
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login or enable runtime user-auth.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] GET {ReferralUrlEndpoint} (referral/url)");

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
                    string preview = text.Length > 1000 ? text.Substring(0, 1000) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        ReferralUrlResponse resp = null;
                        try { resp = JsonUtility.FromJson<ReferralUrlResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

                        // Best-effort fallback if server shape changes
                        if (resp == null)
                            resp = new ReferralUrlResponse { status = true, message = string.IsNullOrWhiteSpace(text) ? "OK" : text, data = null };

                        // If API delivered only the code (rare), synthesize URL as a convenience
                        if (resp.data != null &&
                            string.IsNullOrWhiteSpace(resp.data.referralUrl) &&
                            !string.IsNullOrWhiteSpace(resp.data.referralCode))
                        {
                            // Fallback base derived from current public flow (safe default)
                            resp.data.referralUrl = "https://intelli-verse-x.ai/auth?mode=signup&ref=" + UnityWebRequest.EscapeURL(resp.data.referralCode);
                        }

                        return resp;
                    }

                    // Handle 401:
                    //  - If runtime user-auth is active/configured, refresh and retry.
                    //  - Else if we have a saved session with refresh creds, enable runtime user-auth once and retry.
                    if (code == 401)
                    {
                        if (_useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken) && attempt < MaxRetries)
                        {
                            Log("[HTTP] 401 (referral) – refreshing runtime user token and retrying …");
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // force ResolveBearerTokenAsync to use runtime auth on next loop
                            continue;
                        }

                        if (!triedAutoEnableFromSession && attempt < MaxRetries)
                        {
                            var s = UserSessionManager.Current;
                            if (s != null && !string.IsNullOrWhiteSpace(s.idpUsername) && !string.IsNullOrWhiteSpace(s.refreshToken))
                            {
                                triedAutoEnableFromSession = true;
                                Log("[HTTP] 401 (referral) – enabling runtime user-auth from saved session and retrying …");
                                try
                                {
                                    ConfigureUserAuth(
                                        useUserAuthToken: true,
                                        idpUsername: s.idpUsername,
                                        refreshToken: s.refreshToken,
                                        initialAccessToken: s.accessToken,
                                        accessTokenExpiresInEpoch: s.accessTokenExpiryEpoch > 0 ? s.accessTokenExpiryEpoch : (long?)null
                                    );
                                    tokenToTry = null; // next iteration will use runtime auth
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    LogError("[HTTP] Failed to enable runtime auth from session: " + e.Message);
                                }
                            }
                        }
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
                    if (retryable && attempt < MaxRetries) continue;

                    throw new Exception($"Referral URL failed: HTTP {code} - {(string.IsNullOrWhiteSpace(text) ? req.error : text)}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Referral request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (referral): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (referral) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown referral request error");
    }

    /// <summary>
    /// Convenience helper that returns just the referral URL string (or throws with context).
    /// Uses the same token rules as <see cref="GetReferralUrlAsync"/>.
    /// </summary>
    public static async Task<string> GetReferralUrlOnlyAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var res = await GetReferralUrlAsync(bearerToken, ct);
        if (res?.data?.referralUrl != null) return res.data.referralUrl;
        if (!string.IsNullOrWhiteSpace(res?.data?.referralCode))
            return "https://intelli-verse-x.ai/auth?mode=signup&ref=" + UnityWebRequest.EscapeURL(res.data.referralCode);
        throw new Exception(string.IsNullOrWhiteSpace(res?.message) ? "Referral URL unavailable." : res.message);
    }

    #endregion

    #region User: Claim Signup Reward

    // === Endpoint ===
    public static string ClaimSignupRewardEndpoint = "https://api.intelli-verse-x.ai/api/user/user/claim-signup-reward";

    // === DTOs ===
    [Serializable]
    public class SignupRewardData
    {
        public bool success;        // true if reward granted in this call
        public string message;      // e.g., "User has already received signup reward"
        public string rewardAmount; // "50.00" (string for shape tolerance)
    }

    [Serializable]
    public class SignupRewardResponse
    {
        public bool status;              // envelope status
        public string message;           // e.g., "Signup reward processed successfully"
        public SignupRewardData data;
        public object other;             // ignored
    }

    /// <summary>
    /// Calls the signup reward endpoint. Handles:
    /// - Provided bearer or runtime user-auth (with refresh on 401)
    /// - 408/429/5xx retry with exponential backoff + Retry-After
    /// - Robust JSON shape tolerance
    /// Returns the typed envelope (even when reward already claimed).
    /// </summary>
    public static async Task<SignupRewardResponse> ClaimSignupRewardAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        // Allow calling without explicit token; prefer runtime user-auth if enabled,
        // else fallback to persisted session's token for convenience.
        var fallbackFromSession = UserSessionManager.Current?.accessToken;
        var tokenToTry = bearerToken ?? fallbackFromSession;

        // Masked cURL preview
        PrintCurl("POST", ClaimSignupRewardEndpoint,
            new Dictionary<string, string> {
            { "accept", "application/json" },
            { "Authorization", "Bearer ****" }
            },
            "" // body is empty
        );

        Exception lastErr = null;
        bool triedAutoEnableFromSession = false;

        for (int attempt = 0; attempt <= MaxRetries; attempt++)
        {
            if (attempt > 0)
            {
                var jitter = 1f + (float)((_rng.NextDouble() * 2 - 1) * JitterPct);
                float delaySec = RetryBackoffBaseSeconds * Mathf.Pow(2f, attempt) * jitter;
                Log($"[HTTP] Retry (signup-reward) #{attempt} after backoff {delaySec:0.00}s …");
                await Task.Delay(TimeSpan.FromSeconds(delaySec), ct);
            }

            using (var req = new UnityWebRequest(ClaimSignupRewardEndpoint, UnityWebRequest.kHttpVerbPOST))
            {
                try
                {
                    // Some gateways prefer explicit zero-length body
                    req.uploadHandler = new UploadHandlerRaw(Array.Empty<byte>());
                    req.downloadHandler = new DownloadHandlerBuffer();

                    // Resolve token (runtime user-auth auto-refresh path supported below)
                    var resolved = await ResolveBearerTokenAsync(tokenToTry, ct);
                    if (string.IsNullOrWhiteSpace(resolved))
                        throw new InvalidOperationException("No bearer token available. Please login or enable runtime user-auth.");

                    req.SetRequestHeader("accept", "application/json");
                    req.SetRequestHeader("Authorization", "Bearer " + resolved);
                    req.timeout = RequestTimeoutSeconds;

                    Log($"[HTTP] POST {ClaimSignupRewardEndpoint} (claim-signup-reward)");

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
                    string preview = text.Length > 1000 ? text.Substring(0, 1000) + "...(truncated)" : text;
                    Log($"[HTTP] ← {code} {preview}");

                    // Success (2xx)
                    if (!isNetErr && !isHttpErr && code >= 200 && code < 300)
                    {
                        // Tolerant parse
                        SignupRewardResponse resp = null;
                        try { resp = JsonUtility.FromJson<SignupRewardResponse>(text); } catch (Exception ex) { Debug.LogWarning($"[APIManager] Operation failed: {ex.Message}"); }

                        if (resp == null)
                        {
                            // Fallback: synthesize minimal response from raw
                            resp = new SignupRewardResponse
                            {
                                status = true,
                                message = string.IsNullOrWhiteSpace(text) ? "OK" : text,
                                data = new SignupRewardData
                                {
                                    success = text.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                                    message = ExtractFieldLoose(text, "message"),
                                    rewardAmount = ExtractFieldLoose(text, "rewardAmount")
                                }
                            };
                        }
                        else
                        {
                            // If amount missing but present in raw, try to extract
                            if (resp.data != null && string.IsNullOrWhiteSpace(resp.data.rewardAmount))
                                resp.data.rewardAmount = ExtractFieldLoose(text, "rewardAmount");
                        }

                        return resp;
                    }

                    // 401 → attempt refresh (or auto-enable runtime user-auth from saved session once)
                    if (code == 401)
                    {
                        if (_useUserAuthToken && !string.IsNullOrWhiteSpace(_userIdpUsername) && !string.IsNullOrWhiteSpace(_userRefreshToken) && attempt < MaxRetries)
                        {
                            Log("[HTTP] 401 (signup-reward) – refreshing runtime user token and retrying …");
                            await RefreshUserTokenAsync(ct);
                            tokenToTry = null; // next loop will use runtime auth
                            continue;
                        }

                        if (!triedAutoEnableFromSession && attempt < MaxRetries)
                        {
                            var s = UserSessionManager.Current;
                            if (s != null && !string.IsNullOrWhiteSpace(s.idpUsername) && !string.IsNullOrWhiteSpace(s.refreshToken))
                            {
                                triedAutoEnableFromSession = true;
                                Log("[HTTP] 401 (signup-reward) – enabling runtime user-auth from saved session and retrying …");
                                try
                                {
                                    ConfigureUserAuth(
                                        useUserAuthToken: true,
                                        idpUsername: s.idpUsername,
                                        refreshToken: s.refreshToken,
                                        initialAccessToken: s.accessToken,
                                        accessTokenExpiresInEpoch: s.accessTokenExpiryEpoch > 0 ? s.accessTokenExpiryEpoch : (long?)null
                                    );
                                    tokenToTry = null;
                                    continue;
                                }
                                catch (Exception e)
                                {
                                    LogError("[HTTP] Failed to enable runtime auth from session: " + e.Message);
                                }
                            }
                        }
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
                    if (retryable && attempt < MaxRetries) continue;

                    // If non-2xx but server returned a body, try to surface amount/message
                    if (!string.IsNullOrWhiteSpace(text))
                    {
                        var fallback = new SignupRewardResponse
                        {
                            status = false,
                            message = $"HTTP {code}",
                            data = new SignupRewardData
                            {
                                success = text.IndexOf("\"success\":true", StringComparison.OrdinalIgnoreCase) >= 0,
                                message = ExtractFieldLoose(text, "message"),
                                rewardAmount = ExtractFieldLoose(text, "rewardAmount")
                            }
                        };
                        throw new Exception($"Signup reward failed: HTTP {code} - {(string.IsNullOrWhiteSpace(fallback.data?.message) ? text : fallback.data.message)}");
                    }

                    throw new Exception($"Signup reward failed: HTTP {code} - {req.error}");
                }
                catch (OperationCanceledException) { LogError("[HTTP] Signup-reward request cancelled."); throw; }
                catch (TimeoutException tex) { lastErr = tex; LogError($"[HTTP] Timeout (signup-reward): {tex.Message}"); if (attempt >= MaxRetries) throw; }
                catch (Exception ex) { lastErr = ex; LogError($"[HTTP] Attempt (signup-reward) #{attempt} failed: {ex.GetType().Name} – {ex.Message}"); if (attempt >= MaxRetries) throw; }
            }
        }

        throw lastErr ?? new Exception("Unknown signup-reward request error");
    }

    /// <summary>
    /// Convenience helper: returns (granted, amount, message) in one go.
    /// - granted: true if reward was granted in THIS call (false if it was already claimed)
    /// - amount: parsed decimal amount (0 if unavailable)
    /// - message: server message (e.g., "User has already received signup reward")
    /// </summary>
    public static async Task<(bool granted, decimal amount, string message)> ClaimSignupRewardSummaryAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var resp = await ClaimSignupRewardAsync(bearerToken, ct);

        bool granted = resp?.data?.success == true;

        // Parse amount robustly (string or number, invariant culture)
        decimal amount = 0m;
        string raw = resp?.data?.rewardAmount;
        if (!string.IsNullOrWhiteSpace(raw))
        {
            if (!decimal.TryParse(raw, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount))
            {
                // strip non-numeric except dot
                var cleaned = System.Text.RegularExpressions.Regex.Replace(raw, @"[^\d.]+", "");
                decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out amount);
            }
        }
        else
        {
            // Final fallback: scan last raw body captured in logs (not stored) is not available here,
            // so keep 0 if missing — server normally returns rewardAmount even when already claimed.
            amount = 0m;
        }

        string msg = resp?.data?.message ?? resp?.message ?? "";
        return (granted, amount, msg);
    }

    /// <summary>
    /// One-liner if you only need the numeric amount (throws with context if not available).
    /// Note: backend returns the amount even when already claimed.
    /// </summary>
    public static async Task<decimal> ClaimSignupRewardAmountOnlyAsync(
        string bearerToken = null,
        CancellationToken ct = default)
    {
        var (granted, amount, msg) = await ClaimSignupRewardSummaryAsync(bearerToken, ct);
        // We still return the amount regardless of granted/already-claimed.
        // If amount is zero and that's unexpected for your product, you can choose to throw instead:
        // if (amount <= 0m) throw new Exception(string.IsNullOrWhiteSpace(msg) ? "Reward amount unavailable." : msg);
        return amount;
    }

    // === Local helpers (region-scoped) ===
    private static string ExtractFieldLoose(string json, string field)
    {
        if (string.IsNullOrEmpty(json) || string.IsNullOrEmpty(field)) return null;

        // Matches:  "field": "value"  OR  "field": 50.00
        var m = System.Text.RegularExpressions.Regex.Match(
            json,
            $"\"{System.Text.RegularExpressions.Regex.Escape(field)}\"\\s*:\\s*(\"(?<s>[^\"]*)\"|(?<n>[-]?[0-9]+(?:\\.[0-9]+)?))",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline
        );
        if (!m.Success) return null;

        if (m.Groups["s"].Success) return m.Groups["s"].Value;
        if (m.Groups["n"].Success) return m.Groups["n"].Value;
        return null;
    }

    #endregion

    // ========================================================================
    // FRIENDS API
    // ========================================================================
    // Production-ready Friends system API calls
    // Uses centralized IVXURLs and IVXModels
    // ========================================================================

}
