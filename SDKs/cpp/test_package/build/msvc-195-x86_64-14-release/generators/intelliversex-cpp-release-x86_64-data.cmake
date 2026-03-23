########### AGGREGATED COMPONENTS AND DEPENDENCIES FOR THE MULTI CONFIG #####################
#############################################################################################

set(intelliversex-cpp_COMPONENT_NAMES "")
if(DEFINED intelliversex-cpp_FIND_DEPENDENCY_NAMES)
  list(APPEND intelliversex-cpp_FIND_DEPENDENCY_NAMES nakama-sdk)
  list(REMOVE_DUPLICATES intelliversex-cpp_FIND_DEPENDENCY_NAMES)
else()
  set(intelliversex-cpp_FIND_DEPENDENCY_NAMES nakama-sdk)
endif()
set(nakama-sdk_FIND_MODE "NO_MODULE")

########### VARIABLES #######################################################################
#############################################################################################
set(intelliversex-cpp_PACKAGE_FOLDER_RELEASE "C:/Users/HP/.conan2/p/b/intel0b904e62680de/p")
set(intelliversex-cpp_BUILD_MODULES_PATHS_RELEASE )


set(intelliversex-cpp_INCLUDE_DIRS_RELEASE "${intelliversex-cpp_PACKAGE_FOLDER_RELEASE}/include")
set(intelliversex-cpp_RES_DIRS_RELEASE )
set(intelliversex-cpp_DEFINITIONS_RELEASE )
set(intelliversex-cpp_SHARED_LINK_FLAGS_RELEASE )
set(intelliversex-cpp_EXE_LINK_FLAGS_RELEASE )
set(intelliversex-cpp_OBJECTS_RELEASE )
set(intelliversex-cpp_COMPILE_DEFINITIONS_RELEASE )
set(intelliversex-cpp_COMPILE_OPTIONS_C_RELEASE )
set(intelliversex-cpp_COMPILE_OPTIONS_CXX_RELEASE )
set(intelliversex-cpp_LIB_DIRS_RELEASE "${intelliversex-cpp_PACKAGE_FOLDER_RELEASE}/lib")
set(intelliversex-cpp_BIN_DIRS_RELEASE )
set(intelliversex-cpp_LIBRARY_TYPE_RELEASE STATIC)
set(intelliversex-cpp_IS_HOST_WINDOWS_RELEASE 1)
set(intelliversex-cpp_LIBS_RELEASE intelliversex)
set(intelliversex-cpp_SYSTEM_LIBS_RELEASE )
set(intelliversex-cpp_FRAMEWORK_DIRS_RELEASE )
set(intelliversex-cpp_FRAMEWORKS_RELEASE )
set(intelliversex-cpp_BUILD_DIRS_RELEASE )
set(intelliversex-cpp_NO_SONAME_MODE_RELEASE FALSE)


# COMPOUND VARIABLES
set(intelliversex-cpp_COMPILE_OPTIONS_RELEASE
    "$<$<COMPILE_LANGUAGE:CXX>:${intelliversex-cpp_COMPILE_OPTIONS_CXX_RELEASE}>"
    "$<$<COMPILE_LANGUAGE:C>:${intelliversex-cpp_COMPILE_OPTIONS_C_RELEASE}>")
set(intelliversex-cpp_LINKER_FLAGS_RELEASE
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,SHARED_LIBRARY>:${intelliversex-cpp_SHARED_LINK_FLAGS_RELEASE}>"
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,MODULE_LIBRARY>:${intelliversex-cpp_SHARED_LINK_FLAGS_RELEASE}>"
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,EXECUTABLE>:${intelliversex-cpp_EXE_LINK_FLAGS_RELEASE}>")


set(intelliversex-cpp_COMPONENTS_RELEASE )