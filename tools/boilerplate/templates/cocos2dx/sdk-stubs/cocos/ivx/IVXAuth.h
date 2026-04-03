#pragma once
#include <string>
#include <functional>

namespace ivx {
class IVXAuth {
public:
    static void login(const std::string& device_id, const std::function<void(bool, const std::string&)>& callback);
};
}
