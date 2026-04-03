#pragma once
#include <string>
#include <functional>

namespace ivx {
class IVXManager {
public:
    static IVXManager* getInstance();
    void initialize(const std::string& game_id, const std::string& server_url);
    void connect(const std::function<void(bool)>& callback);
};
}
