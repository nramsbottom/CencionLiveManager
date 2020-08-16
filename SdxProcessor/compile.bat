

@REM	Author: Neil Ramsbottom
@REM	Date:	12/06/2004
@REM
@REM	Compiles the console processor application C# sources 
@REM	in the current directory and all child directories into 
@REM	the executable assembly.

@REM
@REM	NOTE: This application requires the decoder library!
@REM	      Make sure that you compile that first or compiling
@REM	      this will fail!
@REM

@SET FrameworkDir=%SYSTEMROOT%\Microsoft.NET\Framework
@SET FrameworkVersion=v1.1.4322
@SET csc=%FrameworkDir%\%FrameworkVersion%\csc.exe

copy ..\Decoder\Decoder.dll /y

%csc% /target:exe /out:ConsoleProcessor.exe /o /nologo /recurse:*.cs /main:Cencion.SwitchDecoder.Applications.ConsoleProcessorApp /reference:Decoder.dll
