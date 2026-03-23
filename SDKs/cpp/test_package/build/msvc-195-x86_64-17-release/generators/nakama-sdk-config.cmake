########## MACROS ###########################################################################
#############################################################################################

# Requires CMake > 3.15
if(${CMAKE_VERSION} VERSION_LESS "3.15")
    message(FATAL_ERROR "The 'CMakeDeps' generator only works with CMake >= 3.15")
endif()

if(nakama-sdk_FIND_QUIETLY)
    set(nakama-sdk_MESSAGE_MODE VERBOSE)
else()
    set(nakama-sdk_MESSAGE_MODE STATUS)
endif()

include(${CMAKE_CURRENT_LIST_DIR}/cmakedeps_macros.cmake)
include(${CMAKE_CURRENT_LIST_DIR}/nakama-sdkTargets.cmake)
include(CMakeFindDependencyMacro)

check_build_type_defined()

foreach(_DEPENDENCY ${nakama-sdk_FIND_DEPENDENCY_NAMES} )
    # Check that we have not already called a find_package with the transitive dependency
    if(NOT ${_DEPENDENCY}_FOUND)
        find_dependency(${_DEPENDENCY} REQUIRED ${${_DEPENDENCY}_FIND_MODE})
    endif()
endforeach()

set(nakama-sdk_VERSION_STRING "2.9.0")
set(nakama-sdk_INCLUDE_DIRS ${nakama-sdk_INCLUDE_DIRS_RELEASE} )
set(nakama-sdk_INCLUDE_DIR ${nakama-sdk_INCLUDE_DIRS_RELEASE} )
set(nakama-sdk_LIBRARIES ${nakama-sdk_LIBRARIES_RELEASE} )
set(nakama-sdk_DEFINITIONS ${nakama-sdk_DEFINITIONS_RELEASE} )


# Definition of extra CMake variables from cmake_extra_variables


# Only the last installed configuration BUILD_MODULES are included to avoid the collision
foreach(_BUILD_MODULE ${nakama-sdk_BUILD_MODULES_PATHS_RELEASE} )
    message(${nakama-sdk_MESSAGE_MODE} "Conan: Including build module from '${_BUILD_MODULE}'")
    include(${_BUILD_MODULE})
endforeach()


