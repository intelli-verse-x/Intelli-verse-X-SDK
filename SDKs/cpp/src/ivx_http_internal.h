// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <cstdint>
#include <functional>
#include <string>
#include <vector>

namespace ivx {
namespace http {

struct HttpResponse {
    long statusCode = 0;
    std::string body;
    std::string error;
    bool success = false;
};

using ResponseCb = std::function<void(const HttpResponse&)>;

/// Async HTTP POST — callback fires on a background thread.
void post(const std::string& url, const std::string& body,
          const std::string& bearerToken, ResponseCb callback);

/// Async HTTP GET — callback fires on a background thread.
void get(const std::string& url, const std::string& bearerToken,
         ResponseCb callback);

} // namespace http

// ---------------------------------------------------------------------------
// Minimal JSON helpers — flat key look-ups only (no nested path support).
// Sufficient for parsing IntelliVerseX AI API responses whose top-level
// fields are simple scalars or homogeneous arrays.
// ---------------------------------------------------------------------------
namespace json {

std::string escape(const std::string& s);

std::string getString(const std::string& json, const std::string& key);
bool        getBool(const std::string& json, const std::string& key);
int32_t     getInt(const std::string& json, const std::string& key);

/// Split a top-level JSON array into its object-element strings.
/// e.g.  "[{...},{...}]" → vector of "{...}" strings.
std::vector<std::string> getArrayElements(const std::string& json);

/// Extract a JSON array of strings at `key`.
/// e.g.  {"sources":["a","b"]} → {"a","b"}
std::vector<std::string> getStringArray(const std::string& json,
                                        const std::string& key);

/// Extract an array of objects at `key`, returning each element as a string.
/// e.g.  {"steps":[{...},{...}]} → vector of "{...}" strings.
std::vector<std::string> getObjectArray(const std::string& json,
                                        const std::string& key);

} // namespace json
} // namespace ivx
