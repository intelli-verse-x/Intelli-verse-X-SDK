#pragma once
#include <string>
#include <functional>
#include <map>

namespace ivx {
class IVXHiro {
public:
    static void getEconomy(const std::function<void(const std::map<std::string, int>&)>& callback);
};
}
