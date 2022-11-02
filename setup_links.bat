@echo off
set /p rpath="Path to Rimworld: "
REM set /p hpath="Path to HugsLib: "

mkdir "..\Portal_Gun - Release"
mkdir "..\Portal_Gun - Debug"
mklink /J "%rpath%\Mods\Portal_Gun - Release" "..\Portal_Gun - Release"
mklink /J "%rpath%\Mods\Portal_Gun - Debug" "..\Portal_Gun - Debug"
REM mklink /J "RimWorldAssemblies\" "%rpath%\RimWorldWin64_Data\Managed"
REM mklink /J "HugsLibsAssemblies\" "%hpath%\Assemblies"