#!/bin/sh

cd "$(dirname "$0")"

./publish_linux.sh
./publish_osx.sh
./publish_windows.sh