<?php

$GROUPS = array(
	1 => 'a',
	2 => 'ka',
	3 => 'sa',
	4 => 'ta',
	5 => 'na',
	6 => 'ha',
	7 => 'ma',
	8 => 'ya',
	9 => 'ra',
	10 => 'wa',
);

function readent($fp)
{
	$str1 = readpstr($fp, 1);
	if ($str1 === FALSE)
		return FALSE;
	$str2 = readpstr($fp, 1);
	if ($str2 === FALSE)
		return FALSE;

	$str3 = readpstr($fp, 2);
	if ($str3 === FALSE)
		return FALSE;
	return array(
		'orig' => $str1,
		'name' => $str1,
		'read'  => $str2,
		'text' => $str3
	);
}

function readpstr($fp, $preflen)
{
	$len1 = fread($fp, $preflen);
	if ($len1 === FALSE || strlen($len1) < $preflen)
		return FALSE;
	switch($preflen)
	{
	case 1:
		$len1 = ord($len1);
		break;
	case 2:
		$len1 = current(unpack('v', $len1));
		break;
	}
	if ($len1 == 0)
		return FALSE;
		
	$str1 = fread($fp, $len1);
	if ($str1 === FALSE || strlen($str1) < $len1)
		return FALSE;

	assert(ord(substr($str1, -1, 1)) == 0);

	return substr($str1, 0, -1);
}

@mkdir('20decodedglossary');

{
	$fp = fopen("yougo.ymd", 'rb');
	$entgroup = 0;
	$globcounter = 0;
	$index = array();
	$reads = array();
	
	while(($nument = fread($fp, 1)) !== FALSE)
	{
		++$entgroup;
		if(strlen($nument) == 0 || ($nument = ord($nument)) == 0)
		{
			fprintf(STDERR, "*** STOP at 0x%04X --- cf. filesize = %04X\n", ftell($fp), filesize("yougo.ymd"));
			break;
		}
		$entries = array();
		fprintf(STDERR, "*** reading[%d] at 0x%04X\n", $nument, ftell($fp));
		for($i = 0; $i < $nument; ++$i)
		{
			if(($obj = readent($fp)) === FALSE)
			{
				fprintf(STDERR, "Cannot read obj %d at 0x%04X\n", $i, ftell($fp));
				exit;
			}
			$entries[$globcounter++] = $obj;
			$index[] = $obj['orig'];
			$reads[] = $obj['read'];
		}
		$groupname = sprintf("group%d-%s", $entgroup, $GROUPS[$entgroup]);
		file_put_contents("20decodedglossary/$groupname.txt", 'return '. var_export($entries, true). ';');
	}
	
	file_put_contents("20decodedglossary/allentries.txt", implode("\r\n", $index));
	file_put_contents("20decodedglossary/allentries-read.txt", implode("\r\n", $reads));
	
	fclose($fp);
	
}
?>