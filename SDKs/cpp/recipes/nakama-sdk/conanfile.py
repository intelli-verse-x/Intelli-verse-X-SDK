# Local Conan recipe for nakama-sdk using Heroic Labs pre-built binaries.
# For local use only (not Conan Center). Build: conan create . nakama-sdk/2.9.0@
# Pre-built assets from https://github.com/heroiclabs/nakama-cpp/releases

import os
import shutil
from conan import ConanFile
from conan.errors import ConanInvalidConfiguration
from conan.tools.files import get, copy


class NakamaSdkConan(ConanFile):
    name = "nakama-sdk"
    version = "2.9.0"
    description = "Nakama C/C++ client SDK (Heroic Labs) - pre-built binaries"
    license = "Apache-2.0"
    url = "https://github.com/heroiclabs/nakama-cpp"
    homepage = "https://heroiclabs.com"
    topics = ("game", "backend", "realtime", "nakama")
    settings = "os", "arch", "build_type"
    no_copy_source = True

    def source(self):
        pass

    def build(self):
        pass

    def _ignore_cmake(self, directory, files):
        return [f for f in files if f.endswith(".cmake")]

    def package(self):
        os_name = str(self.settings.os)
        arch = str(self.settings.arch)
        build_type = str(self.settings.build_type)
        if os_name == "Windows" and ("x86_64" in arch or arch == "x64"):
            asset = "win-x64-MinSizeRel.zip" if build_type == "Release" else "win-x64-Debug.zip"
        elif "Macos" in os_name:
            asset = "macosx-universal-MinSizeRel.zip" if build_type == "Release" else "macosx-universal-Debug.zip"
        elif os_name == "iOS" and ("arm" in arch or "arm64" in arch):
            asset = "ios-arm64-MinSizeRel.zip" if build_type == "Release" else "ios-arm64-Debug.zip"
        else:
            raise ConanInvalidConfiguration(
                f"Pre-built nakama-sdk not available for {os_name}/{arch}. "
                "Use vcpkg or build from source."
            )
        url = f"https://github.com/heroiclabs/nakama-cpp/releases/download/v{self.version}/{asset}"
        download_folder = os.path.join(self.build_folder, "downloads")
        os.makedirs(download_folder, exist_ok=True)
        get(self, url, destination=download_folder, filename=asset)
        # get() extracts the zip into destination; contents may be in one subdir
        pkg = self.package_folder
        entries = os.listdir(download_folder)
        if len(entries) == 1 and os.path.isdir(os.path.join(download_folder, entries[0])):
            root = os.path.join(download_folder, entries[0])
            for name in os.listdir(root):
                s = os.path.join(root, name)
                d = os.path.join(pkg, name)
                if os.path.isfile(s):
                    copy(self, name, src=root, dst=pkg)
                else:
                    shutil.copytree(s, d, dirs_exist_ok=True, ignore=self._ignore_cmake)
        else:
            for name in entries:
                s = os.path.join(download_folder, name)
                d = os.path.join(pkg, name)
                if os.path.isfile(s):
                    copy(self, name, src=download_folder, dst=pkg)
                else:
                    shutil.copytree(s, d, dirs_exist_ok=True, ignore=self._ignore_cmake)

    def package_info(self):
        self.cpp_info.includedirs = ["include"]
        self.cpp_info.libdirs = ["lib"]
        self.cpp_info.bindirs = ["bin"]
        self.cpp_info.libs = ["nakama-sdk"]
        self.cpp_info.set_property("cmake_target_name", "nakama-sdk::nakama-sdk")
        self.cpp_info.names["cmake_find_package"] = "nakama-sdk"
        self.cpp_info.names["cmake_find_package_multi"] = "nakama-sdk"
