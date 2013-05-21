 Script Localization [SCRIPT.AFS] - Extraction
===============================================

 Used Tools
------------
- `script_decode_aka.php` [in-house dev]

## About

Extract and reinsert scripts are somewhat basic but fit for purposes.

Code Template:  Aoishiro Extraction and Insertion Tool – courtesy of Nik

## Extraction ##

1. In base folder, drop a copy of "20decodedglossary", extracted during a previous step.
2. Create a folder named "10rawscript" and extract all files from SCRIPT.AFS.  
**script.afs** entries are referenced internally by script call instructions (ExecScript, CallScript, etc.)
Order of items is important in script.afs, therefore a prepopulated `_filelist.txt` is provided (`_pctrial-DAT.txt`, `_ps2-DAT.txt`)

3. Place a copy of `_filelist.txt` into ".\10rawscript"

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	php script_decode_aka.php
-or-

	php script_decode_aka.php <scriptname.DAT>

 Source(s)
-----------
1. any extracted .DAT file [from script.afs]
2. .\20decodedglossary\allentries.txt (from a previous step)
3. .\20decodedglossary\allentries-read.txt (from a previous step)
4. .\10rawscript\_filelist.txt (provided)

 Product(s)
-----------

* Folder:`20decodedscript`

* ~sourcefileDAT~.script    => code logic

* ~sourcefileDAT~.l10n.txt  => localization text [where applicable]

Encoding for l10n is the same as source (SJIS)

 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\04-EXTR-script>php script_decode_aka.php
	
	C:\work\_TL_\akaiito_php\lab\04-EXTR-script>
