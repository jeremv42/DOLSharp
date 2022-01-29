cmake_minimum_required(VERSION 3.00)
project(DOL_Detour)

file(GLOB SOURCES Source/*.cpp)
add_library(dol_detour SHARED ${SOURCES})

add_library(RecastNavigation::Detour ALIAS dol_detour)
set_target_properties(dol_detour PROPERTIES DEBUG_POSTFIX -d)

set(dol_detour_INCLUDE_DIR "${CMAKE_CURRENT_SOURCE_DIR}/Include")

target_include_directories(dol_detour PUBLIC
    "$<BUILD_INTERFACE:${dol_detour_INCLUDE_DIR}>"
)

set_target_properties(dol_detour PROPERTIES
        SOVERSION ${SOVERSION}
        VERSION ${LIB_VERSION}
        COMPILE_PDB_OUTPUT_DIRECTORY .
        COMPILE_PDB_NAME "dol_detour-d"
        )

install(TARGETS dol_detour
        RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
        ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
        LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
        COMPONENT library
        )

file(GLOB INCLUDES Include/*.h)
install(FILES ${INCLUDES} DESTINATION
    ${CMAKE_INSTALL_INCLUDEDIR}/recastnavigation)
if(MSVC)
    install(FILES "$<TARGET_FILE_DIR:dol_detour>/dol_detour-d.pdb" CONFIGURATIONS "Debug" DESTINATION "lib")
endif()
