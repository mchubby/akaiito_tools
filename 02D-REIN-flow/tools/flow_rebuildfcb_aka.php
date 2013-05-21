<?php

$offset = 0;
$offsets = '';
$fileout = fopen('40buildedflow/flow_fcb.bin','wb');

$dir = opendir('40buildedflow');
while($file = readdir($dir))
{
	if($file!='.' && $file!='..')
	{
		$info = pathinfo(basename($file));
		if($info['extension'] === 'raw')
		{
			$offsets .= pack('V', $offset);
			$offset += filesize("40buildedflow/$file");
			fwrite($fileout,file_get_contents("40buildedflow/$file"));
		}
	}
}
closedir($dir);

if(strlen($offsets) > 0)
{
	file_put_contents("40buildedflow/offsettab.bin", $offsets);
}

?>
