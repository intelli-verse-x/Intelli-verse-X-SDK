#include "IVXAuth.h"

namespace ivx {
void IVXAuth::login(const std::string& device_id, const std::function<void(bool, const std::string&)>& callback) {
    if(callback) callback(true, "user_123");
}
}
