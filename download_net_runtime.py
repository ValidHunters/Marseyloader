#!/usr/bin/env python3

import os
import sys
import urllib.request
import tarfile
import zipfile
import shutil
from typing import List, Optional

PLATFORM_WINDOWS = "windows"
PLATFORM_LINUX = "linux"
PLATFORM_MACOS = "mac"

DOTNET_RUNTIME_VERSION = "7.0.0"

DOTNET_RUNTIME_DOWNLOADS = {
    PLATFORM_LINUX: "https://download.visualstudio.microsoft.com/download/pr/d4b0a69b-29cd-40ef-9e95-a8d16f0ff346/5844add76ae3917af9efd59d95e3cbd4/dotnet-runtime-7.0.0-linux-x64.tar.gz",
    PLATFORM_WINDOWS: "https://download.visualstudio.microsoft.com/download/pr/243bac9c-0559-4d5e-9ab3-60aa92ef51bd/1bb509033e17301021a458b4214f0931/dotnet-runtime-7.0.0-win-x64.zip",
    PLATFORM_MACOS: "https://download.visualstudio.microsoft.com/download/pr/93aca2a1-570c-4db7-90a1-de89a3a53f27/2d443a62676d498918def9188859f900/dotnet-runtime-7.0.0-osx-x64.tar.gz"
}

p = os.path.join


def main() -> None:
    update_netcore_runtime(sys.argv[1:])


def update_netcore_runtime(platforms: List[str]) -> None:
    runtime_cache = p("Dependencies/dotnet")
    version_file_path = p(runtime_cache, "VERSION")

    # Check if current version is fine.
    current_version: Optional[str]

    try:
        with open(version_file_path, "r") as f:
            current_version = f.read().strip()

    except FileNotFoundError:
        current_version = None

    if current_version != DOTNET_RUNTIME_VERSION and os.path.exists(runtime_cache):
        print("Cached Release .NET Core Runtime out of date/nonexistant, downloading new one..")
        shutil.rmtree(runtime_cache)
    os.makedirs(runtime_cache, exist_ok=True)

    with open(version_file_path, "w") as f:
        f.write(DOTNET_RUNTIME_VERSION)

    # Download missing runtimes if necessary.
    for platform in platforms:
        platform_runtime_cache = p(runtime_cache, platform)
        if not os.path.exists(platform_runtime_cache):
            os.mkdir(platform_runtime_cache)
            download_platform_runtime(platform_runtime_cache, platform)


def download_platform_runtime(dir: str, platform: str) -> None:
    print(f"Downloading .NET Core Runtime for platform {platform}.")
    download_file = p(dir, "download.tmp")
    download_url = DOTNET_RUNTIME_DOWNLOADS[platform]
    urllib.request.urlretrieve(download_url, download_file)

    if download_url.endswith(".tar.gz"):
        # this is a tar gz.
        with tarfile.open(download_file, "r:gz") as tar:
            tar.extractall(dir)
    elif download_url.endswith(".zip"):
        with zipfile.ZipFile(download_file) as zipF:
            zipF.extractall(dir)

    os.remove(download_file)


if __name__ == "__main__":
    main()
