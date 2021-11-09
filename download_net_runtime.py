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

DOTNET_RUNTIME_VERSION = "6.0.0"

DOTNET_RUNTIME_DOWNLOADS = {
    PLATFORM_LINUX: "https://download.visualstudio.microsoft.com/download/pr/0ce1c34f-0d9e-4d9b-964e-da676c8e605a/7a6c353b36477fa84f85b2821f2350c2/dotnet-runtime-6.0.0-linux-x64.tar.gz",
    PLATFORM_WINDOWS: "https://download.visualstudio.microsoft.com/download/pr/6b96c97d-9b8c-4141-a32a-5848d3369dbf/9972321cb7af5938fecdee2d8ebd72bb/dotnet-runtime-6.0.0-win-x64.zip",
    PLATFORM_MACOS: "https://download.visualstudio.microsoft.com/download/pr/d88f74a5-05d2-46cb-886a-a62fd698009d/67f5f05e9c029d284c309f0f712fc99f/dotnet-runtime-6.0.0-osx-x64.tar.gz"
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
