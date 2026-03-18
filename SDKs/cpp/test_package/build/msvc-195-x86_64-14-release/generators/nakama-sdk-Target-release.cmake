# Avoid multiple calls to find_package to append duplicated properties to the targets
include_guard()########### VARIABLES #######################################################################
#############################################################################################
set(nakama-sdk_FRAMEWORKS_FOUND_RELEASE "") # Will be filled later
conan_find_apple_frameworks(nakama-sdk_FRAMEWORKS_FOUND_RELEASE "${nakama-sdk_FRAMEWORKS_RELEASE}" "${nakama-sdk_FRAMEWORK_DIRS_RELEASE}")

set(nakama-sdk_LIBRARIES_TARGETS "") # Will be filled later


######## Create an interface target to contain all the dependencies (frameworks, system and conan deps)
if(NOT TARGET nakama-sdk_DEPS_TARGET)
    add_library(nakama-sdk_DEPS_TARGET INTERFACE IMPORTED)
endif()

set_property(TARGET nakama-sdk_DEPS_TARGET
             APPEND PROPERTY INTERFACE_LINK_LIBRARIES
             $<$<CONFIG:Release>:${nakama-sdk_FRAMEWORKS_FOUND_RELEASE}>
             $<$<CONFIG:Release>:${nakama-sdk_SYSTEM_LIBS_RELEASE}>
             $<$<CONFIG:Release>:>)

####### Find the libraries declared in cpp_info.libs, create an IMPORTED target for each one and link the
####### nakama-sdk_DEPS_TARGET to all of them
conan_package_library_targets("${nakama-sdk_LIBS_RELEASE}"    # libraries
                              "${nakama-sdk_LIB_DIRS_RELEASE}" # package_libdir
                              "${nakama-sdk_BIN_DIRS_RELEASE}" # package_bindir
                              "${nakama-sdk_LIBRARY_TYPE_RELEASE}"
                              "${nakama-sdk_IS_HOST_WINDOWS_RELEASE}"
                              nakama-sdk_DEPS_TARGET
                              nakama-sdk_LIBRARIES_TARGETS  # out_libraries_targets
                              "_RELEASE"
                              "nakama-sdk"    # package_name
                              "${nakama-sdk_NO_SONAME_MODE_RELEASE}")  # soname

# FIXME: What is the result of this for multi-config? All configs adding themselves to path?
set(CMAKE_MODULE_PATH ${nakama-sdk_BUILD_DIRS_RELEASE} ${CMAKE_MODULE_PATH})

########## GLOBAL TARGET PROPERTIES Release ########################################
    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_LINK_LIBRARIES
                 $<$<CONFIG:Release>:${nakama-sdk_OBJECTS_RELEASE}>
                 $<$<CONFIG:Release>:${nakama-sdk_LIBRARIES_TARGETS}>
                 )

    if("${nakama-sdk_LIBS_RELEASE}" STREQUAL "")
        # If the package is not declaring any "cpp_info.libs" the package deps, system libs,
        # frameworks etc are not linked to the imported targets and we need to do it to the
        # global target
        set_property(TARGET nakama-sdk::nakama-sdk
                     APPEND PROPERTY INTERFACE_LINK_LIBRARIES
                     nakama-sdk_DEPS_TARGET)
    endif()

    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_LINK_OPTIONS
                 $<$<CONFIG:Release>:${nakama-sdk_LINKER_FLAGS_RELEASE}>)
    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_INCLUDE_DIRECTORIES
                 $<$<CONFIG:Release>:${nakama-sdk_INCLUDE_DIRS_RELEASE}>)
    # Necessary to find LINK shared libraries in Linux
    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_LINK_DIRECTORIES
                 $<$<CONFIG:Release>:${nakama-sdk_LIB_DIRS_RELEASE}>)
    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_COMPILE_DEFINITIONS
                 $<$<CONFIG:Release>:${nakama-sdk_COMPILE_DEFINITIONS_RELEASE}>)
    set_property(TARGET nakama-sdk::nakama-sdk
                 APPEND PROPERTY INTERFACE_COMPILE_OPTIONS
                 $<$<CONFIG:Release>:${nakama-sdk_COMPILE_OPTIONS_RELEASE}>)

########## For the modules (FindXXX)
set(nakama-sdk_LIBRARIES_RELEASE nakama-sdk::nakama-sdk)
