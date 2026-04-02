// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#include "ivx_http_internal.h"
#include <curl/curl.h>
#include <cstdio>
#include <thread>

namespace ivx {
namespace http {

static size_t writeCallback(char* ptr, size_t size, size_t nmemb, void* ud) {
    auto* buf = static_cast<std::string*>(ud);
    buf->append(ptr, size * nmemb);
    return size * nmemb;
}

static void perform(const std::string& url, const std::string& method,
                    const std::string& body, const std::string& bearerToken,
                    ResponseCb callback) {
    std::thread([=]() {
        CURL* curl = curl_easy_init();
        if (!curl) {
            if (callback) callback({0, "", "Failed to initialise libcurl", false});
            return;
        }

        HttpResponse resp;
        struct curl_slist* hdrs = nullptr;
        hdrs = curl_slist_append(hdrs, "Content-Type: application/json");
        if (!bearerToken.empty()) {
            hdrs = curl_slist_append(hdrs,
                ("Authorization: Bearer " + bearerToken).c_str());
        }

        curl_easy_setopt(curl, CURLOPT_URL, url.c_str());
        curl_easy_setopt(curl, CURLOPT_HTTPHEADER, hdrs);
        curl_easy_setopt(curl, CURLOPT_WRITEFUNCTION, writeCallback);
        curl_easy_setopt(curl, CURLOPT_WRITEDATA, &resp.body);
        curl_easy_setopt(curl, CURLOPT_TIMEOUT, 30L);
        curl_easy_setopt(curl, CURLOPT_CONNECTTIMEOUT, 10L);

        if (method == "POST") {
            curl_easy_setopt(curl, CURLOPT_POSTFIELDS, body.c_str());
        }

        CURLcode res = curl_easy_perform(curl);
        curl_easy_getinfo(curl, CURLINFO_RESPONSE_CODE, &resp.statusCode);
        curl_slist_free_all(hdrs);
        curl_easy_cleanup(curl);

        if (res != CURLE_OK) {
            resp.error   = curl_easy_strerror(res);
            resp.success = false;
        } else {
            resp.success = (resp.statusCode >= 200 && resp.statusCode < 300);
            if (!resp.success) {
                resp.error = "HTTP " + std::to_string(resp.statusCode);
            }
        }

        if (callback) callback(resp);
    }).detach();
}

void post(const std::string& url, const std::string& body,
          const std::string& bearerToken, ResponseCb callback) {
    perform(url, "POST", body, bearerToken, callback);
}

void get(const std::string& url, const std::string& bearerToken,
         ResponseCb callback) {
    perform(url, "GET", "", bearerToken, callback);
}

} // namespace http

// ---------------------------------------------------------------------------
// json helpers
// ---------------------------------------------------------------------------
namespace json {

std::string escape(const std::string& s) {
    std::string out;
    out.reserve(s.size() + 16);
    for (char c : s) {
        switch (c) {
            case '"':  out += "\\\""; break;
            case '\\': out += "\\\\"; break;
            case '\n': out += "\\n";  break;
            case '\r': out += "\\r";  break;
            case '\t': out += "\\t";  break;
            default:
                if (static_cast<unsigned char>(c) < 0x20) {
                    char buf[8];
                    std::snprintf(buf, sizeof(buf), "\\u%04x",
                                  static_cast<unsigned>(c));
                    out += buf;
                } else {
                    out += c;
                }
                break;
        }
    }
    return out;
}

static inline void skipWs(const std::string& s, size_t& pos) {
    while (pos < s.size() &&
           (s[pos] == ' ' || s[pos] == '\t' ||
            s[pos] == '\n' || s[pos] == '\r'))
        ++pos;
}

std::string getString(const std::string& json, const std::string& key) {
    const std::string needle = "\"" + key + "\"";
    auto pos = json.find(needle);
    if (pos == std::string::npos) return "";
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return "";
    ++pos;
    skipWs(json, pos);
    if (pos >= json.size() || json[pos] == 'n') return "";   // null
    if (json[pos] != '"') return "";
    ++pos;
    std::string result;
    while (pos < json.size() && json[pos] != '"') {
        if (json[pos] == '\\' && pos + 1 < json.size()) {
            ++pos;
            switch (json[pos]) {
                case '"':  result += '"';  break;
                case '\\': result += '\\'; break;
                case '/':  result += '/';  break;
                case 'n':  result += '\n'; break;
                case 'r':  result += '\r'; break;
                case 't':  result += '\t'; break;
                default:   result += json[pos]; break;
            }
        } else {
            result += json[pos];
        }
        ++pos;
    }
    return result;
}

bool getBool(const std::string& json, const std::string& key) {
    const std::string needle = "\"" + key + "\"";
    auto pos = json.find(needle);
    if (pos == std::string::npos) return false;
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return false;
    ++pos;
    skipWs(json, pos);
    return (json.compare(pos, 4, "true") == 0);
}

int32_t getInt(const std::string& json, const std::string& key) {
    const std::string needle = "\"" + key + "\"";
    auto pos = json.find(needle);
    if (pos == std::string::npos) return 0;
    pos = json.find(':', pos + needle.size());
    if (pos == std::string::npos) return 0;
    ++pos;
    skipWs(json, pos);
    bool neg = false;
    if (pos < json.size() && json[pos] == '-') { neg = true; ++pos; }
    int32_t val = 0;
    while (pos < json.size() && json[pos] >= '0' && json[pos] <= '9') {
        val = val * 10 + (json[pos] - '0');
        ++pos;
    }
    return neg ? -val : val;
}

std::vector<std::string> getArrayElements(const std::string& json) {
    std::vector<std::string> elems;
    size_t pos = json.find('[');
    if (pos == std::string::npos) return elems;
    ++pos;

    int depth     = 0;
    size_t start  = 0;
    bool inStr    = false;

    for (; pos < json.size(); ++pos) {
        char c = json[pos];
        if (c == '\\' && inStr) { ++pos; continue; }
        if (c == '"') { inStr = !inStr; continue; }
        if (inStr) continue;

        if (c == '{' || c == '[') {
            if (depth == 0) start = pos;
            ++depth;
        } else if (c == '}' || c == ']') {
            if (c == ']' && depth == 0) break;
            --depth;
            if (depth == 0) {
                elems.push_back(json.substr(start, pos - start + 1));
            }
        }
    }
    return elems;
}

std::vector<std::string> getStringArray(const std::string& json,
                                        const std::string& key) {
    std::vector<std::string> result;
    const std::string needle = "\"" + key + "\"";
    auto pos = json.find(needle);
    if (pos == std::string::npos) return result;
    pos = json.find('[', pos + needle.size());
    if (pos == std::string::npos) return result;
    ++pos;

    while (pos < json.size()) {
        skipWs(json, pos);
        if (pos >= json.size() || json[pos] == ']') break;
        if (json[pos] == ',') { ++pos; continue; }
        if (json[pos] != '"') { ++pos; continue; }
        ++pos;
        std::string val;
        while (pos < json.size() && json[pos] != '"') {
            if (json[pos] == '\\' && pos + 1 < json.size()) {
                ++pos;
                switch (json[pos]) {
                    case '"':  val += '"';  break;
                    case '\\': val += '\\'; break;
                    case 'n':  val += '\n'; break;
                    case 't':  val += '\t'; break;
                    default:   val += json[pos]; break;
                }
            } else {
                val += json[pos];
            }
            ++pos;
        }
        if (pos < json.size()) ++pos; // closing "
        result.push_back(std::move(val));
    }
    return result;
}

std::vector<std::string> getObjectArray(const std::string& json,
                                        const std::string& key) {
    const std::string needle = "\"" + key + "\"";
    auto kpos = json.find(needle);
    if (kpos == std::string::npos) return {};
    auto apos = json.find('[', kpos + needle.size());
    if (apos == std::string::npos) return {};
    return getArrayElements(json.substr(apos));
}

} // namespace json
} // namespace ivx
