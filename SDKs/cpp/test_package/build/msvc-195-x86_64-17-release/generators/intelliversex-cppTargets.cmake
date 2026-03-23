# Load the debug and release variables
file(GLOB DATA_FILES "${CMAKE_CURRENT_LIST_DIR}/intelliversex-cpp-*-data.cmake")

foreach(f ${DATA_FILES})
    include(${f})
endforeach()

# Create the targets for all the components
foreach(_COMPONENT ${intelliversex-cpp_COMPONENT_NAMES} )
    if(NOT TARGET ${_COMPONENT})
        add_library(${_COMPONENT} INTERFACE IMPORTED)
        message(${intelliversex-cpp_MESSAGE_MODE} "Conan: Component target declared '${_COMPONENT}'")
    endif()
endforeach()

if(NOT TARGET intelliversex)
    add_library(intelliversex INTERFACE IMPORTED)
    message(${intelliversex-cpp_MESSAGE_MODE} "Conan: Target declared 'intelliversex'")
endif()
# Load the debug and release library finders
file(GLOB CONFIG_FILES "${CMAKE_CURRENT_LIST_DIR}/intelliversex-cpp-Target-*.cmake")

foreach(f ${CONFIG_FILES})
    include(${f})
endforeach()