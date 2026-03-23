########### AGGREGATED COMPONENTS AND DEPENDENCIES FOR THE MULTI CONFIG #####################
#############################################################################################

set(nakama-sdk_COMPONENT_NAMES "")
if(DEFINED nakama-sdk_FIND_DEPENDENCY_NAMES)
  list(APPEND nakama-sdk_FIND_DEPENDENCY_NAMES )
  list(REMOVE_DUPLICATES nakama-sdk_FIND_DEPENDENCY_NAMES)
else()
  set(nakama-sdk_FIND_DEPENDENCY_NAMES )
endif()

########### VARIABLES #######################################################################
#############################################################################################
set(nakama-sdk_PACKAGE_FOLDER_RELEASE "C:/Users/HP/.conan2/p/b/nakam1508136aeca19/p")
set(nakama-sdk_BUILD_MODULES_PATHS_RELEASE )


set(nakama-sdk_INCLUDE_DIRS_RELEASE "${nakama-sdk_PACKAGE_FOLDER_RELEASE}/include")
set(nakama-sdk_RES_DIRS_RELEASE )
set(nakama-sdk_DEFINITIONS_RELEASE )
set(nakama-sdk_SHARED_LINK_FLAGS_RELEASE )
set(nakama-sdk_EXE_LINK_FLAGS_RELEASE )
set(nakama-sdk_OBJECTS_RELEASE )
set(nakama-sdk_COMPILE_DEFINITIONS_RELEASE )
set(nakama-sdk_COMPILE_OPTIONS_C_RELEASE )
set(nakama-sdk_COMPILE_OPTIONS_CXX_RELEASE )
set(nakama-sdk_LIB_DIRS_RELEASE "${nakama-sdk_PACKAGE_FOLDER_RELEASE}/lib")
set(nakama-sdk_BIN_DIRS_RELEASE )
set(nakama-sdk_LIBRARY_TYPE_RELEASE UNKNOWN)
set(nakama-sdk_IS_HOST_WINDOWS_RELEASE 1)
set(nakama-sdk_LIBS_RELEASE nakama-sdk)
set(nakama-sdk_SYSTEM_LIBS_RELEASE )
set(nakama-sdk_FRAMEWORK_DIRS_RELEASE )
set(nakama-sdk_FRAMEWORKS_RELEASE )
set(nakama-sdk_BUILD_DIRS_RELEASE )
set(nakama-sdk_NO_SONAME_MODE_RELEASE FALSE)


# COMPOUND VARIABLES
set(nakama-sdk_COMPILE_OPTIONS_RELEASE
    "$<$<COMPILE_LANGUAGE:CXX>:${nakama-sdk_COMPILE_OPTIONS_CXX_RELEASE}>"
    "$<$<COMPILE_LANGUAGE:C>:${nakama-sdk_COMPILE_OPTIONS_C_RELEASE}>")
set(nakama-sdk_LINKER_FLAGS_RELEASE
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,SHARED_LIBRARY>:${nakama-sdk_SHARED_LINK_FLAGS_RELEASE}>"
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,MODULE_LIBRARY>:${nakama-sdk_SHARED_LINK_FLAGS_RELEASE}>"
    "$<$<STREQUAL:$<TARGET_PROPERTY:TYPE>,EXECUTABLE>:${nakama-sdk_EXE_LINK_FLAGS_RELEASE}>")


set(nakama-sdk_COMPONENTS_RELEASE )