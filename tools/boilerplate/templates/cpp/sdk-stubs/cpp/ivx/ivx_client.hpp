#pragma once

#include <string>
#include <map>

namespace ivx {

class IVXClient {
public:
    void configure(const std::string& game_id, const std::string& host, int port, const std::string& key) {}
    void authenticate_guest() {}
    void load_hiro_systems() {}
    void track_event(const std::string& event_name, const std::map<std::string, std::string>& properties) {}
};

} // namespace ivx
