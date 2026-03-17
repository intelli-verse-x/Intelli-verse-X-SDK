cmake_minimum_required(VERSION 3.14)
project(IntelliVerseX VERSION 5.1.0 LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

option(IVX_BUILD_SHARED "Build shared library" OFF)
option(IVX_BUILD_EXAMPLES "Build examples" OFF)
option(IVX_BUILD_TESTS "Build tests" OFF)

# Nakama: use IVX_NAKAMA_DIR if set (path to pre-built nakama install or extracted zip)
if(DEFINED IVX_NAKAMA_DIR)
  # Normalize path: backslashes escape in CMake (e.g. \n=newline); use forward slashes
  string(REPLACE "\\" "/" _ivx_nakama_norm "${IVX_NAKAMA_DIR}")
  set(IVX_NAKAMA_DIR "${_ivx_nakama_norm}")
  list(PREPEND CMAKE_PREFIX_PATH "${IVX_NAKAMA_DIR}")
endif()
find_package(nakama-sdk QUIET)
if(NOT nakama-sdk_FOUND AND DEFINED IVX_NAKAMA_DIR)
  # Pre-built zip may not include CMake config; create imported target from lib/include
  # Zip often extracts to a subfolder (e.g. win-x64-MinSizeRel); search for it
  set(_nakama_root "${IVX_NAKAMA_DIR}")
  if(NOT EXISTS "${_nakama_root}/include/nakama-cpp/Nakama.h")
    file(GLOB _subdirs "${IVX_NAKAMA_DIR}/*")
    foreach(_d ${_subdirs})
      if(IS_DIRECTORY "${_d}" AND EXISTS "${_d}/include/nakama-cpp/Nakama.h")
        set(_nakama_root "${_d}")
        break()
      endif()
    endforeach()
  endif()
  set(_nakama_libdir "${_nakama_root}/lib")
  set(_nakama_incdir "${_nakama_root}/include")
  if(EXISTS "${_nakama_incdir}/nakama-cpp/Nakama.h")
    set(_nakama_dll "${_nakama_libdir}/nakama-sdk.dll")
    if(NOT EXISTS "${_nakama_dll}")
      set(_nakama_dll "${_nakama_root}/bin/nakama-sdk.dll")
    endif()
    add_library(nakama-sdk SHARED IMPORTED)
    add_library(nakama-sdk::nakama-sdk ALIAS nakama-sdk)
    set_target_properties(nakama-sdk PROPERTIES
      IMPORTED_LOCATION "${_nakama_dll}"
      IMPORTED_IMPLIB "${_nakama_libdir}/nakama-sdk.lib"
      INTERFACE_INCLUDE_DIRECTORIES "${_nakama_incdir}"
    )
    if(CMAKE_BUILD_TYPE STREQUAL "Debug")
      if(EXISTS "${_nakama_libdir}/nakama-sdk-d.dll")
        set_target_properties(nakama-sdk PROPERTIES
          IMPORTED_LOCATION_DEBUG "${_nakama_libdir}/nakama-sdk-d.dll"
          IMPORTED_IMPLIB_DEBUG "${_nakama_libdir}/nakama-sdk-d.lib"
        )
      elseif(EXISTS "${_nakama_root}/bin/nakama-sdk-d.dll")
        set_target_properties(nakama-sdk PROPERTIES
          IMPORTED_LOCATION_DEBUG "${_nakama_root}/bin/nakama-sdk-d.dll"
          IMPORTED_IMPLIB_DEBUG "${_nakama_libdir}/nakama-sdk-d.lib"
        )
      endif()
    endif()
  else()
    message(FATAL_ERROR "IVX_NAKAMA_DIR=${IVX_NAKAMA_DIR} given but no valid lib/include found. Extract the pre-built zip so lib/ and include/nakama-cpp/ exist.")
  endif()
elseif(NOT nakama-sdk_FOUND)
  message(FATAL_ERROR "Could not find nakama-sdk. Set IVX_NAKAMA_DIR to the path of the extracted nakama pre-built (e.g. -DIVX_NAKAMA_DIR=\"D:/path/to/nakama-cpp-install\"). Download from https://github.com/heroiclabs/nakama-cpp/releases")
endif()

set(IVX_SOURCES
    src/ivx_manager.cpp
    src/ivx_storage.cpp
)

set(IVX_HEADERS
    include/intelliversex/ivx.h
    include/intelliversex/ivx_config.h
    include/intelliversex/ivx_types.h
    include/intelliversex/ivx_manager.h
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

# Use namespaced target (Conan/CMake config) or plain name (manual IVX_NAKAMA_DIR)
if(TARGET nakama-sdk::nakama-sdk)
  target_link_libraries(intelliversex PUBLIC nakama-sdk::nakama-sdk)
else()
  target_link_libraries(intelliversex PUBLIC nakama-sdk)
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

if(IVX_BUILD_TESTS)
    add_subdirectory(tests)
endif()
if(IVX_BUILD_EXAMPLES)
    add_subdirectory(examples)
endif()
