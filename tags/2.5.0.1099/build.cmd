copy /y %~dp0Sidi.Build\google.code.Targets %~dp0lib\sidi-util\google.code.Targets
set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
%msbuild% %~dp0sidi-util.msbuild /t:%1

