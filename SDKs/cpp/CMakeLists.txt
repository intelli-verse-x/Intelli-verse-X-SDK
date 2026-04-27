cmake_minimum_required(VERSION 3.14)
project(IntelliVerseX VERSION 5.8.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

option(IVX_BUILD_SHARED "Build shared library" OFF)
option(IVX_BUILD_EXAMPLES "Build examples" OFF)
option(IVX_BUILD_TESTS "Build tests" OFF)
option(IVX_ENABLE_OPENXR "Enable OpenXR XR platform detection (requires OpenXR SDK)" OFF)

# Console platform bridge — exactly one should be set ON for a console
# build. If none are set, the desktop bridge is compiled.
option(IVX_PLATFORM_XBOX   "Compile Xbox / GDK console bridge"   OFF)
option(IVX_PLATFORM_PS5    "Compile PS5 / libNp console bridge"  OFF)
option(IVX_PLATFORM_SWITCH "Compile Nintendo Switch / NEX bridge" OFF)

include(FetchContent)

FetchContent_Declare(
  nakama-sdk
  GIT_REPOSITORY https://github.com/heroiclabs/nakama-cpp.git
  GIT_TAG        v2.8.5
  GIT_SHALLOW    TRUE
)

option(NAKAMA_BUILD_TESTS "Build Nakama tests" OFF)
option(NAKAMA_BUILD_EXAMPLES "Build Nakama examples" OFF)
FetchContent_MakeAvailable(nakama-sdk)

find_package(CURL REQUIRED)

set(IVX_SOURCES
    src/ivx_manager.cpp
    src/ivx_xr.cpp
    src/ivx_storage.cpp
    src/ivx_ai_client.cpp
    src/ivx_ai_assistant.cpp
    src/ivx_http_internal.cpp
    src/ivx_discord_social.cpp
    src/ivx_game_modes.cpp
    src/ivx_hiro_systems.cpp
    src/IVXDiscordLinkedChannels.cpp
    src/IVXDiscordDebug.cpp
    src/IVXSatori.cpp
)

# ---------------------------------------------------------------------------
# Console bridge translation unit selection. Mutually exclusive: pick one.
# ---------------------------------------------------------------------------
set(_ivx_platform_count 0)
foreach(_p IVX_PLATFORM_XBOX IVX_PLATFORM_PS5 IVX_PLATFORM_SWITCH)
    if(${${_p}})
        math(EXPR _ivx_platform_count "${_ivx_platform_count}+1")
    endif()
endforeach()
if(_ivx_platform_count GREATER 1)
    message(FATAL_ERROR "IVX: only one of IVX_PLATFORM_XBOX/PS5/SWITCH may be ON")
endif()

if(IVX_PLATFORM_XBOX)
    list(APPEND IVX_SOURCES src/platform/xbox/ivx_xbox_bridge.cpp)
elseif(IVX_PLATFORM_PS5)
    list(APPEND IVX_SOURCES src/platform/ps5/ivx_ps5_bridge.cpp)
elseif(IVX_PLATFORM_SWITCH)
    list(APPEND IVX_SOURCES src/platform/switch/ivx_switch_bridge.cpp)
else()
    list(APPEND IVX_SOURCES src/platform/desktop/ivx_desktop_bridge.cpp)
endif()

set(IVX_HEADERS
    include/intelliversex/ivx.h
    include/intelliversex/ivx_xr.h
    include/intelliversex/ivx_config.h
    include/intelliversex/ivx_types.h
    include/intelliversex/ivx_manager.h
    include/intelliversex/ivx_ai_client.h
    include/intelliversex/ivx_ai_types.h
    include/intelliversex/ivx_discord_social.h
    include/intelliversex/ivx_game_modes.h
    include/intelliversex/ivx_hiro_systems.h
    include/intelliversex/IVXAIAssistant.h
    include/intelliversex/IVXAIContentGenerator.h
    include/intelliversex/IVXAIModerator.h
    include/intelliversex/IVXAINPCDialogManager.h
    include/intelliversex/IVXAIProfiler.h
    include/intelliversex/IVXAIVoiceServices.h
    include/intelliversex/IVXDiscordDebug.h
    include/intelliversex/IVXDiscordLinkedChannels.h
    include/intelliversex/IVXDiscordMessages.h
    include/intelliversex/IVXDiscordModeration.h
    include/intelliversex/IVXDiscordSettings.h
    include/intelliversex/IVXSatori.h
)

if(IVX_BUILD_SHARED)
    add_library(intelliversex SHARED ${IVX_SOURCES})
    target_compile_definitions(intelliversex PRIVATE IVX_EXPORTS)
else()
    add_library(intelliversex STATIC ${IVX_SOURCES})
endif()

target_include_directories(intelliversex PUBLIC
    $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
    $<INSTALL_INTERFACE:include>
)

target_include_directories(intelliversex PRIVATE
    ${CMAKE_CURRENT_SOURCE_DIR}/src
)

target_link_libraries(intelliversex PUBLIC nakama-cpp CURL::libcurl)

if(IVX_ENABLE_OPENXR)
    find_package(OpenXR REQUIRED)
    target_compile_definitions(intelliversex PUBLIC IVX_HAS_OPENXR)
    target_link_libraries(intelliversex PUBLIC OpenXR::openxr_loader)
endif()

if(IVX_PLATFORM_XBOX)
    target_compile_definitions(intelliversex PUBLIC IVX_PLATFORM_XBOX)
elseif(IVX_PLATFORM_PS5)
    target_compile_definitions(intelliversex PUBLIC IVX_PLATFORM_PS5)
elseif(IVX_PLATFORM_SWITCH)
    target_compile_definitions(intelliversex PUBLIC IVX_PLATFORM_SWITCH)
endif()

set_target_properties(intelliversex PROPERTIES
    VERSION ${PROJECT_VERSION}
    SOVERSION 5
)

install(TARGETS intelliversex
    ARCHIVE DESTINATION lib
    LIBRARY DESTINATION lib
    RUNTIME DESTINATION bin
)

install(DIRECTORY include/intelliversex DESTINATION include)

if(IVX_BUILD_EXAMPLES)
    add_subdirectory(examples)
endif()
