# Conan 2 recipe for IntelliVerseX C++ SDK.
# Use from SDKs/cpp: conan create . --version=1.5.0
# Requires nakama-sdk in your Conan cache (not in Conan Center; see docs/conan-recipe-tasks.md).

from conan import ConanFile
from conan.tools.cmake import CMake, CMakeToolchain, CMakeDeps, cmake_layout
from conan.tools.files import copy
import os


class IntelliversexCppConan(ConanFile):
    name = "intelliversex-cpp"
    version = "1.5.0"
    description = "IntelliVerseX C/C++ SDK — Auth, Backend (Nakama), Analytics, Social, Monetization for game development"
    license = "MIT"
    url = "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK"
    homepage = "https://github.com/Intelli-verse-X/Intelli-verse-X-Unity-SDK"
    topics = ("game", "sdk", "nakama", "backend", "auth")
    settings = "os", "compiler", "build_type", "arch"
    options = {"shared": [True, False], "fPIC": [True, False]}
    default_options = {"shared": False, "fPIC": True}
    requires = "nakama-sdk/2.9.0"
    exports_sources = "CMakeLists.txt", "src/*", "include/*"

    def config_options(self):
        if self.settings.os == "Windows":
            del self.options.fPIC

    def layout(self):
        self.folders.source = "."
        self.folders.build = "build"
        self.folders.generators = os.path.join(self.folders.build, "generators")

    def generate(self):
        deps = CMakeDeps(self)
        deps.generate()
        tc = CMakeToolchain(self)
        tc.generate()

    def build(self):
        cmake = CMake(self)
        cmake.configure(
            variables={
                "IVX_BUILD_TESTS": "OFF",
                "IVX_BUILD_EXAMPLES": "OFF",
                "IVX_BUILD_SHARED": "ON" if self.options.get_safe("shared", False) else "OFF",
            }
        )
        cmake.build()

    def package(self):
        cmake = CMake(self)
        cmake.install()
        # LICENSE is at repo root; for Conan Center recipe copy from source()
        license_src = os.path.join(self.recipe_folder, "..", "..", "LICENSE")
        if os.path.isfile(license_src):
            copy(self, "LICENSE", src=os.path.dirname(license_src), dst=os.path.join(self.package_folder, "licenses"))

    def package_info(self):
        self.cpp_info.libs = ["intelliversex"]
        self.cpp_info.set_property("cmake_target_name", "intelliversex")
        self.cpp_info.names["cmake_find_package"] = "intelliversex"
        self.cpp_info.names["cmake_find_package_multi"] = "intelliversex"
