add_executable(ivx_tests test_ivx.cpp)
# Output: single-config (Makefiles/Ninja) -> build/tests/ivx_tests.exe
#         multi-config (Visual Studio) -> build/tests/Release/ivx_tests.exe
# Use default location (build/tests/) so exe is easy to find.
target_link_libraries(ivx_tests PRIVATE intelliversex)

# Copy nakama-sdk.dll next to test exe so it loads at runtime (nakama is shared lib)
if(TARGET nakama-sdk)
  get_target_property(_nakama_dll nakama-sdk IMPORTED_LOCATION)
  if(_nakama_dll AND EXISTS "${_nakama_dll}")
    add_custom_command(TARGET ivx_tests POST_BUILD
      COMMAND ${CMAKE_COMMAND} -E copy_if_different "${_nakama_dll}" "$<TARGET_FILE_DIR:ivx_tests>"
      COMMENT "Copying nakama-sdk.dll to test output directory")
  endif()
  # Also try IMPORTED_IMPLIB's directory for the DLL (same dir as .lib)
  get_target_property(_nakama_implib nakama-sdk IMPORTED_IMPLIB)
  if(NOT _nakama_dll AND _nakama_implib AND EXISTS "${_nakama_implib}")
    get_filename_component(_nakama_libdir "${_nakama_implib}" DIRECTORY)
    set(_nakama_dll_guess "${_nakama_libdir}/nakama-sdk.dll")
    if(EXISTS "${_nakama_dll_guess}")
      add_custom_command(TARGET ivx_tests POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_if_different "${_nakama_dll_guess}" "$<TARGET_FILE_DIR:ivx_tests>"
        COMMENT "Copying nakama-sdk.dll to test output directory")
    endif()
  endif()
endif()
