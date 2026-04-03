#include "IVXManager.h"

namespace ivx {
IVXManager* IVXManager::getInstance() {
    static IVXManager instance;
    return &instance;
}
void IVXManager::initialize(const std::string& game_id, const std::string& server_url) {}
void IVXManager::connect(const std::function<void(bool)>& callback) { if(callback) callback(true); }
}
