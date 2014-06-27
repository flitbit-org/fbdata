@ECHO OFF
:: -- Version History --
::           Version       YYYYMMDD Author         Description
SET "version=0.0.1"      &:20120729 Phillip Clark  initial version 
SET "version=0.0.2"      &:20131127 Phillip Clark  updated to use specified VS environ, or fallback to the latest version installed
SET "version=0.0.3"      &:20140207 Eli Mumford    changed to defer and provide defaults to build.cmd
SET "title=Build (%~nx0) - %version%"
TITLE %title%

SET CFG=Release
SET NUG=true
CALL build.cmd %*