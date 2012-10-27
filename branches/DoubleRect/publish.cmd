set msbuild=%SystemRoot%\Microsoft.NET\Framework\v4.0.30319\msbuild.exe
%msbuild% %~dp0sidi-util.msbuild /t:Upload
