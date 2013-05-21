<?php


function decodefcb($bin)
{

} // end of function

function readstrtab($bin, $offset, $params)
{
	$txt = '';
	for($k = 0; $k < $params['STRCOUNT']; ++$k)
	{
		$len = ord(substr($bin,$offset,1));
		$str = strtr(substr($bin,$offset+1,$len), array('='=>'^EQ^'));
		$txt .= sprintf("<STR:%d>%s=\n", $k+1, $str);

		$offset += ++$len;
	}
	
	if(strlen($txt) > 0)
	{
		$nodetxtpath = sprintf("20decodedflow/flow.%02d.afcb.txt", $params['INDEX']);
		file_put_contents($nodetxtpath, $txt);
	}

} // end of function


@mkdir('10rawflow');
@mkdir('20decodedflow');

$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';

if (strlen($script))
{
	var_dump($script);
	decodefcb($script);

	$bin = file_get_contents("flow_fcb.bin");
	$fp = fopen($script, 'rb');
	$NUMENT = 0x32;
	fseek($fp, 0x92728); // PC exe
	//fseek($fp, 0x134370); // PS2 ELF
	$offsets = array();
	$lengths = array();
	$prevoffset = null;
	for ($i=0; $i < $NUMENT; ++$i)
	{
		$offsets[] = $offset = current(unpack("V", fread($fp,4)));
		if ($prevoffset !== null)
		{
			$lengths[] = $offset - $prevoffset;
		}
		$prevoffset = $offset;
	}
	$lengths[] = strlen($bin) - $prevoffset;
	
	//var_dump($offsets, $lengths);

	for ($i=0; $i < $NUMENT; ++$i)
	{
		$nodebinpath = sprintf("20decodedflow/flow.%02d.afcb", $i);
		file_put_contents($nodebinpath, substr($bin, $offsets[$i], $lengths[$i]));
		$numx = current(unpack("v", substr($bin, $offsets[$i] + 0x4, 2)));
		$numy = current(unpack("v", substr($bin, $offsets[$i] + 0x6, 2)));
		$offsetend = 8 + $numx * $numy * 0xC;
		//file_put_contents($nodebinpath, substr($bin, $offsets[$i], $lengths[$i]));
		file_put_contents($nodebinpath, substr($bin, $offsets[$i], $offsetend + 4));
		$nodebinpath = sprintf("10rawflow/flow.%02d.raw", $i);
		file_put_contents($nodebinpath, substr($bin, $offsets[$i], $lengths[$i]));
		$numstr = current(unpack("V", substr($bin, $offsets[$i] + $offsetend, 4)));
		
		readstrtab($bin, $offsets[$i] + $offsetend + 4, array('INDEX'=>$i,'STRCOUNT'=>$numstr));
	}


	fclose($fp);
	
}
/*
else
{
	$dir = opendir('10rawscript');
	while($file = readdir($dir))
	{
		if($file!='.' && $file!='..')
		{
			$info = pathinfo(basename($file));
			if($info['extension'] === 'DAT')
			{
				decodefcb($file);
			}
		}
	}
	closedir($dir);
}

*/
?>