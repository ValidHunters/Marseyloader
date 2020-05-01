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

DOTNET_RUNTIME_VERSION = "3.1.3"

DOTNET_RUNTIME_DOWNLOADS = {
    PLATFORM_LINUX: "https://download.visualstudio.microsoft.com/download/pr/c1d419e7-4312-4464-b272-27bee7676560/22e7bb584ff56f3089c85d98b21c0445/dotnet-runtime-3.1.3-linux-x64.tar.gz",
    PLATFORM_WINDOWS: "https://download.visualstudio.microsoft.com/download/pr/f6387d06-5958-4935-ba28-183bb1f8ec7f/a9ccb4d10faec396135e6b967b7037da/dotnet-runtime-3.1.3-win-x64.zip",
    PLATFORM_MACOS: "https://download.visualstudio.microsoft.com/download/pr/6adeeaf9-e591-4a3c-bc34-9cf3b7c60f9b/75826932f66a9afb6f6e2115ded1355b/dotnet-runtime-3.1.3-osx-x64.tar.gz"
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