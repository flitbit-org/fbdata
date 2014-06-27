@ECHO OFF
::: -- Prepare the processor --
@SETLOCAL ENABLEEXTENSIONS 
@SETLOCAL ENABLEDELAYEDEXPANSION 

:: -- Version History --
::           Version       YYYYMMDD Author         Description
SET "version=0.0.1"      &:20120729 Phillip Clark  initial version 
SET "version=0.0.2"      &:20131127 Phillip Clark  updated to use specified VS environ, or fallback to the latest version installed
SET "version=0.0.3"      &:20140207 Eli Mumford    updates to reflect a git environment and newer versions of visual studio
SET "title=Build (%~nx0) - %version%"
TITLE %title%

SET "DISPOSITION=DISPOSITION UNKNOWN"
SET "UNIQUE=unknown"
CALL:make_timestamp UNIQUE

IF "%~1" NEQ "" (
	CALL :ParseCommandLineArg "%~1"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~1
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~2" NEQ "" (
	CALL :ParseCommandLineArg "%~2"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~2
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~3" NEQ "" (
	CALL :ParseCommandLineArg "%~3"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~3
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~4" NEQ "" (
	CALL :ParseCommandLineArg "%~4"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~4
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~5" NEQ "" (
	CALL :ParseCommandLineArg "%~5"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~5
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~6" NEQ "" (
	CALL :ParseCommandLineArg "%~6"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~6
	CALL:PrintUsage
	GOTO:EXIT
)
IF "%~7" NEQ "" (
	CALL :ParseCommandLineArg "%~7"	
)
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Invalid argument: %~7
	CALL:PrintUsage
	GOTO:EXIT
)

IF "%CFG%" == "" (
	SET CFG=Debug
)
IF "%PLT%" == "" (
	SET PLT="AnyCPU"
)
IF "%VRB%" == "" (
	SET VRB=minimal
)
IF "%FIL%" == "" (
	SET FIL=detailed
)
IF "%NUG%" == "" (
	SET NUG=false
)
IF "%TGT%" == "" (
	SET TGT=Clean;Build
)
IF "%VSE%" == "" (
	SET VSE=12.0
	SET TLS=12.0
)
IF NOT EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio %VSE%\VC\vcvarsall.bat" (
	SET VSE=11.0
	SET TLS=4.0
)
IF NOT EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio %VSE%\VC\vcvarsall.bat" (
	SET VSE=10.0
	SET TLS=4.0
)
IF NOT EXIST "%ProgramFiles(x86)%\Microsoft Visual Studio %VSE%\VC\vcvarsall.bat" (
	ECHO.probing... "%ProgramFiles(x86)%\Microsoft Visual Studio %VSE%\VC\vcvarsall.bat"
	ECHO.Unable to locate Visual Studio %VSE%
	GOTO:FAIL
)
ECHO.Building using Visual Studio/Tools Version %VSE%
CALL "%ProgramFiles(x86)%\Microsoft Visual Studio %VSE%\VC\vcvarsall.bat"

FOR %%I IN (*.csproj) DO CALL :build_csproj "%%~nxI"	

IF "%DISPOSITION%" == "DISPOSITION UNKNOWN" (
	SET "DISPOSITION=Success!"
	GOTO:EXIT
)
GOTO:EOF

:build_csproj
SET F="%~1"
ECHO.%~p1
ECHO.    %~1
SET "CL=msbuild %F% /m /p:Configuration=%CFG%;Platform=%PLT%;BuildPackage=%NUG% /t:%TGT% /tv:%TLS% /clp:v=%VRB% /flp1:v=%FIL%;logfile=build_%UNIQUE%.log"
%CL%
IF %ERRORLEVEL% NEQ 0 (
	ECHO.Build failed...
	GOTO:FAIL
)
GOTO:EOF

:ParseCommandLineArg -- Parses a command line argument and sets the corresponding variable
::                   -- %~1: the argument
SETLOCAL
SET C=%~1
SET A=%C:~0,3%
SET V=%C:~3%
SET "G="
IF "%A%" == "/p:" (
	SET "G=PLT"
) ELSE IF "%A%"	== "/P:" (
	SET "G=PLT"
) ELSE IF "%A%"	== "/c:" (
	SET "G=CFG"
) ELSE IF "%A%"	== "/C:" (
	SET "G=CFG"
) ELSE IF "%A%"	== "/v:" (
	SET "G=VRB"
) ELSE IF "%A%"	== "/V:" (
	SET "G=VRB"
) ELSE IF "%A%"	== "/e:" (
	SET "G=VSE"
) ELSE IF "%A%"	== "/E:" (
	SET "G=VSE"
) ELSE IF "%A%"	== "/f:" (
	SET "G=FIL"
) ELSE IF "%A%"	== "/F:" (
	SET "G=FIL"
) ELSE IF "%A%"	== "/n:" (
	SET "G=NUG"
) ELSE IF "%A%"	== "/N:" (
	SET "G=NUG"
) ELSE IF "%A%"	== "/t:" (
	SET "G=TGT"
) ELSE IF "%A%"	== "/T:" (
	SET "G=TGT"
) ELSE (
	VERIFY OTHER 2> NUL
)
ENDLOCAL&SET "%G%=%V%"
GOTO:EOF

:make_timestamp -- creates a timestamp and returns it's value in the variable given
::              -- %~1: reference to a variable to hold the timestamp
FOR /f "tokens=2-8 delims=/:. " %%A IN ("%date%:%time: =0%") DO SET "%~1=%%C%%A%%B_%%D%%E%%F%%G"
GOTO:EOF

:PrintUsage
ECHO.
ECHO. The following switch parameters can be used:
ECHO.  '/p:' - Target Platform (i.e. 'Any CPU', 'x86' or 'x64')
ECHO.  '/c:' - Target Configuration (i.e. 'Debug' or 'Release')
ECHO.  '/v:' - Verbosity of console output. The available 
ECHO.		   verbosity levels are: q[uiet], m[inimal],
ECHO.		   n[ormal], d[etailed], and diag[nostic].
ECHO.  '/e:' - Target Visual Studio Environment Version
ECHO.		   Current valid values: 10.0, 11.0, 12.0
ECHO.  '/f:' - Verbosity of log file output. The available 
ECHO.		   verbosity levels are: q[uiet], m[inimal],
ECHO.		   n[ormal], d[etailed], and diag[nostic].
ECHO.  '/n:' - Build NuGet package. Use when building projects 
ECHO.		   that are designed to be a NuGet Package.
ECHO.		   Valid values: true, false
ECHO.  '/t:' - Targets to build (i.e. 'Clean;Build', 'Build')
GOTO:EOF

:FAIL
SET "DISPOSITION=FAILED"
COLOR 47
GOTO:EXIT

:EXIT -- Displays the disposition and exits.
ECHO.
SET /P WAIT_RESULT=Script complete: %DISPOSITION% (enter to continue)
COLOR 7
GOTO:EOF
