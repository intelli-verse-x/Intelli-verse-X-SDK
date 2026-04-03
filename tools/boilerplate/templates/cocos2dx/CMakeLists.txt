cmake_minimum_required(VERSION 3.20)
project({{game_name}}_cocos LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

set(COCOS2DX_ROOT "" CACHE PATH "Path to cocos2d-x (for include dirs)")

include(FetchContent)
# FetchContent_Declare(
#   intelliversex_sdk
#   GIT_REPOSITORY https://github.com/intelliversex/sdk-cpp.git
#   GIT_TAG v5.8.0
# )
# FetchContent_MakeAvailable(intelliversex_sdk)

add_library(ivx_sdk_stubs STATIC
  sdk-stubs/cocos/ivx/IVXManager.cpp
  sdk-stubs/cocos/ivx/IVXAuth.cpp
  sdk-stubs/cocos/ivx/IVXHiro.cpp
  sdk-stubs/cocos/ivx/IVXSatori.cpp
)
target_include_directories(ivx_sdk_stubs PUBLIC
  "${CMAKE_CURRENT_SOURCE_DIR}/sdk-stubs/cocos"
)

add_library(ivx_game_classes STATIC
  Classes/GameBootstrap.cpp
  Classes/MainMenuScene.cpp
  Classes/StoreScene.cpp
  Classes/AchievementsScene.cpp
  Classes/DailyRewardsScene.cpp
  Classes/EnergyScene.cpp
  Classes/LeaderboardScene.cpp
  Classes/SettingsScene.cpp
)
target_include_directories(ivx_game_classes PUBLIC
  "${CMAKE_CURRENT_SOURCE_DIR}/Classes"
)
if(COCOS2DX_ROOT)
  target_include_directories(ivx_game_classes PUBLIC
    "${COCOS2DX_ROOT}/cocos"
    "${COCOS2DX_ROOT}/cocos/platform"
  )
endif()
target_link_libraries(ivx_game_classes PUBLIC ivx_sdk_stubs)
# After `cocos new`, link the engine target from the generated build, e.g.:
# target_link_libraries(ivx_game_classes PUBLIC cocos2d)
