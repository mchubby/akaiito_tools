 Script Localization [SCRIPT.AFS] - Modding
============================================

 Used Tools
------------
- `script_compile_aka.php` [in-house dev]

#### Working Directory ####

To get started, create a copy of folder .\20decodedscript and name it "30insertedscript".
\20decodedflow and name it "30insertedflow".

Copy "40buildedglossary" created during a previous step into the working dir.


## About ##

There are three sources for localizable text:

- Dialogue
- Selection
- Flowchart reference

Files where designed to work with [Sakuraume's Text Editor](http://vn.i-forge.net/tools/#text+files+editor).

Encoding is Shift-JIS and translated text using characters outside ASCII is untested and not guaranteed to work.

## Guidelines for modifications:

- Extraction has generated *.l10n.txt files containing translatable text.  

- I recommend leaving names alone when starting localization, those can be substituted in later stages.

- Note: all translated entries in the .txt file will cause untranslated strings to be used during the recompilation process.

- Limitations:
  Some strings may be limited to a hard limit of around 256 characters. It should be plenty enough for most use cases.

_Sample translated line:_

	<FLO:1>　[CR]^CRLF^―――――――――――――――――――――――[CR]^CRLF^◇それは物語の予兆[CR]^CRLF^―――――――――――――――――――――――[CR]^CRLF^　=　[CR]^CRLF^―――――――――――――――――――――――[CR]^CRLF^◇It was the Omen of a Legend[CR]^CRLF^―――――――――――――――――――――――[CR]^CRLF^　


- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -
	
 Command
-----------
	php script_compile_aka.php
-or-

	php script_compile_aka.php <scriptname.DAT.script>

 Source(s)
-----------
1. .\30insertedscript\~scriptname~.DAT.script
2. .\30insertedscript\~scriptname~.DAT.l10n.txt
3. .\40buildedglossary\allentries-index.txt [from a previous step]

Note: every time you compile a newer `yougo.ymd`, you should consider rebuilding all scripts.

 Product(s)
-----------

* Folder:`40buildedscript`

* .\40buildedscript\~scriptname~.DAT

All DAT should be repacked into SCRIPT.AFS by specifying an input filelist. It is important to preserve the **ordering of entries**.


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\04-REIN-script>php script_compile_aka.php
	
	C:\work\_TL_\akaiito_php\lab\04-REIN-script>

