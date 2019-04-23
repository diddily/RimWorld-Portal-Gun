@echo off
set /p rpath="Path to Rimworld: "
set /p hpath="Path to HugsLib: "

mkdir "..\Portal_Gun - Release"
mkdir "..\Portal_Gun - Debug"
mklink /J "%rpath%\Mods\Portal_Gun - Release" "..\Portal_Gun - Release"
mklink /J "%rpath%\Mods\Portal_Gun - Debug" "..\Portal_Gun - Debug"
mklink /J "RimWorldAssemblies\" "%rpath%\RimWorldWin64_Data\Managed"
mklink /J "HugsLibsAssemblies\" "%hpath%\Assemblies"