########## MACROS ###########################################################################
#############################################################################################

# Requires CMake > 3.15
if(${CMAKE_VERSION} VERSION_LESS "3.15")
    message(FATAL_ERROR "The 'CMakeDeps' generator only works with CMake >= 3.15")
endif()

if(intelliversex-cpp_FIND_QUIETLY)
    set(intelliversex-cpp_MESSAGE_MODE VERBOSE)
else()
    set(intelliversex-cpp_MESSAGE_MODE STATUS)
endif()

include(${CMAKE_CURRENT_LIST_DIR}/cmakedeps_macros.cmake)
include(${CMAKE_CURRENT_LIST_DIR}/intelliversex-cppTargets.cmake)
include(CMakeFindDependencyMacro)

check_build_type_defined()

foreach(_DEPENDENCY ${intelliversex-cpp_FIND_DEPENDENCY_NAMES} )
    # Check that we have not already called a find_package with the transitive dependency
    if(NOT ${_DEPENDENCY}_FOUND)
        find_dependency(${_DEPENDENCY} REQUIRED ${${_DEPENDENCY}_FIND_MODE})
    endif()
endforeach()

set(intelliversex-cpp_VERSION_STRING "1.5.0")
set(intelliversex-cpp_INCLUDE_DIRS ${intelliversex-cpp_INCLUDE_DIRS_RELEASE} )
set(intelliversex-cpp_INCLUDE_DIR ${intelliversex-cpp_INCLUDE_DIRS_RELEASE} )
set(intelliversex-cpp_LIBRARIES ${intelliversex-cpp_LIBRARIES_RELEASE} )
set(intelliversex-cpp_DEFINITIONS ${intelliversex-cpp_DEFINITIONS_RELEASE} )


# Definition of extra CMake variables from cmake_extra_variables


# Only the last installed configuration BUILD_MODULES are included to avoid the collision
foreach(_BUILD_MODULE ${intelliversex-cpp_BUILD_MODULES_PATHS_RELEASE} )
    message(${intelliversex-cpp_MESSAGE_MODE} "Conan: Including build module from '${_BUILD_MODULE}'")
    include(${_BUILD_MODULE})
endforeach()


