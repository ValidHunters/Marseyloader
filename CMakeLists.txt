cmake_minimum_required(VERSION 3.14)
project(ss14launcherext)

set(CMAKE_CXX_STANDARD 11)

set(CMAKE_AUTOMOC ON)
set(CMAKE_AUTOUIC ON)
set(CMAKE_AUTORCC ON)

find_package(Qt5Core REQUIRED)
find_package(Qt5Quick REQUIRED)

add_library(ss14launcherext SHARED src/main.cpp)

target_link_libraries(ss14launcherext Qt5::Core)
target_link_libraries(ss14launcherext Qt5::Quick)