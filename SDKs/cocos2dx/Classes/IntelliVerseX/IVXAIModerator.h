// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include "IntelliVerseX/IVXTypes.h"
#include <functional>
#include <string>
#include <vector>
#include <map>

namespace IntelliVerseX {

enum class IVXContentCategory {
    Clean, Toxic, Spam, PII, Harassment, HateSpeech, SelfHarm, Sexual, Violence, Custom
};

enum class IVXAIModerationSeverity { None, Low, Medium, High, Critical };

enum class IVXAIModerationActionType { Allow, Warn, Replace, Block, Flag };

struct IVXAIModerationResult {
    IVXContentCategory category = IVXContentCategory::Clean;
    IVXAIModerationSeverity severity = IVXAIModerationSeverity::None;
    float confidence = 0.f;
    IVXAIModerationActionType suggestedAction = IVXAIModerationActionType::Allow;
    std::string replacement;
    std::string originalText;
};

struct IVXAIModerationRule {
    std::string pattern;
    IVXContentCategory category = IVXContentCategory::Clean;
    IVXAIModerationActionType action = IVXAIModerationActionType::Allow;
    std::string replacementText;
};

/// AI moderation — stub matching Unity IVXAIModerator.
class IVXAIModerator {
public:
    static IVXAIModerator& getInstance();

    bool isEnabled() const;

    void initialize(void* config);
    void classifyText(const std::string& text, std::function<void(const IVXAIModerationResult&)> callback);
    void filterMessage(const std::string& text, std::function<void(const std::string&)> onFiltered);
    void scanBatch(const std::vector<std::string>& messages, std::function<void(std::vector<IVXAIModerationResult>)> onComplete);
    void addCustomRule(const IVXAIModerationRule& rule);
    void removeCustomRule(const std::string& pattern);
    void setCustomRules(const std::vector<IVXAIModerationRule>& rules);
    void clearCustomRules();
    IVXAIModerationResult checkLocalRules(const std::string& text);
    std::map<std::string, std::string> getDiscordModerationMetadata(const IVXAIModerationResult& result);

private:
    IVXAIModerator() = default;
};

} // namespace IntelliVerseX
