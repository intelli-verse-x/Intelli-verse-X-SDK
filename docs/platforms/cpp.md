# C / C++

> IntelliVerseX native C++ library, built with CMake. Suitable for custom engines, embedded systems, and native applications.

## Requirements

- C++17 compiler (GCC 8+, Clang 7+, MSVC 2019+)
- CMake 3.14+
- [Nakama C++ SDK](https://github.com/heroiclabs/nakama-cpp) v2.8+

## Installation

### CMake (Recommended)

```cmake
add_subdirectory(path/to/intelliversex-cpp)
target_link_libraries(your_app PRIVATE intelliversex)
```

### FetchContent

```cmake
include(FetchContent)
FetchContent_Declare(intelliversex
    GIT_REPOSITORY https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK.git
    SOURCE_SUBDIR SDKs/cpp
)
FetchContent_MakeAvailable(intelliversex)
target_link_libraries(your_app PRIVATE intelliversex)
```

## Quick Start

```cpp
#include <intelliversex/ivx.h>

int main() {
    auto& mgr = ivx::Manager::instance();

    ivx::Config cfg;
    cfg.host = "127.0.0.1";
    cfg.port = 7350;
    cfg.serverKey = "defaultkey";
    cfg.debugLogs = true;

    mgr.init(cfg);
    mgr.authDevice("", []() {
        auto& m = ivx::Manager::instance();
        printf("Logged in: %s\n", m.username().c_str());
    });

    while (running) { mgr.tick(); }
    return 0;
}
```

## Nakama Client

Built on [nakama-cpp](https://github.com/heroiclabs/nakama-cpp) (87 stars, 31 forks).

## Source

[SDKs/cpp/](https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK/tree/main/SDKs/cpp)
