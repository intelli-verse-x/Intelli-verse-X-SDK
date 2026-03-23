# Load the debug and release variables
file(GLOB DATA_FILES "${CMAKE_CURRENT_LIST_DIR}/nakama-sdk-*-data.cmake")

foreach(f ${DATA_FILES})
    include(${f})
endforeach()

# Create the targets for all the components
foreach(_COMPONENT ${nakama-sdk_COMPONENT_NAMES} )
    if(NOT TARGET ${_COMPONENT})
        add_library(${_COMPONENT} INTERFACE IMPORTED)
        message(${nakama-sdk_MESSAGE_MODE} "Conan: Component target declared '${_COMPONENT}'")
    endif()
endforeach()

if(NOT TARGET nakama-sdk::nakama-sdk)
    add_library(nakama-sdk::nakama-sdk INTERFACE IMPORTED)
    message(${nakama-sdk_MESSAGE_MODE} "Conan: Target declared 'nakama-sdk::nakama-sdk'")
endif()
# Load the debug and release library finders
file(GLOB CONFIG_FILES "${CMAKE_CURRENT_LIST_DIR}/nakama-sdk-Target-*.cmake")

foreach(f ${CONFIG_FILES})
    include(${f})
endforeach()