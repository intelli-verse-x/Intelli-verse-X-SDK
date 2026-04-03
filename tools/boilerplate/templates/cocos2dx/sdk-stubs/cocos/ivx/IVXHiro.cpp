#include "IVXHiro.h"

namespace ivx {
void IVXHiro::getEconomy(const std::function<void(const std::map<std::string, int>&)>& callback) {
    if(callback) {
        std::map<std::string, int> eco = {{"coins", 1000}, {"gems", 50}};
        callback(eco);
    }
}
}
