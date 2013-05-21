 glossary [yougo.ymd]: Unpacking
=================================

 Used Tools
------------
- `glossary_decode_aka.php` [in-house dev]

## About ##

**yougo.ymd** is the embedded glossary found in game. It is accessible using the game menu.

It is divided into several sections, in the usual Japanese categorization as found in dictionaries.
The unpack script splits yougo.ymd into 10 groups of text files, where entries are placed depending on the pronunciation of the term's first syllable.

Glossary entries are referenced to in game scripts, it is therefore preferable to extract its data in earlier processing stages.

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	php glossary_decode_aka.php

Script has no external subscript dependency.


 Source(s)
-----------
1. yougo.ymd [extracted from EXE_CEL.AFS]

 Product(s)
-----------

* Folder:`20decodedglossary`

* yougo.ymd => `.\20decodedglossary\group*.txt` glossary group entries
> group1-a.txt  
> group2-ka.txt  
> group3-sa.txt  
> group4-ta.txt  
> group5-na.txt  
> group6-ha.txt   
> group7-ma.txt  
> group8-ya.txt  
> group9-ra.txt  
> group10-wa.txt  

* yougo.ymd => `.\20decodedglossary\allentries.txt` & `.\20decodedglossary\allentries-read.txt` [used for unpacking source scripts]


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02C-EXTR-glo>php glossary_decode_aka.php
	*** reading[52] at 0x0001
	*** reading[43] at 0x5DA8
	*** reading[45] at 0x9A28
	*** reading[31] at 0xE57A
	*** reading[19] at 0x11D52
	*** reading[39] at 0x13C99
	*** reading[24] at 0x17548
	*** reading[9] at 0x19AA6
	*** reading[11] at 0x1A5B1
	*** reading[4] at 0x1B19D
	*** STOP at 0x1B61B --- cf. filesize = 1B61B
	
	C:\work\_TL_\akaiito_php\lab\02C-EXTR-glo>
	


