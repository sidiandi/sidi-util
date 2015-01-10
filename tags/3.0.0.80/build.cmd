copy /y %~dp0Sidi.Build\google.code.Targets %~dp0lib\sidi-util\google.code.Targets
set msbuild="%ProgramFiles(x86)%\MSBuild\12.0\Bin\msbuild.exe"
%msbuild% %~dp0build.proj /t:%1

