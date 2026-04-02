// Copyright (c) 2026 Intelli-verse-X
// MIT License — see LICENSE in the project root.

#pragma once

#include <functional>
#include <stdexcept>
#include <string>
#include <unordered_map>
#include <vector>

namespace ivx {

enum class ContentCategory {
    Clean, Toxic, Spam, PII, Harassment, HateSpeech, SelfHarm, Sexual, Violence, Custom
};

enum class AIModerationSeverity { None, Low, Medium, High, Critical };

enum class AIModerationActionType { Allow, Warn, Replace, Block, Flag };

struct AIModerationResult {
    ContentCategory category = ContentCategory::Clean;
    AIModerationSeverity severity = AIModerationSeverity::None;
    float confidence = 0.f;
    AIModerationActionType suggestedAction = AIModerationActionType::Allow;
    std::string replacement;
    std::string originalText;
};

struct AIModerationRule {
    std::string pattern;
    ContentCategory category = ContentCategory::Clean;
    AIModerationActionType action = AIModerationActionType::Allow;
    std::string replacementText;
};

/// Text moderation — stub matching Unity IVXAIModerator.
class IVXAIModerator {
public:
    static IVXAIModerator& instance() {
        static IVXAIModerator inst;
        return inst;
    }

    bool isEnabled() const { return false; }

    void initialize(void*) { throw std::runtime_error("Not implemented"); }

    void classifyText(const std::string&, std::function<void(const AIModerationResult&)>) {
        throw std::runtime_error("Not implemented");
    }

    void filterMessage(const std::string&, std::function<void(const std::string&)>) {
        throw std::runtime_error("Not implemented");
    }

    void scanBatch(const std::vector<std::string>&, std::function<void(std::vector<AIModerationResult>)>) {
        throw std::runtime_error("Not implemented");
    }

    void addCustomRule(const AIModerationRule&) { throw std::runtime_error("Not implemented"); }
    void removeCustomRule(const std::string&) { throw std::runtime_error("Not implemented"); }
    void setCustomRules(const std::vector<AIModerationRule>&) { throw std::runtime_error("Not implemented"); }
    void clearCustomRules() { throw std::runtime_error("Not implemented"); }

    AIModerationResult checkLocalRules(const std::string&) { throw std::runtime_error("Not implemented"); }

    std::unordered_map<std::string, std::string> getDiscordModerationMetadata(const AIModerationResult&) {
        throw std::runtime_error("Not implemented");
    }

private:
    IVXAIModerator() = default;
};

} // namespace ivx
