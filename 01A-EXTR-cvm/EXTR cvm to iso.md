 gamedata.cvm - Conversion to ISO
==================================

 Used Tools
------------
- cvmutil.exe [in-house dev]


 About
-----------

.cvm is basically header + ISO 9660, constaining a few obfuscated data sectors. It is refered to as **CRI ROFS**.

The resulting .iso contains a bunch of (CRI Middleware) AFS files, in particular

- EXE_CEL.AFS
> some UI elements (graphics), glossary, flowchart

- FONT.AFS
> 11x22 and 10x20 bitmap fonts

- SCRIPT.AFS

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	cvmutil.exe gamedata.cvm

 Source(s)
-----------
1. gamedata.cvm

 Product(s)
-----------

* gamedata.cvm.iso


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\01A-EXTR-cvm>cvmutil.exe
	cvmutil version 1.0, Copyright (C) 2011 Nanashi3.
	
	cvmutil comes with ABSOLUTELY NO WARRANTY.
	
	Usage: cvmutil [-e | -d] infile [outfile]
	  -e    encodes .iso to .cvm
	  -d    decodes .cvm to .iso
	
	If unspecified, name for outfile is automatically derived from infile
	
	
	C:\work\_TL_\akaiito_php\lab\01A-EXTR-cvm>cvmutil.exe gamedata.cvm
	DECODING: auto-detected CVM header on input, outputting .iso
	
	C:\work\_TL_\akaiito_php\lab\01A-EXTR-cvm>



