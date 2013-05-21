 flowchart [flow_fcb.bin]: Unpacking
=====================================

 Used Tools
------------
- `flow_decode_aka.php` [in-house dev]

## About ##

**flow_fcb.bin** defines what information is printed when accessing the built-in flowchart feature from the game menu.

A descriptor index is hardwired into the game exe, at a specific file offset.


- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

 Command
-----------
	php flow_decode_aka.php Akaiito.exe

Script has no external subscript dependency.


 Source(s)
-----------
1. Akaiito.exe - game executable
2. `flow_fcb.bin` [extracted from EXE_CEL.AFS]

 Product(s)
-----------

* Folder:`10rawflow`
* Folder:`20decodedflow`

* `.\10rawflow\*.raw` node dump [not used in later stages, only for manual comparison vs. recompiles]

* `.\20decodedflow\flow.NN.afcb` node binary data

* `.\20decodedflow\flow.NN.afcb.txt` node text strings, where applicable


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02D-EXTR-flow>php flow_decode_aka.php Akaiito.exe
	string(11) "Akaiito.exe"
	
	C:\work\_TL_\akaiito_php\lab\02D-EXTR-flow>
	

