 Modding .cel graphics: cel to tiff and png
============================================

 Used Tools
------------
- cel2tiff.exe [in-house dev]
- Convert.exe by ImageMagick Studio LLC

## About .cel graphics ##

.cel are image containers for 32-bit bitmaps (RGB images with 256-level transparency) or palettized (discrete number of RGBA colors, still with transparency).
A given .cel may simply contain a single image, a collection of multiple images, or a collection of key frames for an animation.

**DOCREF**: Refer to `celformat.txt` (Reverse-engineered by `Nik_`) for more info, and Nik's own tool cel_tool.exe.

C# program cel2tiff.exe converts into a .tiff multipage standard format.
1st page is a placeholder image, 2nd & forth are the actual images; you may need a compatible image viewer to break it down into several components.
Modding animations is not really supported.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

## tiff generation ##

 Command
-----------
	cel2tiff.exe mm_menu.cel

 Source(s)
-----------
1. .cel file [extracted from *.AFS]

 Product(s)
-----------

* ~sourcefile~.cel.tiff multipage image
* ~sourcefile~.*.lz77 One to several compressed segments
* ~sourcefile~.*.palette.bin [for information/debug purposes]



 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02B-EXTR-cel>cel2tiff.exe mm_menu.cel
	
	C:\work\_TL_\akaiito_php\lab\02B-EXTR-cel>
	
You may use the following batch (.bat) to batch convert files:

	@echo off
	for %%f in (*.cel); do ( echo %%f & C:\work\_TL_\akaiito_cel\cel_cel2tiff\bin\Release\cel2tiff.exe %%f )


- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

## tiff decomposition ##

1. Create a subfolder named after the .cel, e.g. `mm_menu`
2. Copy and rename `mm_menu.cel.tiff` into `.\mm_menu`
3. Use the ImageMagick toolset to split the tiff file.
Although it originates from Linux, there are also compiled binaries for Windows, where each .exe is standalone.

 Command
-----------
	convert.exe -scene 1 mm_menu.cel.tiff -delete 0 -write PNG32:org_%02d.png PNG32:edited_%02d.png

Note: Pay no attention to the conversion warnings, they refer to metadata that is stored in the TIFF's first page.

 Source(s)
-----------
1. ~sourcefile~.cel.tiff

 Product(s)
-----------

* `org_01.png`, `org_02.png`, etc.
* `edited_01.png`, `edited_02.png`, etc.  
You should delete all `edited_*.png` files you do not intend to localize, and the others are where you will put modifications.

 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02B-EXTR-cel\mm_menu>"C:\Tools\ImageMagick\Convert.exe" -scene 1 mm_menu.cel.tiff -delete 0 -write PNG32:org_%02d.png PNG32:edited_%02d.png
	Convert.exe: mm_menu.cel.tiff: unknown field with tag 37500 (0x927c) encountered. `TIFFReadDirectory' @ warning/tiff.c/TIFFWarnings/706.
	Convert.exe: mm_menu.cel.tiff: unknown field with tag 59933 (0xea1d) encountered. `TIFFReadDirectory' @ warning/tiff.c/TIFFWarnings/706.
	
	C:\work\_TL_\akaiito_php\lab\02B-EXTR-cel\mm_menu>
	
