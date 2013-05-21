 AFS - Archive Extraction
==========================

 Used Tools
------------
- AFSPacker.exe by PacoChan


 About
-----------

CRI Middleware AFS is extracted with standard tools such as **AFS Extract 7** (GUI) or **AFS Packer** (commandline).

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	AFSPacker.exe -e FONT.AFS FONT list_FONT.txt

You may use the following command file (.bat) to batch convert files:

	@echo off
	for %%f in (*.afs); do ( echo %%f & AFSPacker.exe -e %%f %%~nf list_%%~nf.txt )

 Source(s)
-----------
1. *.afs archive [extracted from .iso]

 Product(s)
-----------

* Extracted items in subfolder, e.g. FONT
* Ordered file list, e.g. list_FONT.txt


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\01B-EXTR-afs>AFSPacker.exe -e FONT.AFS FONT list_FONT.txt
	
	      #----------------------------------------------------------------#
	      #                    AFS Packer - Version 1.1                    #
	      #           By PacoChan - http://pacochan.tales-tra.com          #
	      #----------------------------------------------------------------#
	
	
	Extracting files...
	
	
	Reading TOC... 3/3
	Reading attributes table... 3/3
	Reading files... 3/3
	
	
	Operation complete.
	
	C:\work\_TL_\akaiito_php\lab\01B-EXTR-afs>
