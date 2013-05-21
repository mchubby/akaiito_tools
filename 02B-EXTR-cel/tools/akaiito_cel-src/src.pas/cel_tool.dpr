{
  Program module
  Copyright © 2009 WinKiller Studio. Open Source.
  Based on code by Nik.
  This software is free. Please see License for details.
}
program cel_tool;

{$APPTYPE CONSOLE}
{$R *.res}

uses SysUtils, Classes, cel_core;

procedure Help;
var Help : array of string; i : integer;
begin
 SetLength(Help,12);
 Help[ 0] := 'Aoi Shiro CEL Image Decompiler\Compiler [2009/07/20] ALPHA';
 Help[ 1] := 'Incorporates CEL format description and LZSS decoder by Nik.';
 Help[ 2] := 'Copyright (c) 2009 WinKiller Studio. Open Source.';
 Help[ 3] := 'This software is free. Please see License for details.';
 Help[ 4] := '';
 Help[ 5] := 'Usage:';
 Help[ 6] := '';
 Help[ 7] := ExtractFileName(paramstr(0))+' input [dir]';
 Help[ 8] := '';
 Help[ 9] := 'input - input CEL or INI filename';
 Help[10] := 'dir   - path for output file(s). If skipped, input file''s path will be used.';
 Help[11] := '';
 for i := 0 to 11 do writeln(Help[i]);
end;

var FileExt : string;

begin
 if paramstr(1) = '' then begin
  Help;
  writeln('Press Enter to quit.');
  readln;
 end else try
  FileExt := ExtractFileExt(paramstr(1));
  if FileExt = '.cel' then Export_CEL(paramstr(1));
  if FileExt = '.ini' then Import_CEL(paramstr(1));
 except
  writeln('An error has accured while trying to export or import CEL file.');
  writeln('Press Enter to quit.');
  readln;
 end;
end.
