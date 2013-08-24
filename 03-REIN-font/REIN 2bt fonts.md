 2bt Fonts [FONT.AFS]: Repacking
=================================

 Used Tools
------------
- `font_tga2lines.php` [in-house dev]
- [Angelcode's Bitmap Font Generator](http://www.angelcode.com/products/bmfont/)
- A precompiled modified version of Cyotek's [AngelCode bitmap font parsing using C#](http://www.codeproject.com/Articles/317694/AngelCode-bitmap-font-parsing-using-Csharp) is provided in the tools folder.
- Convert.exe by ImageMagick Studio LLC

#### Working Directory ####

To get started, create a working copy of folder .\20decodedfont and name it "30insertedfont".
That's where all editing should go exclusively.

## Setup:

- Install source monospace font (e.g. 'Liberation Mono') into the operating system folder.
- Install Angelcode's Bitmap Font Generator.
- The special version of "AngelCode bitmap font parsing using C#" generates all needed files when a .fnt is being loaded.


## Guidelines for modifications:

- BMFont's bmfc configuration file is a text file containing directives on how to generate the texture file with bitmap glyphs. It is generated in the application's menu [Options &gt; Save configuration as].
Relevant parameters are:
	* `useFixedHeight=1` - all glyph sub-images are evenly matched
	* `outWidth=14`, `outHeight=2280` - output texture depending on target font grid dimensions. For example, characteristics of font2408 is 12x24 glyph size, so the ASCII range is 95 characters multiplied by 24 px = 2280 height
	* `fontDescFormat=1`, `textureFormat=png` (xml export)
	* `chars=32-126` (latin range)
	* Adjust `paddingDown=0` and `paddingUp=3` so that all entries in the .fnt descriptor match the target size (height="24" instead of height="21")

In BMFont, export a texture [Options &gt; Save bitmap font as...] into a .fnt.


- Run "`BitmapFontParser\BitmapFontViewer\bin\Release\BitmapFontViewer.exe`" and open your generated .fnt file. Close the application right after the font has been loaded.
- In "`BitmapFontParser\BitmapFontViewer\bin\Release\`" you shall find generated files `map_image.png` and `your_name.fnt.png`. Copy these into `30insertedfont`.

- rename `your_name.fnt.png` into `~fontname~_unmapped.png` (e.g. font2408_unmapped.png)

- you will now map the image to the grayscale palette.
Each pixel of a character grid has 8 possible values, mapping to these colors (transparent black background + white #FFFFFF & variable alpha channel):

		$PALETTE = array(
			0 => "\x00\x00\x00\x00",
			1 => "\xFF\xFF\xFF\x33",
			2 => "\xFF\xFF\xFF\x55",
			3 => "\xFF\xFF\xFF\x77",
			4 => "\xFF\xFF\xFF\x99",
			5 => "\xFF\xFF\xFF\xBB",
			6 => "\xFF\xFF\xFF\xDD",
			7 => "\xFF\xFF\xFF\xFF",
		);
	You need to make sure all colors used in the replacement font match this palette.

## Palette Color Mapping ##
 Command
-----------
	convert.exe PNG32:font2408_unmapped.png -map PNG32:map_image.png PNG32:font2408_mapped.png


- Load the resulting `font2408_mapped.png` and save it as TGA:
 * 32 bits per pixel format (RGBA)
 * v1 (no extended header)
 * no compression (no RLE)

You may use an editor such as Paint.Net which is known to produce compatible files.

----------

 Command
-----------
	php pal_tga2lines.php 30insertedfont\font2408_mapped.tga

 Source(s)
-----------
1. ~fontname~.tga file in 30insertedfont

 Product(s)
-----------

* Folder:`40buildedfont`
* .\40buildedfont\~fontname~.2bt [reinsert into FONT.AFS]

Note: For both font2016.2bt and font2216.2bt, a round-trip conversion may yield different results. I don't know if it matters because I'm not changing those, but be aware of it.
The `fontNN16.2bt` font files are full-width variants that were left untouched when localizing for a given language (that only used roman letters).

 Expected Output
-----------
	C:\work\_TL_\akaiito_php\lab\03-REIN-font>php font_tga2lines.php 30insertedfont\font2016.tga

	C:\work\_TL_\akaiito_php\lab\03-REIN-font>

