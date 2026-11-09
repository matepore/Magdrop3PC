cmake_minimum_required(VERSION 3.21)
project(magdrop3 VERSION 0.1.0 LANGUAGES C)

set(CMAKE_C_STANDARD 11)
set(CMAKE_C_STANDARD_REQUIRED ON)

# Select the system BIOS used by this regional build.
set(MAGDROP3_BIOS_FILE "sp-s2.sp1" CACHE STRING
    "Neo Geo BIOS filename used by this build")

# Use the toolkit checkout beside this project.
set(NEOGEORECOMP_DIR
    "${CMAKE_CURRENT_SOURCE_DIR}/../neogeorecomp"
    CACHE PATH "neogeorecomp checkout")

add_subdirectory("${NEOGEORECOMP_DIR}" neogeorecomp_build)

target_compile_definitions(neogeorecomp PRIVATE
    "NG_DEFAULT_BIOS_NAME=\"${MAGDROP3_BIOS_FILE}\"")

# Leave empty for the initial interpreter-only build.
set(MAGDROP3_ROM_DIR "" CACHE PATH
    "Directory containing the Magical Drop III ROM files")

set(MAGDROP3_BIOS_DIR "" CACHE PATH
    "Directory containing the Neo Geo system ROM files")

set(MAGDROP3_ENTRIES "${CMAKE_BINARY_DIR}/entries.txt"
    CACHE FILEPATH "Additional entry points discovered during profiling")

# Generator: builds the tool that will create native C code later.
add_executable(magdrop3_recomp
    src/recomp_main.c
    src/game.c
)
target_link_libraries(magdrop3_recomp PRIVATE m68krecomp_core)
target_include_directories(magdrop3_recomp PRIVATE src)

# Player: initially uses the interpreter.
set(GAME_SOURCES src/main.c)
set(GEN_DIR "${CMAKE_BINARY_DIR}/generated")
set(GEN_FILES 16)

if(MAGDROP3_ROM_DIR)
    set(GEN_SOURCES "${GEN_DIR}/recomp_table.c")

    math(EXPR LAST_GEN_FILE "${GEN_FILES} - 1")
    foreach(i RANGE ${LAST_GEN_FILE})
        if(i LESS 10)
            list(APPEND GEN_SOURCES "${GEN_DIR}/recomp_0${i}.c")
        else()
            list(APPEND GEN_SOURCES "${GEN_DIR}/recomp_${i}.c")
        endif()
    endforeach()

    if(NOT EXISTS "${MAGDROP3_ENTRIES}")
        file(WRITE "${MAGDROP3_ENTRIES}" "")
    endif()

    file(MAKE_DIRECTORY "${GEN_DIR}")

    add_custom_command(
        OUTPUT ${GEN_SOURCES} "${GEN_DIR}/recomp_common.h"
        COMMAND magdrop3_recomp
            --rom-path "${MAGDROP3_ROM_DIR}"
            --bios-path "${MAGDROP3_BIOS_DIR}"
            --bios "${MAGDROP3_BIOS_FILE}"
            --out "${GEN_DIR}"
            --files ${GEN_FILES}
            --entries "${MAGDROP3_ENTRIES}"
        DEPENDS magdrop3_recomp "${MAGDROP3_ENTRIES}"
        COMMENT "Recompiling Magical Drop III"
        VERBATIM
    )

    list(APPEND GAME_SOURCES ${GEN_SOURCES})
endif()

add_executable(magdrop3 ${GAME_SOURCES} src/game.c)
target_link_libraries(magdrop3 PRIVATE neogeorecomp)
target_include_directories(magdrop3 PRIVATE src "${GEN_DIR}")

if(MAGDROP3_ROM_DIR)
    target_compile_definitions(magdrop3 PRIVATE MAGDROP3_HAVE_RECOMP)

    if(MSVC)
        set_source_files_properties(
            ${GEN_SOURCES}
            PROPERTIES COMPILE_OPTIONS "/bigobj"
        )
    endif()
endif()

if(MSVC)
    target_compile_definitions(magdrop3 PRIVATE _CRT_SECURE_NO_WARNINGS)
    target_compile_definitions(magdrop3_recomp PRIVATE _CRT_SECURE_NO_WARNINGS)
endif()

# Prepare the standalone runtime layout beside the executable.
add_custom_command(TARGET magdrop3 POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E make_directory
        "$<TARGET_FILE_DIR:magdrop3>/rom"
        "$<TARGET_FILE_DIR:magdrop3>/bios"
)

# Copy SDL2 if the optional windowed runtime was built.
if(WIN32 AND TARGET SDL2::SDL2)
    add_custom_command(TARGET magdrop3 POST_BUILD
        COMMAND ${CMAKE_COMMAND} -E copy_if_different
            $<TARGET_RUNTIME_DLLS:magdrop3>
            $<TARGET_FILE_DIR:magdrop3>
        COMMAND_EXPAND_LISTS
    )
endif()
