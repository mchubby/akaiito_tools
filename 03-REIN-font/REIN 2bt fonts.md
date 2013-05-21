 2bt Fonts [FONT.AFS]: Repacking
=================================

 Used Tools
------------
- `font_tga2lines.php` [in-house dev]

#### Working Directory ####

To get started, create a working copy of folder .\20decodedfont and name it "30insertedfont".
That's where all editing should go exclusively.

## Guidelines for modifications:

- Save TGA with the same 32-bit format as source:
 * v1 (no extended header)
 * no compression (no RLE)

- Each pixel of a character grid has 8 possible values, mapping to these colors (white #FFFFFF + alpha channel):

		$PALETTE = array(
			0 => "\xFF\xFF\xFF\x00",
			1 => "\xFF\xFF\xFF\x33",
			2 => "\xFF\xFF\xFF\x55",
			3 => "\xFF\xFF\xFF\x77",
			4 => "\xFF\xFF\xFF\x99",
			5 => "\xFF\xFF\xFF\xBB",
			6 => "\xFF\xFF\xFF\xDD",
			7 => "\xFF\xFF\xFF\xFF",
		);
	You need to make sure all colors used in the replacement font match this palette.


- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	php pal_tga2lines.php 30insertedfont\font2008.tga

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

