{
  AnimED - Visual Novel Tools
  Math unit
  Copyright © 2007-2010 WinKiller Studio. Open Source.
  This software is free. Please see License for details.

  Contains *modified* CRC32 calculation function from VPatch2 sources.
  This function can calculate the standard ZIP CRC32 for a stream.
  © 2002-2003 Van de Sande Productions.
}
unit AnimED_Math;

interface

uses Classes;

{ MATH functions }
function FileCRC(fs: TStream): Integer;
function Involution(Base, InvolutionValue: extended) : extended;
function ByteConverter(a, b : integer; operation : byte) : byte;

procedure BlockXOR(InputStream : TStream; value : byte);
procedure BlockXORIO(InputStream, OutputStream : TStream; value : byte);

function HexToInt(InputString : array of char) : int64;

{ ISF-related functions }
function CharToBase36(i : char) : byte;
function Base36ToChar(i : byte) : char;

{ ConvEndian function from ExtractData sources }
function EndianSwap(i : longword) : longword;

const

{ operations for byte converter }
  bcInvert = 0; // Invert
  bcXOR    = 1; // XORing
  bcAND    = 2; // ANDing
  bcOR     = 3; // ORing
  bcSHL    = 4; // Circular bitwise left-shift
  bcSHR    = 5; // Circular bitwise right-shift
  bc256m   = 6; // 256 - value
  bcCM     = 7; // b=b*4 encode
  bcCD     = 8; // b=b*4 decode

{ This is the implementation of base36 table }
  charTable = '0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ';

implementation

{ делает ксор потока в самого себя }
procedure BlockXOR;
var i,j,LastChunk,XorKey : integer;
begin
 // перематываем поток на начало
 InputStream.Position := 0;

 // It's ugly. Really ugly... =_= ...but fast
 xorkey := value shl 24 + value shl 16 + value shl 8 + value;
 // Decryption in progress...
 LastChunk := InputStream.Size mod 4;
 for i := 1 to (InputStream.Size div 4) do begin
  InputStream.Read(j,4);
  InputStream.Position := InputStream.Position - 4;
  j := j xor xorkey;
  InputStream.Write(j,4);
 end;

 if LastChunk > 0 then begin
  InputStream.Read(j,LastChunk);
  InputStream.Position := InputStream.Position - LastChunk;
  j := j xor xorkey;              //  8 16 24 32 40 48 56 64
  InputStream.Write(j,LastChunk); // FF FF FF FF FF FF FF FF
 end;

end;

procedure BlockXORIO;
var i,j,LastChunk,XorKey : integer;
begin
 // перематываем поток на начало
 InputStream.Position := 0;
 OutputStream.Position := 0;

 // It's ugly. Really ugly... =_= ...but fast
 xorkey := value shl 24 + value shl 16 + value shl 8 + value;
 // Decryption in progress...
 LastChunk := InputStream.Size mod 4;
 for i := 1 to (InputStream.Size div 4) do begin
  InputStream.Read(j,4);
  j := j xor xorkey;
  OutputStream.Write(j,4);
 end;

 if LastChunk > 0 then begin
  InputStream.Read(j,LastChunk);
  j := j xor xorkey;               //  8 16 24 32 40 48 56 64
  OutputStream.Write(j,LastChunk); // FF FF FF FF FF FF FF FF
 end;

end;

{ AWESOME(ly stupid) byte converter function }
function ByteConverter(a, b : integer; operation : byte) : byte;
{var i,j : integer; }
begin
 case operation of
  bcInvert: Result := ByteConverter(a,$FF,bcXOR);
  bcXOR:    Result := a xor b;
  bcAND:    Result := a and b;
  bcOR:     Result := a or b;
  bcSHL:    Result := (a shl b) or (a shr (8-b));
  bcSHR:    Result := (a shr b) or (a shl (8-b));
  bc256m:   Result := 256 - a;
  bcCM:     Result := ByteConverter(a,2,bcSHL); {begin a := a * $4; if a > $FF then while a > $FF do a := a - $FF; ByteConverter := a; end;}
  bcCD:     Result := ByteConverter(a,2,bcSHR); {begin for i := 0 to 63 do begin for j := 0 to 3 do begin if a = i*4+j then b := (a+$FF*j) div 4; end; end; ByteConverter := b; end;}
 else Result := 0;
 end;
end;

{ CRC32 calculation function - begin }
function FileCRC(fs: TStream): Integer;
const
 CRCBlock = 4096;
var CRCTable: array[0..255] of LongWord;
    c: LongWord;  //!!! this must be an unsigned 32-bits var!
    Block: array[0..CRCBlock-1] of Byte;
    i,j,bytesread: Integer;
begin
// this used to be the InitCRC procedure
 for i := 0 to 255 do begin
  c := i;
  for j:= 0 to 7 do begin
   if (c and 1) = 0 then c:= (c div 2)
   else c:= (c div 2) xor $EDB88320;
  end;
  CRCTable[i] := c;
 end;
// InitCRC procedure end;
 c:=$FFFFFFFF;
 fs.Seek(0,soFromBeginning);
 for i:=0 to (fs.Size div CRCBlock)+1 do begin
  bytesread:=fs.Read(Block,CRCBlock);
  for j:=0 to bytesread-1 do c := CRCTable[(c and $FF) xor Block[j]] xor (((c and $FFFFFF00) div 256) and $FFFFFF);
 end;
 Result := c xor $FFFFFFFF;
end;
{ CRC32 calculation function - end }

{ HEX to integer custom function - begin }
function HexToInt(InputString : array of char) : int64;
var i : integer; j : int64;
begin
 j := 0;
 for i := 1 to length(InputString) do j := j + CharToBase36(InputString[i]) + 1 shl 4*(length(InputString)-i);
 Result := j;
end;
{ HEX to integer custom function - end }

{ helper macro to convert ('0'..'9','A'..'Z') to (0..35) }
function CharToBase36(i : char) : byte;
begin
 Result := pos(i,CharTable)-1; //-1 here because the pos function returns [1..???]
end;

function Base36ToChar(i : byte) : char;
begin
 Result := CharTable[i+1]; //+1 here because i is in range [0..35]
end;

function Involution(Base, InvolutionValue: extended) : extended;
begin
 Result := exp(InvolutionValue*ln(Base));
end;

{ Endian swapping function }
function EndianSwap(i : longword) : longword;
begin
 asm
  mov eax, i
  bswap eax
  mov i, eax
 end;
 Result := i;
end;

end.