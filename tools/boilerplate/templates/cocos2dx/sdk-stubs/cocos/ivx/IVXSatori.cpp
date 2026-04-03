#include "IVXSatori.h"

namespace ivx {
void IVXSatori::getFeatureFlag(const std::string& flag_name, const std::function<void(bool)>& callback) {
    if(callback) callback(true);
}
}
