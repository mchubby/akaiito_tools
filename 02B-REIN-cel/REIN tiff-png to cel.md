 Modding .cel graphics: png to tiff to cel
===========================================

 Used Tools
------------
- tiff2cel.exe [in-house dev]

## About ##
To maximize compatibility, it is recommended to feed input PNG images in 32bpp RGBA color format.

.cel are image containers for 32-bit bitmaps (RGB images with 256-level transparency) or palettized (discrete number of RGBA colors, still with transparency).

C# program tiff2cel.exe produces a .cel image container using:

- a .tiff multipage standard format produced by cel2tiff
- one or several .png images
- one or several precompresed segments

Modding animations is not really supported.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

## Image Modding ##

In a previous step, the following images were generated:

* `org_01.png`, `org_02.png`, etc.
* `edited_01.png`, `edited_02.png`, etc.  

For frames you don't need or want translated, copy the compressed binary blobs (raw*.lz77) from parent folder into the working subfolder. These frames will be passed through with optimal efficiency.

Make sure you have exported your final work into `edited_01.png`, `edited_02.png`, etc.
Preferably, save them as 32-bit PNGs.


## .cel remuxing ##

Open a command prompt and chdir to the working subfolder.

 Command
-----------
	tiff2cel.exe mm_menu.cel.tiff

 Source(s)
-----------
1. ~source~.cel.tiff
2. ~source~.raw_NN.lz77 [optional]
3. edited_NN.png [optional]

 Product(s)
-----------

- ~source~.cel.tiff.cel

You may copy and rename the produced container, for later AFS reinsertion.

 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02B-REIN-cel\mm_menu>tiff2cel.exe mm_menu.cel.tiff
	....................
	C:\work\_TL_\akaiito_php\lab\02B-REIN-cel\mm_menu>
	
