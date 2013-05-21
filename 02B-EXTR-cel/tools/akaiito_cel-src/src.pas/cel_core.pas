unit cel_core;

interface

uses Classes, Sysutils, IniFiles,
     Generic_LZSS,
     AnimED_Math,
     AG_Fundamental,
     AG_RFI;

type
 TCEL_CP10 = packed record
  Header          : array[1..4] of char; // CP10
  Dummy1          : longword; // 0x00000000
  ENTRQty         : word;     // count of ENTR entries
  ANIMQty         : word;     // count of ANIM entries
  IMAGQty         : word;     // count of IMAG entries
  Dummy2          : word;     // 0x0000
  Length          : longword; // Length of cel - 0x18 (size of CP10)
  Unknown         : longword;
 end;

 TCEL_ENTR = packed record
  Header          : array[1..4] of char; // ENTR
  OffsetNextEntry : longword;
  Number1         : longword;
  Number2         : longword; // Number1 xor $FFFF0000
  Unk1            : longword;
  xShift          : word;
  yShift          : word;
  xStretch        : longword; // use only small word
  yStretch        : longword; // use only small word
  Dummy5          : longword; // 0x0000
  Dummy6          : longword; // 0xFFFFFFFF
  Unk2            : longword; // 0xBF000000 / 0x3F000000
 end;

 TCEL_ANIM_INIT = packed record
  Header          : array[1..4] of char; // ANIM
  OffsetNextEntry : longword;
  Number          : longword;
  Unknown         : longword; // 0x00000000
  NumberOfActions : longword;
 end;

 TCEL_ANIM_ACTION = packed record
  Action          : word;
  Value           : word;
 end;

 TCEL_IMAG = packed record
  Header          : array[1..4] of char; // IMAG
  OffsetNextEntry : longword;
  Number1         : longword; // порядковый номер изображения, начиная с нуля
  XDimension      : word;     // Width
  YDimension      : word;     // Height
  BitDepthFlag    : longword; // 7 - 32, 5 - 8+palette
  unk2            : longword; // 0x00000000
  CompFlag        : longword; // compression flag (1 = compressed, 0 = uncompressed)
  ImageLength     : longword;
 end;

 TCEL_LZ = packed record
  Header          : array[1..2] of char; // LZ
  UnpackedLength  : longword; // Big-endian
  Packedlength    : longword; // Big-endian
 end;

 TCEL_ENDC = packed record
  Header          : array[1..4] of char; // ENDC
  celLength       : longword;
 end;

{ Original Targa (required things only) }
 TTGA = packed record
  IDLength         : byte; // ID field length (usually 0)
{ Little notice: if Paletted = 0, but ImageType = 1, we're dealing with
  UNMAPPED GRAYSCALE IMAGE - it DOESN'T HAVE PALETTE AT ALL!! }
  Paletted         : byte; // Uses palette (True) or not (False)
  ImageType        : byte; // Image type (0 - dummy, 1 - paletted, 2 - true-color, 3 - b&w, 9,10,11 - RLE (not supported)
  PaletteIndex     : word; // Beginning position of the palette (always 0)
  PaletteLength    : word; // Number of palette entries (1 - 2; 4 - 16; 8 - 256; 16,24,32 - 0)
  PaletteEntrySize : byte; // Size of palette entry in BITS (usually 24)
  X                : word; // X render coordinate
  Y                : word; // Y render coordinate
  Width            : word; // Image width
  Height           : word; // Image height
  BitDepth         : byte; // Bits per pixel
  ImageDescriptor  : byte; // Not used, always 0
 end;

{ Aoi Shiro CEL LZ decoding function }
function CEL_LZDecoder(IStream, OStream : TStream; CryptLength : integer) : boolean;
function CEL_LZEncoder(IStream, OStream : TStream) : boolean;

function Export_CEL(FileName : string) : boolean;
function Import_CEL(FileName : string) : boolean;

{ Opens\Generates TrueVision Targa-compatible files }
function Import_TGA(InputStream, OutputStream : TStream; OutputStreamA : TStream = nil) : TRFI;
function Export_TGA(RFI : TRFI; OutputStream, InputStream : TStream; InputStreamA : TStream = nil) : boolean;

implementation

{ Формат CEL использует довольно мудрёный способ представления данных. }
function Export_CEL;
var i,j : integer;
    CEL_Header   : TCEL_CP10;
    CEL_ENTR     : array of TCEL_ENTR;
    CEL_ANIM     : array of TCEL_ANIM_INIT;
    CEL_ANIMA    : array of array of TCEL_ANIM_ACTION;
    CEL_IMAG     : array of TCEL_IMAG;
    CEL_LZ       : TCEL_LZ;
    CEL_ENDC     : TCEL_ENDC;
    TempoStream  : TStream;
    TempoStream2 : TStream;
    InputStream  : TStream;
    OutputStream : TStream;
    Ini          : TINIFile;
    HeadString   : string;
    RFI : TRFI;
label StopThis;
begin
 Result := False;
 InputStream := TFileStream.Create(FileName,fmOpenRead);
 Ini := TINIFile.Create(ChangeFileExt(FileName,'.ini'));
 TempoStream := TMemoryStream.Create;
 with CEL_Header, InputStream do begin
  Seek(0,soBeginning);
  Read(CEL_Header,SizeOf(CEL_Header));
  if Header <> 'CP10' then goto StopThis;

  SetLength(CEL_ENTR,ENTRQty);
  SetLength(CEL_ANIM,ANIMQty);
  SetLength(CEL_ANIMA,ANIMQty);
  SetLength(CEL_IMAG,IMAGQty);

/// INI ///
  with INI do begin
   WriteInteger(Header,'Dummy1',Dummy1);
   WriteInteger(Header,'ENTRQty',ENTRQty);
   WriteInteger(Header,'ANIMQty',ANIMQty);
   WriteInteger(Header,'IMAGQty',IMAGQty);
   WriteInteger(Header,'Unknown',Unknown);
  end;

/////////////////////////////////////////////////////////////////////////////////////////
  RFI_Init(IMAGQty); // Устанавливаем кол-во слотов в буфере для изображений
/////////////////////////////////////////////////////////////////////////////////////////

  for i := 0 to ENTRQty-1 do begin
   Read(CEL_ENTR[i],SizeOf(CEL_ENTR[i]));
   Seek(CEL_ENTR[i].OffsetNextEntry,soBeginning);

/// INI ///
   with INI, CEL_ENTR[i] do begin
    HeadString := Header+'_'+inttostr(i);

//    WriteInteger(HeadString,'OffsetNextEntry',OffsetNextEntry);
    WriteInteger(HeadString,'Number1',Number1);
//    WriteInteger(HeadString,'Number2',Number2);
    WriteInteger(HeadString,'Unk1',Unk1);
    WriteInteger(HeadString,'xShift',xShift);
    WriteInteger(HeadString,'yShift',yShift);
    WriteInteger(HeadString,'xStretch',xStretch);
    WriteInteger(HeadString,'yStretch',yStretch);
    WriteInteger(HeadString,'Unk2',Unk2);

   end;

  end;

  for i := 0 to ANIMQty-1 do begin
   Read(CEL_ANIM[i],SizeOf(CEL_ANIM[i]));

/// INI ///
   with INI, CEL_ANIM[i] do begin
    HeadString := Header+'_'+inttostr(i);
//    WriteInteger(HeadString,'OffsetNextEntry',OffsetNextEntry);
    WriteInteger(HeadString,'Number',Number);
    WriteInteger(HeadString,'Unknown',Unknown);
    WriteInteger(HeadString,'NumberOfActions',NumberOfActions);
   end;

   SetLength(CEL_ANIMA[i],CEL_ANIM[i].NumberOfActions);
   for j := 0 to CEL_ANIM[i].NumberOfActions-1 do begin
    Read(CEL_ANIMA[i][j],SizeOf(CEL_ANIMA[i][j]));

/// INI ///
    with INI, CEL_ANIMA[i][j] do begin
     HeadString := 'ANIM_'+inttostr(i)+'_'+inttostr(j);
     WriteInteger(HeadString,'Action',Action);
     WriteInteger(HeadString,'Value',Value);
    end;

   end;
   Seek(CEL_ANIM[i].OffsetNextEntry,soBeginning);
  end;

  for i := 0 to IMAGQty-1 do begin
   Read(CEL_IMAG[i],SizeOf(CEL_IMAG[i]));

   with INI, CEL_IMAG[i] do begin

    HeadString := Header+'_'+inttostr(i);

//    WriteInteger(HeadString,'OffsetNextEntry',OffsetNextEntry);
    WriteInteger(HeadString,'Number1',Number1);
//    WriteInteger(HeadString,'XDimension',XDimension);
//    WriteInteger(HeadString,'YDimension',YDimension);
//    WriteInteger(HeadString,'BitDepthFlag',BitDepthFlag);
    WriteInteger(HeadString,'CompFlag',CompFlag);
//    WriteInteger(HeadString,'ImageLength',ImageLength);

    writeln(Header+'|'+inttostr(OffsetNextEntry)+'|'+inttostr(Number1)+'|'+inttostr(XDimension)+'x'+inttostr(YDimension)+'|'+inttostr(BitDepthFlag)+'|'+inttostr(unk2)+'|'+inttostr(CompFlag)+'|'+inttostr(ImageLength));

   end;

   TempoStream.Size := 0; // обнуляем временный поток

   case CEL_IMAG[i].CompFlag of
   0: TempoStream.CopyFrom(InputStream,CEL_IMAG[i].ImageLength);
   1: begin
       FillChar(CEL_LZ,SizeOf(CEL_LZ),0); // обнуляем LZ-буфер, на всякий случай ;)
       Read(CEL_LZ,SizeOf(CEL_LZ));
       CEL_LZ.UnpackedLength := EndianSwap(CEL_LZ.UnpackedLength);
       CEL_LZ.PackedLength := EndianSwap(CEL_LZ.PackedLength);
       CEL_LZDecoder(InputStream,TempoStream,CEL_LZ.PackedLength);
      end;
   end;


   with ImageBuffer[i].ImAttrib do begin
    RealWidth    := CEL_IMAG[i].XDimension;
    RealHeight   := CEL_IMAG[i].YDimension;
    Valid        := True;

    TempoStream.Seek(0,soBeginning);

    case CEL_IMAG[i].BitDepthFlag of
     5 : begin
          TempoStream.Read(Palette,SizeOf(Palette));
          TempoStream2 := TMemoryStream.Create;
          TempoStream2.CopyFrom(TempoStream,TempoStream.Size-SizeOf(Palette));
          TempoStream.Size := 0;
          TempoStream2.Position := 0;
          TempoStream.CopyFrom(TempoStream2,TempoStream2.Size);
          TempoStream.Position := 0;
          FreeAndNil(TempoStream2);
          BitDepth     := 8;
          ExtAlpha     := False;
          VerticalFlip(TempoStream,GetScanLineLen2(RealWidth,8),RealHeight);
          TempoStream.Seek(0,soBeginning);
          ImageBuffer[i].Image.CopyFrom(TempoStream,TempoStream.Size);
         end;
     7 : begin
          Palette := NullPalette; // if BitDepth > 8 then ignored
          BitDepth     := 24;
          ExtAlpha     := True;

          VerticalFlip(TempoStream,GetScanLineLen2(RealWidth,32),RealHeight);
          TempoStream.Seek(0,soBeginning);

          ExtractAlpha(TempoStream,ImageBuffer[i].Alpha,RealWidth,RealHeight);
          StripAlpha(TempoStream,ImageBuffer[i].Image,RealWidth,RealHeight);
         end;
    end;

    OutputStream := TFileStream.Create(ChangeFileExt(FileName,'')+'_'+inttostr(i)+'.tga',fmCreate);

    Export_TGA(ImageBuffer[i].ImAttrib,OutputStream,ImageBuffer[i].Image,ImageBuffer[i].Alpha);

    FreeAndNil(OutputStream);

   end;

   Seek(CEL_IMAG[i].OffsetNextEntry,soBeginning);
  end;

  writeln(inttostr(IMAGQty),' images found in CEL file');
  Read(CEL_ENDC,SizeOf(CEL_ENDC));
 end;

 StopThis:
 try
  RFI_Clear(RFI);
  FreeAndNil(TempoStream);
  FreeAndNil(InputStream);
 except
 end; 

end;

function Import_CEL;
var i,j,ReOffset : integer;
    GlobalDummy  : byte;
    CEL_Header   : TCEL_CP10;
    CEL_ENTR     : array of TCEL_ENTR;
    CEL_ANIM     : array of TCEL_ANIM_INIT;
    CEL_ANIMA    : array of array of TCEL_ANIM_ACTION;
    CEL_IMAG     : array of TCEL_IMAG;
    CEL_LZ       : TCEL_LZ;
    CEL_ENDC     : TCEL_ENDC;
    TempoStream  : TStream;
    IStream      : TStream;
    OStream      : TStream;
    Ini : TINIFile;
    HeadString, TGAFile : string;
    RFI : TRFI;
begin
 GlobalDummy := 0;

 INI := TINIFile.Create(FileName);

 with INI, CEL_Header do begin

  HeadString := 'CP10';

  Header  := 'CP10';
  Dummy1  := ReadInteger(HeadString,'Dummy1',0);
  ENTRQty := ReadInteger(HeadString,'ENTRQty',0);
  ANIMQty := ReadInteger(HeadString,'ANIMQty',0);
  IMAGQty := ReadInteger(HeadString,'IMAGQty',0);
  Dummy2  := 0;
  Length  := 0; // WILL BE UPDATED WHEN OTHER DATA WILL BE WRITTEN
  Unknown := ReadInteger(HeadString,'Unknown',0);

  if IMAGQty < 1 then begin
   writeln('Error: IMAGQty < 1. Aborted');
   raise Exception.Create('IMAGQty < 1');
  end;

  SetLength(CEL_ENTR,ENTRQty);
  SetLength(CEL_ANIM,ANIMQty);
  SetLength(CEL_ANIMA,ANIMQty);
  SetLength(CEL_IMAG,IMAGQty);

  OStream := TFileStream.Create(ChangeFileExt(FileName,'.cel'),fmCreate);

  OStream.Write(CEL_Header,SizeOf(CEL_Header));

  for i := 0 to ENTRQty-1 do begin
   ReOffset := OStream.Position;
   with CEL_ENTR[i] do begin
    Header := 'ENTR';
 /// Устанавливаем имя читаемой секции
    HeadString := Header+'_'+inttostr(i);
    OffsetNextEntry := ReOffset + SizeOf(CEL_ENTR[i]);
 /// Проверка на последний элемент и вставка выравнивания
    if i = ENTRQty-1 then OffsetNextEntry := OffsetNextEntry + 16 - (OffsetNextEntry mod 16);
    Number1  := ReadInteger(HeadString,'Number1',0);
    Number2  := Number1 xor $FFFF0000;
    Unk1     := ReadInteger(HeadString,'Unk1',0);
    xShift   := ReadInteger(HeadString,'xShift',0);
    yShift   := ReadInteger(HeadString,'yShift',0);
    xStretch := ReadInteger(HeadString,'xStretch',0);
    yStretch := ReadInteger(HeadString,'yStretch',0);
    Dummy5   := 0;
    Dummy6   := $FFFFFFFF;
    Unk2     := ReadInteger(HeadString,'Unk2',0);
    OStream.Write(CEL_ENTR[i],SizeOf(CEL_ENTR[i]));
 /// Дописываем выравнивание
    if i = ENTRQty-1 then while OStream.Position <> OffsetNextEntry do OStream.Write(GlobalDummy,1);
   end;
  end;

  for i := 0 to ANIMQty-1 do begin
   ReOffset := OStream.Position;
   with CEL_ANIM[i] do begin
    Header          := 'ANIM';
    HeadString      := Header+'_'+inttostr(i);
    Number          := ReadInteger(HeadString,'Number',0);
    Unknown         := ReadInteger(HeadString,'Unknown',0);
    NumberOfActions := ReadInteger(HeadString,'NumberOfActions',0);

/// Подсчитываем оффсет до следующего элемента
    OffsetNextEntry := ReOffset + SizeOf(CEL_ANIM[i]) + (NumberOfActions * SizeOf(CEL_ANIMA[i]));
/// Паддинг
    if i = ANIMQty-1 then OffsetNextEntry := OffsetNextEntry + 16 - (OffsetNextEntry mod 16);

    OStream.Write(CEL_ANIM[i],SizeOf(CEL_ANIM[i]));

/// Обрабатываем ячейки с анимацией
    SetLength(CEL_ANIMA[i],CEL_ANIM[i].NumberOfActions);
    for j := 0 to CEL_ANIM[i].NumberOfActions-1 do begin
     with CEL_ANIMA[i][j] do begin
      HeadString := 'ANIM_'+inttostr(i)+'_'+inttostr(j);
      Action     := ReadInteger(HeadString,'Action',0);
      Value      := ReadInteger(HeadString,'Value',0);
      OStream.Write(CEL_ANIMA[i][j],SizeOf(CEL_ANIMA[i][j]));
     end;
    end;

/// Паддинг
    if i = ANIMQty-1 then while OStream.Position <> OffsetNextEntry do OStream.Write(GlobalDummy,1);
   end;
  end;

  RFI_Init(IMAGQty);

  for i := 0 to IMAGQty-1 do begin

   TGAFile := ChangeFileExt(FileName,'')+'_'+inttostr(i)+'.tga';

   if not FileExists(TGAFile) then begin
    writeln('Error: ',ExtractFileName(TGAFile),' not found. Aborted');
    raise Exception.Create(ExtractFileName(TGAFile)+' not found');
   end;

/// Импортируем ранее сохранённые TGA-изображения
   writeln('Importing ',ExtractFileName(TGAFile),'...');

   IStream := TFileStream.Create(TGAFile,fmOpenRead);

   TempoStream := TMemoryStream.Create;

   RFI := Import_TGA(IStream,ImageBuffer[i].Image,ImageBuffer[i].Alpha);

   FreeAndNil(IStream);

/// делаем переворот по вертикали
   with RFI do begin
    RAW_AnyToTrueColor(ImageBuffer[i].Image,ImageBuffer[i].Alpha,TempoStream,RealWidth,RealHeight,BitDepth,Palette);
    VerticalFlip(TempoStream,GetScanLineLen2(RealWidth,32),RealHeight);
   end;

   ReOffset := OStream.Position;

   with CEL_IMAG[i], RFI do begin
    Header := 'IMAG';
    HeadString      := Header+'_'+inttostr(i);
    OffsetNextEntry := ReOffset + SizeOf(CEL_IMAG[i]) + TempoStream.Size; // считаем предварительный оффсет
    OffsetNextEntry := OffsetNextEntry + 16 - (OffsetNextEntry mod 16);   // прибавляем паддинг
    Number1         := i;
    XDimension      := RealWidth;
    YDimension      := RealHeight;
    BitDepthFlag := 7;
    unk2            := 0;
    CompFlag        := 0; // CEL Tool always generates uncompressed graphics
    ImageLength     := TempoStream.Size;
/// Пишем заголовок
    OStream.Write(CEL_IMAG[i],SizeOf(CEL_IMAG[i]));
/// Пишем изображение, предварительно перемотав поток
    TempoStream.Seek(0,soBeginning);
    OStream.CopyFrom(TempoStream,TempoStream.Size);

    FreeAndNil(TempoStream);

/// Паддинг
    while OStream.Position <> OffsetNextEntry do OStream.Write(GlobalDummy,1);
   end;
  end;

/// Формируем закрывающий заголовок
  with CEL_ENDC do begin
   Header := 'ENDC';
   celLength := OStream.Position + SizeOf(CEL_ENDC);
  end;

/// Пишем закрывающий заголовок
  OStream.Write(CEL_ENDC,SizeOf(CEL_ENDC));

/// Пересчитываем поле длины файла для основного заголовка
  Length := OStream.Size - SizeOf(CEL_Header);

/// Перематываем файл на начало
  OStream.Position := 0;

/// Пишем новую версию заголовка
  OStream.Write(CEL_Header,SizeOf(CEL_Header));  

 end;
end;

{ A wrapper to LZ decode function }
function CEL_LZDecoder;
begin
 Result := GLZSSDecode(IStream,OStream,CryptLength,$3EE,$3FF); // $3EE $3FF
end;

{ A wrapper to LZ "encode" function }
function CEL_LZEncoder;
begin
 Result := GLZSSEncode(IStream,OStream);
end;

function Import_TGA(InputStream, OutputStream : TStream; OutputStreamA : TStream = nil) : TRFI;
var i : integer;
    TGA : TTGA;
    RGB : TRGB;
    Palette : TPalette;
    TempoStream : TStream;
    RFI : TRFI;
label StopThis;
begin
 RFI.Valid := False;

 TempoStream := TMemoryStream.Create;
 with TGA, InputStream do
  begin
   Seek(0,soBeginning);
 { Reads TGA header (18 bytes) }
   Read(TGA,SizeOf(TGA));

 { TGA Header check code }
   if (Paletted > 1) or (PaletteIndex > 0) then goto StopThis;

 { Grayscale TGA image check }
   if (Paletted = 0) and (ImageType = 1) then Palette := GrayscalePalette;

 { Reading the TGA ID comment }
//   if IDLength > 0 then Read(RFI.Comment,IDLength);

   if PaletteEntrySize > 0 then
    case PaletteEntrySize of
     24 : for i := 0 to GetPaletteColors(BitDepth)-1 do
           begin
          { Reading TGA palette and converting into BMP-compatible }
            Read(RGB,3);
            Palette.Palette[i] := RGBtoARGB(RGB);
           end;
   { I'm not sure if this part of code will be used }
     32 : for i := 0 to GetPaletteColors(BitDepth)-1 do Read(Palette.Palette[i],4);
    end;

   TempoStream.CopyFrom(InputStream,GetScanlineLen2(Width,BitDepth)*Height);
   TempoStream.Seek(0,soBeginning);
 { Copying into internal non-interleaved data container }
   OutputStream.CopyFrom(TempoStream,TempoStream.Size);
 { Copies alpha channel into separate non-interleaved stream and strips it from base stream }
   if (BitDepth > 24) and (OutputStreamA <> nil) then
    begin
     OutputStream.Size := 0;
     ExtractAlpha(TempoStream,OutputStreamA,Width,Height);
     StripAlpha(TempoStream,OutputStream,Width,Height);
     BitDepth := 24;
    end;
  end;

 RFI.RealWidth    := TGA.Width;
 RFI.RealHeight   := TGA.Height;
 RFI.BitDepth     := TGA.BitDepth;
 if OutputStreamA.Size <> 0 then RFI.ExtAlpha := True else RFI.ExtAlpha := False;
// RFI.X            := TGA.X;
// RFI.Y            := TGA.Y;
// RFI.RenderWidth  := 0;
// RFI.RenderHeight := 0;
 RFI.Palette      := Palette; // if BitDepth > 8 then ignored
// RFI.FormatId     := 'TrueVision Targa';

 RFI.Valid := True;

StopThis:
 FreeAndNil(TempoStream);
 Result := RFI;
end;

function Export_TGA(RFI : TRFI; OutputStream, InputStream : TStream; InputStreamA : TStream = nil) : boolean;
var TGA : TTGA;
    Palette : TTGAPalette;
    TempoStream : TStream;
    TempoPalette : TPalette;
begin
 InputStream.Seek(0,soBeginning);
 if InputStreamA <> nil then InputStreamA.Seek(0,soBeginning);

 TempoStream := TMemoryStream.Create;

 with TGA do begin
{ Generating TGA-compatible file }
  IDLength   := 0; // could be changed if there's will be an commentary
{ Marking the TGA as paletted or not }
  if RFI.BitDepth > 8 then
   begin
    Paletted := 0;
    ImageType := 2;
   end
  else
   begin
    Paletted := 1;
    ImageType := 1;
   end;
  PaletteIndex := 0;
  PaletteLength := GetPaletteColors(RFI.BitDepth);
  PaletteEntrySize := 24; // RGB scheme is preferred instead of ARGB.
                          // Unfortunately, drops single color transparency. :]
  X := 0;
  Y := 0;
  Width := RFI.RealWidth;
  Height := RFI.RealHeight;
  BitDepth := RFI.BitDepth;
  ImageDescriptor := 0;
//  ImageDescriptor := 32; // 0, 8 - usual, 32 - flipped

{ TGA uses 3-byte palette instead of 4-byte }
  Palette := ARGBPtoRGBP(RFI.Palette);

{ Combining data & saving bitmap stream... }

  if ((InputStreamA <> nil) and (InputStreamA.Size <> 0)) and RFI.ExtAlpha then
   begin
 // Input image stream,alpha stream,output stream,width,height,bits,palette,prtalpha,interleaved
    TempoPalette := RGBPtoARGBP(Palette);
    RAW_AnyToTrueColor(InputStream,InputStreamA,TempoStream,Width,Height,BitDepth,TempoPalette);
    BitDepth := 32;
   end
  else TempoStream.CopyFrom(InputStream,InputStream.Size);
 end;

 OutputStream.Write(TGA,SizeOf(TGA));
 if GetPaletteSize(RFI.BitDepth) <> 0 then OutputStream.Write(Palette,GetPaletteColors(RFI.BitDepth)*3);

 TempoStream.Seek(0,soBeginning);
 OutputStream.CopyFrom(TempoStream,TempoStream.Size);
 FreeAndNil(TempoStream);

 Result := True;
end;

end.