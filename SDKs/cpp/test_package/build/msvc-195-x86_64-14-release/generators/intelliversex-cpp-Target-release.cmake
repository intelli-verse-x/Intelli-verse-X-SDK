# Avoid multiple calls to find_package to append duplicated properties to the targets
include_guard()########### VARIABLES #######################################################################
#############################################################################################
set(intelliversex-cpp_FRAMEWORKS_FOUND_RELEASE "") # Will be filled later
conan_find_apple_frameworks(intelliversex-cpp_FRAMEWORKS_FOUND_RELEASE "${intelliversex-cpp_FRAMEWORKS_RELEASE}" "${intelliversex-cpp_FRAMEWORK_DIRS_RELEASE}")

set(intelliversex-cpp_LIBRARIES_TARGETS "") # Will be filled later


######## Create an interface target to contain all the dependencies (frameworks, system and conan deps)
if(NOT TARGET intelliversex-cpp_DEPS_TARGET)
    add_library(intelliversex-cpp_DEPS_TARGET INTERFACE IMPORTED)
endif()

set_property(TARGET intelliversex-cpp_DEPS_TARGET
             APPEND PROPERTY INTERFACE_LINK_LIBRARIES
             $<$<CONFIG:Release>:${intelliversex-cpp_FRAMEWORKS_FOUND_RELEASE}>
             $<$<CONFIG:Release>:${intelliversex-cpp_SYSTEM_LIBS_RELEASE}>
             $<$<CONFIG:Release>:nakama-sdk::nakama-sdk>)

####### Find the libraries declared in cpp_info.libs, create an IMPORTED target for each one and link the
####### intelliversex-cpp_DEPS_TARGET to all of them
conan_package_library_targets("${intelliversex-cpp_LIBS_RELEASE}"    # libraries
                              "${intelliversex-cpp_LIB_DIRS_RELEASE}" # package_libdir
                              "${intelliversex-cpp_BIN_DIRS_RELEASE}" # package_bindir
                              "${intelliversex-cpp_LIBRARY_TYPE_RELEASE}"
                              "${intelliversex-cpp_IS_HOST_WINDOWS_RELEASE}"
                              intelliversex-cpp_DEPS_TARGET
                              intelliversex-cpp_LIBRARIES_TARGETS  # out_libraries_targets
                              "_RELEASE"
                              "intelliversex-cpp"    # package_name
                              "${intelliversex-cpp_NO_SONAME_MODE_RELEASE}")  # soname

# FIXME: What is the result of this for multi-config? All configs adding themselves to path?
set(CMAKE_MODULE_PATH ${intelliversex-cpp_BUILD_DIRS_RELEASE} ${CMAKE_MODULE_PATH})

########## GLOBAL TARGET PROPERTIES Release ########################################
    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_LINK_LIBRARIES
                 $<$<CONFIG:Release>:${intelliversex-cpp_OBJECTS_RELEASE}>
                 $<$<CONFIG:Release>:${intelliversex-cpp_LIBRARIES_TARGETS}>
                 )

    if("${intelliversex-cpp_LIBS_RELEASE}" STREQUAL "")
        # If the package is not declaring any "cpp_info.libs" the package deps, system libs,
        # frameworks etc are not linked to the imported targets and we need to do it to the
        # global target
        set_property(TARGET intelliversex
                     APPEND PROPERTY INTERFACE_LINK_LIBRARIES
                     intelliversex-cpp_DEPS_TARGET)
    endif()

    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_LINK_OPTIONS
                 $<$<CONFIG:Release>:${intelliversex-cpp_LINKER_FLAGS_RELEASE}>)
    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_INCLUDE_DIRECTORIES
                 $<$<CONFIG:Release>:${intelliversex-cpp_INCLUDE_DIRS_RELEASE}>)
    # Necessary to find LINK shared libraries in Linux
    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_LINK_DIRECTORIES
                 $<$<CONFIG:Release>:${intelliversex-cpp_LIB_DIRS_RELEASE}>)
    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_COMPILE_DEFINITIONS
                 $<$<CONFIG:Release>:${intelliversex-cpp_COMPILE_DEFINITIONS_RELEASE}>)
    set_property(TARGET intelliversex
                 APPEND PROPERTY INTERFACE_COMPILE_OPTIONS
                 $<$<CONFIG:Release>:${intelliversex-cpp_COMPILE_OPTIONS_RELEASE}>)

########## For the modules (FindXXX)
set(intelliversex-cpp_LIBRARIES_RELEASE intelliversex)
