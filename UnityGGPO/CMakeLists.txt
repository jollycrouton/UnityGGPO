cmake_minimum_required(VERSION 3.2)

include_directories("./include/")
include_directories("../ggpo/src/include/")
include_directories("../ggpo/src/lib/ggpo/")

set(STEAMWORKS_PATH "${CMAKE_SOURCE_DIR}/ggpo/external/steamworks/sdk")

# target_include_directories(UnityGGPO PUBLIC
#     ${STEAMWORKS_PATH}/public/steam
# )

file(GLOB_RECURSE SOURCE_FILES "./src/*.cpp" "./src/*.h")

add_library(UnityGGPO MODULE ${SOURCE_FILES})

#add_definitions(-D_UNICODE -DUNICODE)
target_link_libraries(UnityGGPO LINK_PUBLIC GGPO winmm.lib ws2_32.lib "${STEAMWORKS_PATH}/redistributable_bin/win64/steam_api64.lib")

install(TARGETS UnityGGPO DESTINATION "../Unity/Packages/UnityGGPO/Plugins/Windows/x86_64")