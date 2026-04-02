cmake_minimum_required(VERSION 3.10)
project(IntelliVerseX VERSION 5.8.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

option(IVX_BUILD_EXAMPLES "Build example projects" OFF)

find_package(nakama-sdk REQUIRED)

set(IVX_SOURCES
    Classes/IntelliVerseX/IVXManager.cpp
    Classes/IntelliVerseX/IVXConfig.cpp
    Classes/IntelliVerseX/IVXAIClient.cpp
    Classes/IntelliVerseX/IVXDiscordSocial.cpp
    Classes/IntelliVerseX/IVXDiscordMessages.cpp
    Classes/IntelliVerseX/IVXDiscordModeration.cpp
    Classes/IntelliVerseX/IVXDiscordLinkedChannels.cpp
    Classes/IntelliVerseX/IVXDiscordDebug.cpp
    Classes/IntelliVerseX/IVXDiscordSettings.cpp
    Classes/IntelliVerseX/IVXSatori.cpp
    Classes/IntelliVerseX/IVXHiroSystems.cpp
    Classes/IntelliVerseX/IVXGameModes.cpp
    Classes/IntelliVerseX/IVXAINPCDialogManager.cpp
    Classes/IntelliVerseX/IVXAIAssistant.cpp
    Classes/IntelliVerseX/IVXAIModerator.cpp
    Classes/IntelliVerseX/IVXAIContentGenerator.cpp
    Classes/IntelliVerseX/IVXAIProfiler.cpp
    Classes/IntelliVerseX/IVXAIVoiceServices.cpp
)

set(IVX_HEADERS
    Classes/IntelliVerseX/IVXManager.h
    Classes/IntelliVerseX/IVXConfig.h
    Classes/IntelliVerseX/IVXTypes.h
)

add_library(intelliversex STATIC ${IVX_SOURCES} ${IVX_HEADERS})

target_include_directories(intelliversex PUBLIC
    ${CMAKE_CURRENT_SOURCE_DIR}/Classes
)

target_link_libraries(intelliversex PUBLIC
    nakama-sdk
)

if(IVX_BUILD_EXAMPLES)
    add_subdirectory(examples)
endif()
