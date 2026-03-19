cmake_minimum_required(VERSION 3.14)
project(test_package LANGUAGES CXX)

find_package(intelliversex-cpp CONFIG REQUIRED)
find_package(nakama-sdk CONFIG REQUIRED)

add_executable(example src/example.cpp)
target_link_libraries(example PRIVATE intelliversex nakama-sdk::nakama-sdk)
