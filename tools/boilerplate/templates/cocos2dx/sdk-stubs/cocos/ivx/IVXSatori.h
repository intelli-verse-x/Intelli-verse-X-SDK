#pragma once
#include <string>
#include <functional>

namespace ivx {
class IVXSatori {
public:
    static void getFeatureFlag(const std::string& flag_name, const std::function<void(bool)>& callback);
};
}
