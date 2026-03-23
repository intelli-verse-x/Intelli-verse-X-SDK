message(STATUS "Conan: Using CMakeDeps conandeps_legacy.cmake aggregator via include()")
message(STATUS "Conan: It is recommended to use explicit find_package() per dependency instead")

find_package(intelliversex-cpp)
find_package(nakama-sdk)

set(CONANDEPS_LEGACY  intelliversex  nakama-sdk::nakama-sdk )