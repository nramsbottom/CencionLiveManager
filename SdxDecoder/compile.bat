

@REM	Author: Neil Ramsbottom
@REM	Date:	12/06/2004
@REM
@REM	Compiles the decoder library C# sources in the current
@REM	directory and all child directories into the assembly.

@REM Compiles the assembly into the current directory


@SET FrameworkDir=%SYSTEMROOT%\Microsoft.NET\Framework
@SET FrameworkVersion=v1.1.4322
@SET csc=%FrameworkDir%\%FrameworkVersion%\csc.exe


%csc% /target:library /out:Decoder.dll /o /nologo /recurse:*.cs
