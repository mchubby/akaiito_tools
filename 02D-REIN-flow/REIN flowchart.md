 flowchart [flow_fcb.bin]: Repacking
=====================================

 Used Tools
------------
- `flow_compile_aka.php` [in-house dev]
- `flow_rebuildfcb_aka.php` [in-house dev]
- A hex editor

#### Working Directory ####

To get started, create a copy of folder .\20decodedflow and name it "30insertedflow".
That's where all editing should go exclusively.

## About ##

Recombining a `flow_fcb.bin` flowchart is a 2-step process.

- Generate nodes from binary .afcb + translated resource file .afcb.txt => creates .raw files
- Combine .raw files into a single flow_fcb.bin + produce an offsets table to be embedded into the game exe.


## Guidelines for modifications:

- Encoding of .txt files is **Shift-JIS**. Other encodings are untested.

- Extraction has generated `*.afcb.txt` files for nodes containing translatable text.  
  + Files where designed to work with [Sakuraume's Text Editor](http://vn.i-forge.net/tools/#text+files+editor)
  + Length of each translated entry is at most 127 characters.

_Sample translated line:_

	<STR:1>¶ŠˆŒüãŒv‰æ=Environment Amelioration Project
- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

## Make individual nodes

`flow_compile_aka.php` compiles individual flowchart nodes.

 Command
-----------
	php flow_compile_aka.php

 Source(s)
-----------
1. .\30insertedflow\flow.*.afcb
2. .\30insertedflow\flow.*.afcb.txt

 Product(s)
-----------

* Folder:`40buildedflow`

* `.\40buildedflow\flow.*.raw` => binary node

 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02D-REIN-flow>php flow_compile_aka.php
	
	C:\work\_TL_\akaiito_php\lab\02D-REIN-flow>

- - - - - - - - - - - - - - - - - - - - - - - - - - - - - -

## Rebuild flowchart

`flow_rebuildfcb_aka.php` rejoins the flow.*.raw parts and produces the offsets table.

This table should override the one built into the original game executable.


 Command
-----------
	php flow_rebuildfcb_aka.php

 Source(s)
-----------
1. `.\40buildedflow\flow.*.raw`

 Product(s)
-----------

* `.\40buildedflow\flow_fcb.bin` => it should be included in **EXE_CEL.AFS** rebuilds

* `.\40buildedflow\offsettab.bin` => It should be written in game executable at a specific location.


 Expected Output
-----------

	C:\work\_TL_\akaiito_php\lab\02D-REIN-flow>php flow_rebuildfcb_aka.php
	
	C:\work\_TL_\akaiito_php\lab\02D-REIN-flow>
