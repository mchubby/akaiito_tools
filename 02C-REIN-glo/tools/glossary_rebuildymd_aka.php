<?php

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


function writepstr($fp, $str, $preflen)
{
	$len1 = strlen($str) + 1;
	switch($preflen)
	{
	case 1:
		$len1 = chr($len1 & 0xFF);
		break;
	case 2:
		$len1 = pack('v', $len1 & 0xFFFF);
		break;
	}
		
	fwrite($fp, "$len1$str". chr(0));
}

@mkdir('40buildedglossary');

$fileout = fopen('40buildedglossary/yougo.ymd','wb');

$index = array();
$globcounter = 0;

foreach($GROUPS as $grpnum => $grpcat)
{
	$groupname = sprintf("group%d-%s", $grpnum, $grpcat);
	$arr = eval(file_get_contents("30insertedglossary/$groupname.txt"));
	if ( $arr === FALSE && ( $error = error_get_last() ) )
	{
		fprintf(STDERR, "Syntax error in glossary file %s: %s FILE:%s LINE:%s\r\n", "30insertedglossary/$groupname.txt", $error['message'], $error['file'], $error['line']);
		return;
	}
	$groupcount = count($arr);
	fwrite($fileout, chr($groupcount & 0xFF));
	$globcounter += $groupcount;
	foreach($arr as $val)
	{
		$index[] = $val['orig'];
		if(md5($val['orig']) == '92717636085e57065d4994491a481556')
		{
			// Match: orig is "yougo jiten"
			$bitfield_byte = (count($index) - 1) >> 3;
			$bitfield_bit = (count($index) - 1) % 8;
		}
		writepstr($fileout, $val['name'], 1);
		writepstr($fileout, $val['read'], 1);
		writepstr($fileout, $val['text'], 2);
	}
}

file_put_contents("40buildedglossary/allentries-index.txt", implode("\r\n", $index));

if(!isset($bitfield_byte))
{
	fprintf(STDERR, "Warning: orig:'<yougo jiten>' entry not defined\r\n");
}
else
{
	fprintf(STDERR, "'<yougo jiten>' info:\r\n");
	fprintf(STDERR, "--- Mnemonic ---\r\n");
	fprintf(STDERR, "OR BYTE PTR [EAX+%02X], %02X\r\n", 0x78+$bitfield_byte, (1 << $bitfield_bit));
	fprintf(STDERR, "--- Binary ---\r\n");
	fprintf(STDERR, "Search:\r\n    E8 8DB60600 8B4D 24 8B41 04  8B98 98000000  6A 01 8DB5 00010000 68 1C144900 83CB 10  56\r\nOr:\r\n    E8 8DB60600 8B4D 24 8B41 04  8088  ?000000   ? 90 8DB5 00010000 6A 01 68 1C144900 90 56\r\n");
	fprintf(STDERR, "Replace:\r\n    E8 8DB60600 8B4D 24 8B41 04  8088 %02X000000  %02X 90 8DB5 00010000 6A 01 68 1C144900 90 56\r\n", 0x78+$bitfield_byte, (1 << $bitfield_bit));
	fprintf(STDERR, "\r\n");
	fprintf(STDERR, "Note: if found 1st search pattern, NOP(90 90 90 90 90 90) the subsequent:\r\n    8998 98000000\r\n");

	
}

?>
