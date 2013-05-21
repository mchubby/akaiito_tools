{
  AnimED - Visual Novel Tools
  RFI Internal AnimED Image Format library
  Copyright © 2007-2010 WinKiller Studio. Open Source.
  This software is free. Please see License for details.
}

unit AG_RFI;

interface

uses AG_Fundamental, Classes, SysUtils;

type
{ Uncompressed only }
 TRFI = packed record
  RealWidth    : word;     // Image width
  RealHeight   : word;     // Image height
  BitDepth     : word;     // Bitdepth -- if > 8 then Palette is not used
  ExtAlpha     : boolean;  // Uses external alpha or not
  Palette      : TPalette; // 4*256
  Valid        : boolean;  // Used as return value of the functions
 end;

 TImageBuf = packed record
  Image : TStream;
  Alpha : TStream;
  ImAttrib : TRFI;
 end;

var
 ImageBuffer : array of TImageBuf;

{ Helper procedure for ImageBuffer initialisation }
procedure RFI_Init(NumOfImages : integer);

{ Helper procedure for RFI cleanup }
procedure RFI_Clear(var RFI : TRFI);

implementation

procedure RFI_Init;
var i : integer;
begin
 SetLength(ImageBuffer,NumOfImages);
 for i := 0 to NumOfImages-1 do begin
  with ImageBuffer[i] do begin
   Image := TMemoryStream.Create;
   Alpha := TMemoryStream.Create;
  end;
 end;
end;

procedure RFI_Clear;
var i : integer;
begin
 FillChar(RFI,SizeOf(RFI),0);
 for i := 0 to length(ImageBuffer)-1 do try
  FillChar(ImageBuffer[i].ImAttrib,SizeOf(ImageBuffer[i].ImAttrib),0);
  FreeAndNil(ImageBuffer[i].Image);
  FreeAndNil(ImageBuffer[i].Alpha);
 except
 end;
 SetLength(ImageBuffer,0);
end;

end.