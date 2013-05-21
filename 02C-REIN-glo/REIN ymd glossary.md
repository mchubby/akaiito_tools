 glossary [yougo.ymd]: Repacking
=================================

 Used Tools
------------
- `glossary_rebuildymd_aka.php` [in-house dev]
- A hex editor

#### Working Directory ####

To get started, create a working copy of folder .\20decodedglossary and name it "30insertedglossary".
That's where all editing should go exclusively.

## About ##

`glossary_rebuildymd_aka.php` is the repacking script.
You can see the following definition at the start of file:

	$GROUPS = array(
		1 => 'A-B',
		2 => 'C-D',
		3 => 'E-G',
		4 => 'H-I',
		5 => 'J-K',
		6 => 'L-N',
		7 => 'O-Q',
		8 => 'R-S',
		9 => 'T-U',
		10 => 'V-Z',
	);

It determines how group files are being fetched, allowing them to be arbitrarily split, instead of "group1-a", "group2-ka", etc.

You may want to adjust how glossary groups are populated, so that all groups are somewhat evenly balanced.
Each group is allowed at most 255 entries.

With this suggested distribution, input files in
`.\30insertedglossary\group*.txt` should be:
> group1-A-B.txt  
> group2-C-D.txt  
> group3-E-G.txt  
> group4-H-I.txt  
> group5-J-K.txt  
> group6-L-N.txt   
> group7-O-Q.txt  
> group8-R-S.txt  
> group9-T-U.txt  
> group10-V-Z.txt  

Therefore, you should rename all files accordingly.

## Guidelines for modifications:

- Encoding of .txt files is **Shift-JIS**. Other encodings are untested.

- Entries may be transferred back and forth between groups, but you need to leave array indices untouched.

- those txt files are included into php code. As such their syntax should be compliant (e.g. make sure there's no extra, unescaped quote to break things)  
  In such cases, the script will complain about syntax errors when attempting to assemble, prompting you to fix things at a given location.

- On completion, script will print instructions to modify x86 assembly code. The code sets which entry ('*yougo jiten* in kanji') is initially displayed when the glossary is invoked in game.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	php glossary_rebuildymd_aka.php

 Source(s)
-----------
1. .\30insertedglossary\group*.txt

 Product(s)
-----------

* Folder:`40buildedglossary`

* `.\40buildedglossary\yougo.ymd` => it should be included in **EXE_CEL.AFS** rebuilds

* `.\40buildedglossary\allentries-index.txt`  [used for compiling scripts]


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02C-REIN-glo>php glossary_rebuildymd_aka.php
	'<yougo jiten>' info:
	--- Mnemonic ---
	OR BYTE PTR [EAX+78], 02
	--- Binary ---
	Search:
	    E8 8DB60600 8B4D 24 8B41 04  8B98 98000000  6A 01 8DB5 00010000 68 1C144900 83CB 10  56
	Or:
	    E8 8DB60600 8B4D 24 8B41 04  8088  ?000000   ? 90 8DB5 00010000 6A 01 68 1C144900 90 56
	Replace:
	    E8 8DB60600 8B4D 24 8B41 04  8088 78000000  02 90 8DB5 00010000 6A 01 68 1C144900 90 56
	
	Note: if found 1st search pattern, NOP(90 90 90 90 90 90) the subsequent:
	    8998 98000000
	
	C:\work\_TL_\akaiito_php\lab\02C-REIN-glo>

