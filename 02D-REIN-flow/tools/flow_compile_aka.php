<?php


class TLmgr
{
	public function __construct($path)
	{
		$this->original = array();
		$this->translation = array();
		if (file_exists($path))
			foreach(file($path) as $line)
			{
				if(preg_match('/^<[^:]+:(?P<id>[^:>]+)(:[^>]+)?'.'>[\\s]*(?P<org>[^=]+)=(?P<tl>[^\\r\\n]*)/', $line, $matches))
				{
					$id = intval($matches['id']);
					$this->original_raw[$id] = $matches['org'];
					$this->original[$id] = TLmgr::unescape_tags($matches['org']);
					$this->translation[$id] = TLmgr::unescape_tags($matches['tl']);
					$this->translation_raw[$id] = $matches['tl'];
				}
			}
	}

	// Get translated string or fallback
	public function get($id)
	{
		return (isset($this->translation[$id]) && $this->translation[$id] != '')? $this->translation[$id] : $this->original[$id];
	}
	
	
	public static function unescape_tags($wstr)
	{
		$wstr = str_replace('%', "\x81\x93", $wstr);  // replace special character by its full-width equivalent
		return strtr($wstr, array('^CRLF^' => '', '^EQ^' => '='));
	}
	
}

function alignupper($number)
{
	// want multiple of 16
	while (($number & 0x0F) != 0)
	{
		++$number;
	}
	return $number;
}


function buildfcb($bin)
{
	$fileout = fopen("40buildedflow/$bin.raw",'wb');
	$tl = new TLmgr("30insertedflow/$bin.afcb.txt");

	$dat = file_get_contents("30insertedflow/$bin.afcb");
	$count = current(unpack("V", substr($dat, -4,4)));
	
	$strings = '';
	
	for($id = 1; $id<=$count; ++$id)
	{
		$wstr = $tl->get($id);
		if(strlen($wstr) > 127)
		{
			fprintf(STDERR, "Found string #%d: invalid length, greater than 127 in %s\r\n", $id, "30insertedflow/$bin.txt");
			return;
		}
		$strings .= chr( strlen($wstr) ). $wstr;
	}
	
	$pktlen = strlen($dat) + strlen($strings);
	$alignedpktlen = alignupper($pktlen);
	$tail = ($alignedpktlen != $pktlen) ? str_repeat("\x00",$alignedpktlen-$pktlen) : '';
	
	fwrite($fileout,$dat);
	//fine even if empty
	fwrite($fileout,$strings);
	fwrite($fileout,$tail);
	fclose($fileout);
}

@mkdir('40buildedflow');
$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';
if (strlen($script))
{
	var_dump($script);
	buildfcb($script);
}
else
{
	$dir = opendir('30insertedflow');
	while($file = readdir($dir))
	{
		if($file!='.' && $file!='..')
		{
			$info = pathinfo(basename($file));
			if($info['extension'] === 'afcb')
			{
				$file = $info['filename'];
				buildfcb($file);
			}
		}
	}
	closedir($dir);
}
?>
