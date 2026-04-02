cmake_minimum_required(VERSION 3.14)
project(IntelliVerseX VERSION 5.8.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

option(IVX_BUILD_SHARED "Build shared library" OFF)
option(IVX_BUILD_EXAMPLES "Build examples" OFF)
option(IVX_BUILD_TESTS "Build tests" OFF)

find_package(nakama-sdk REQUIRED)

set(IVX_SOURCES
    src/ivx_manager.cpp
    src/ivx_xr.cpp
    src/ivx_storage.cpp
    src/ivx_ai_client.cpp
    src/ivx_discord_social.cpp
    src/ivx_game_modes.cpp
    src/ivx_hiro_systems.cpp
    src/IVXDiscordLinkedChannels.cpp
    src/IVXDiscordDebug.cpp
    src/IVXSatori.cpp
)

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

target_link_libraries(intelliversex PUBLIC nakama-sdk)

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
