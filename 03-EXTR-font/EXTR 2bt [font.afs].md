 2bt Fonts [FONT.AFS]: Unpacking
=================================

 Used Tools
------------
- `font_lines2tga.php` [in-house dev]

## About .2bt fonts ##

.2bt is a bitmap font format featuring 8 levels of gray (3-bpp grayscale).

**DOCREF**: Refer to `2bt.txt` for a very crude look at the internals.


- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

* font2008.2bt (20px, 10px-wide, 8-bit chars)
* font2016.2bt (20px, 22px-wide, high range)
* font2208.2bt (22px, 11px-wide, 8-bit chars)
* font2216.2bt (22px, 22px-wide, high range)


 Command
-----------
	php font_lines2tga.php font2008.2bt

 Source(s)
-----------
1. .2bt file [extracted from FONT.AFS]

 Product(s)
-----------

* Folder:`20decodedfont`
* .\20decodedfont\~fontname~.tga

 Expected Output
-----------
	C:\work\_TL_\akaiito_php\lab\03-EXTR-font>php font_lines2tga.php font2008.2bt
	
	C:\work\_TL_\akaiito_php\lab\03-EXTR-font>

The `fontNN16.2bt` font files are full-width variants that were not useful to decode at the time.
