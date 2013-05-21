<?php

define ("MAXLENGTH", 45);
# Only do this for ascii characters
function addSpaces($str) {

    $strArr = str_split($str);
    if (ord($strArr[0]) == 129) {
	return $str;
    }
    
    $newStr = "";
    $curBytes = 0;
    $skipSpace = false;

    $words = explode(" ", $str);
    
    foreach($words as $word) {
	$nextWordBytes = strlen($word);
	if ($curBytes + $nextWordBytes > MAXLENGTH) {
	    $numSpaces = MAXLENGTH - $curBytes;
	    for ($i = 0; $i < $numSpaces; $i++) {
		$newStr .= " ";
	    }
	    //Width is 48, need 3 extra spaces
	    $newStr .= "   ";

	    //Reset stuff
	    $curBytes = 0;
	} elseif ($curBytes + $nextWordBytes == MAXLENGTH) {
	    $skipSpace = true;
	}
	
	$curBytes += $nextWordBytes;
	$newStr .= $word;

	// Add space after words
	if ($skipSpace) {
	    $skipSpace = false;
	} else {
	    $newStr .= " ";
	    $curBytes++;
	}
    }
#    print "\n\nOld: $str\n\n\nNew: $newStr";
 return $newStr;
}
function hexstring2binarry($str,$e = false)
{
	$out = '';
	$len = strlen($str);
	
	for($i = 0;$i < $len;$i+=2)
	{
		$out .= chr(hexdec(substr($str,$i,2)));
	}

	return $out;
}

function dechex_symbols($bin,$symb)
{
	$nulls = '';
	$symb--;
	while($symb)
	{
		$ifnum = pow(2,$symb*4);
		if($bin<$ifnum)
			$nulls .= '0';
		else
			break;
		$symb--;
	}
	return $nulls . dechex($bin);
}

function getbyten($var,$n)
{
	$ff = 0xFF << (($n - 1)*8);
	$var = $var & $ff;
	return $var >> (($n - 1)*8);
}

function get2b($int2b)
{
	return chr(($int2b & 0xFF)) . chr((($int2b >> 8) & 0xFF));
}

function get2l($int2l)
{
	return chr((($int2l >> 8) & 0xFF)) . chr(($int2l & 0xFF));
}

function get4b($int4b)
{
	return chr(($int4b & 0xFF)) . chr((($int4b >> 8) & 0xFF)) . chr((($int4b >> 16) & 0xFF)) . chr((($int4b >> 24) & 0xFF));
}


function tagtranslate($wstr)
{
	while(($pos = strpos($wstr,'[TAG')) !== false)
	{
		$wstr = substr($wstr,0,$pos) . chr(0x23) . chr(0x63) . hexstring2binarry(substr($wstr,$pos+4,4)) . substr($wstr,$pos+9);
	}
	return $wstr;
}


class TLmgr
{
	public function __construct($path)
	{
		$this->original = array();
		$this->translation = array();
		if (file_exists($path))
			foreach(file($path) as $line)
			{
				if(preg_match('/^<[^:]+:(?P<id>[^:>]+)(:[^>]+)?>[\\s]*(?P<org>[^=]+)=(?P<tl>[^\\r\\n]*)/', $line, $matches))
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
		$wstr = preg_replace('/\\[TAG([^\\]]+)]/e', '"#c". hexstring2binarry("$1")', $wstr);
		return strtr($wstr, array('[CR]^CRLF^' => '#cr0', '^CRLF^' => '', '[CR]' => '#cr0', '^EQ^' => '=' ));
	}
	
}

function alignupper($number)
{
	// want multiple of 4
	while (($number & 0x03) != 0)
	{
		++$number;
	}
	return $number;
}


function buidscript($bin)
{
	global $GLOSSARYEX;

	$fileout = fopen("40buildedscript/$bin",'wb');

	$codepageout = null;
	$codepagein = null;

	$tl = new TLmgr("30insertedscript/$bin.l10n.txt");
	$addr = 0;
	$defined_labels = array();
	
	$byteout = array();
	$seen_eof = FALSE;
	$needsfix = array();
	$broken = FALSE;

	foreach(file("30insertedscript/$bin.script") as $srcline => $str)
	{
		++$srcline;
		$str = ltrim(strtr($str,array("\r"=>'',"\n"=>'')));
		if(strlen($str) < 3 || substr($str,0,2) === '//')
			continue;

		if(preg_match('/^([^:]+):$/', $str, $matches))
		{
			//printf("Define $matches[1]\r\n");
			$defined_labels[$matches[1]] = $addr;
		}

		if($seen_eof)
		{
			fprintf(STDERR, "Found instruction past EOF in %s:%d - ignoring\r\n", "30insertedscript/$bin.script", $srcline);
			break;
		}

		$do = substr($str,0,3);
		switch($do)
		{
		case '[EO':
			$seen_eof = TRUE;

			$byteout[$addr] = "\x1A";
			$addr += strlen($byteout[$addr]);
			break;

		case '[P]':
			$args = explode(',', substr($str,3));
			$blob = hexstring2binarry(array_shift($args));
			$blob .= get2b(intval(array_shift($args)));
			if(count($args))
			{
				$blob .= hexstring2binarry(array_shift($args));
			}
			$byteout[$addr] = $blob;
			$addr += strlen($byteout[$addr]);
			break;

		case '[N]':
		case '[S]':
			$id = intval(substr($str,3));
			$wstr = $tl->get($id);
			//$wstr = addSpaces($wstr);
			
			if(strlen($wstr) > 4 && substr($wstr,0,2) === "\x81\x79")
			{
				$wstr = substr($wstr,2,-2);
			}
#			print "\n\n wstr: $wstr " . strlen($wstr);
			if(strlen($wstr) >= 0x102)
			{
				fprintf(STDERR, "ERR: [S] instruction at %s:%d message string is too long - truncating to 0x101 (you should manually *split*)\r\n", "$bin.script", $srcline);
				$wstr = substr($wstr,0,0x101);
			}
			
			$subpktlen = strlen($wstr);
			$pktlen = 6 + (2 + $subpktlen);
			$endpos = $addr + $pktlen;
			$alignedendpos = alignupper($endpos);
			$alignedppktlen = $pktlen + $alignedendpos - $endpos;
			
			$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';
			
			$byteout[$addr] = 
				"\x03\x1D".
				get2b($alignedppktlen).
				"\x00\x00".
				($do == '[S]'? "\x00" : "\x01").
				"\x00\x00\x00".
				get2b($subpktlen).
				$wstr.
				$tail;
			$addr += strlen($byteout[$addr]);
			break;

		case '[M]':
			$var1 = explode(':', substr($str,3));
			$choices = explode(',', $var1[1]);
			
			$pktlen = 0x8;  // things past initlen
			$subpkts = array();
			
			foreach ($choices as $choiceid)
			{
				$id = intval($choiceid);
				$wstr = $tl->get($id);
				$subpktlen = strlen($wstr);
				$pktlen += (2 + $subpktlen);
				$subpkts[] = get2b($subpktlen).$wstr;
			}
			$endpos = $addr + $pktlen;
			$alignedendpos = alignupper($endpos);
			$alignedppktlen = $pktlen + $alignedendpos - $endpos;

			$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';

			$byteout[$addr] = 
				"\x03\x1F".
				get2b($alignedppktlen).
				"\x00\x00".
				get2b($var1[0]).
				"\x00\x00".
				get2b(count($choices)).
				implode('',$subpkts).
				$tail;

			$addr += strlen($byteout[$addr]);
			break;

		case '[F]':
			$id = intval(substr($str,3));
			$wstr = $tl->get($id);
			
			$subpktlen = strlen($wstr);
			$pktlen = (2 + $subpktlen);
			$endpos = $addr + $pktlen;
			$alignedendpos = alignupper($endpos);
			$alignedppktlen = $pktlen + $alignedendpos - $endpos;
			
			$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';
			
			$byteout[$addr] = 
				"\x0E\x02".
				get2b($alignedppktlen).
				get2b($subpktlen).
				$wstr.
				$tail;
			$addr += strlen($byteout[$addr]);
			break;

		case '[G]':
			$wstr = array_shift(explode('"', substr($str,4)));
			$id = array_search($wstr, $GLOSSARYEX, TRUE);
			if($id===FALSE)
			{
				fprintf(STDERR, "ERR: [G] instruction at %s:%d refers to undefined glossary term - breaking\r\n", "$bin.script", $srcline);
				$broken = TRUE;
				goto Lbl_end_foreach;
			}
			
			//1200 followed by 0C0B
			$pktlen = 12;
			$endpos = $addr + $pktlen;
			$alignedendpos = alignupper($endpos);
			$alignedppktlen = $pktlen + $alignedendpos - $endpos;

			$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';

			$byteout[$addr] = "\x12\x00".
				get2b($alignedppktlen).
				"\x24\x77\x00\x00\x00\x00".
				"\x00\x00". get4b($id).
				$tail;
			$addr += strlen($byteout[$addr]);


			$pktlen = 12;
			$endpos = $addr + $pktlen;
			$alignedendpos = alignupper($endpos);
			$alignedppktlen = $pktlen + $alignedendpos - $endpos;

			$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';

			$byteout[$addr] = "\x0C\x0B".
				get2b($alignedppktlen).
				get4b(0).
				get4b(0).
				get4b(0xD).
				$tail;
			$addr += strlen($byteout[$addr]);
			break;

		case '[J]':
			static $asmcmds = array(
				'0C01-ToLabel' => 0x0C01,
				'0C02-GoSub' => 0x0C02,
				'Return' => 0x0C03,
				'0C04-ExecScript' => 0x0C04,
				'0C05-CallScript' => 0x0C05,
				'EndSub' => 0x0C06,
				'SetOptScript' => 0x0C0C,
				'SetTitleScript' => 0x0C0D,

				'JumpNE' => 0x120C,
				'JumpEQ' => 0x120D,
				'JumpGE' => 0x120E,
				'JumpGT' => 0x120F,
				'JumpLE' => 0x1210,
				'JumpLT' => 0x1211,
				'12-JumpCond-LE' => 0x1212,
				'12-JumpCond-GT' => 0x1213,
				
				'multi' => 0x1214,
			);
			$args = explode(',', substr($str,3));
			$blob = '';
			
			switch($asmcmd = $asmcmds[array_shift($args)])
			{
				case 0x0C01:
				case 0x0C02:
					$blob .= get2l($asmcmd).
						"\x04\x00";
					if(isset($defined_labels[$args[0]]))
					{
						$blob .= get4b($defined_labels[$args[0]]);
					}
					else
					{
						$needsfix[$addr] = array(
							0x04 => $args[0]
						);
						$blob .= get4b(0xC0DE5005);
					}
					break;

				case 0x0C03:
					$blob .= "\x0C\x03\x00\x00";
					break;

				case 0x0C04:
				case 0x0C05:
				case 0x0C0C:
				case 0x0C0D:
					$scriptid = sprintf('%d', intval(array_shift($args)));

					$subpktlen = strlen($scriptid);
					$pktlen = (2 + $subpktlen);
					$endpos = $addr + $pktlen;
					$alignedendpos = alignupper($endpos);
					$alignedppktlen = $pktlen + $alignedendpos - $endpos;
					
					$tail = ($alignedppktlen != $pktlen) ? str_repeat("\x00",$alignedppktlen-$pktlen) : '';
					
					$blob .= get2l($asmcmd);
					$blob .= get2b($alignedppktlen).get2b($subpktlen).$scriptid.$tail;
					break;

				case 0x0C06:
					$blob .= "\x0C\x06\x00\x00";
					break;
					
				case 0x120C:
				case 0x120D:
				case 0x120E:
				case 0x120F:
				case 0x1210:
				case 0x1211:
				case 0x1212:
				case 0x1213:
					$varcmp = explode('_', $args[0]);
					$varcmp2 = explode('_', $args[1]);
					if(count($varcmp2)==1)
					{
						array_unshift($varcmp2, "\x00\x00");
					}

					$blob .= get2l($asmcmd);
					$blob .= get2b(16).
						$varcmp[0].get2b(intval($varcmp[1])).
						"\x00\x00".
						$varcmp2[0].get2b(intval($varcmp2[1])).
						"\x00\x00";
					if(isset($defined_labels[$args[2]]))
					{
						$blob .= get4b($defined_labels[$args[2]]);
					}
					else
					{
						$needsfix[$addr] = array(
							0x10 => $args[2]
						);
						$blob .= get4b(0xC0DE5005);
					}
					break;

				case 0x1214:
					$blob .= get2l($asmcmd);
					$blob .= get2b(0x8 + 0x4*count($args)).
					"\x24\x77\x04\x00\x00\x00".
					get2b(count($args));
					
					$offset_offset = 0x0C;
					
					foreach($args as $label)
					{
						if(isset($defined_labels[$label]))
						{
							$blob .= get4b($defined_labels[$label]);
						}
						else
						{
							$needsfix[$addr][$offset_offset] = $label;
							$blob .= get4b(0xC0DE5005);
						}
						$offset_offset += 0x4;
					}
//var_dump($needsfix[$addr]);
					break;
					
				default:
					fprintf(STDERR, "ERR: Unhandled [J] instruction at %s:%d - breaking\r\n", "$bin.script", $srcline);
					$broken = TRUE;
					goto Lbl_end_foreach;
				
			}  // end switch for '[J]'

			if ($blob != '')
			{
				$byteout[$addr] = $blob;
				$addr += strlen($byteout[$addr]);
			}
			break;
		}  // end switch '$do'
	}  // end foreach
	
	Lbl_end_foreach:

	foreach($needsfix as $addr => $patch)
	{
		$str = $byteout[$addr];
		foreach($patch as $offset => $label)
		{
			if(!isset($defined_labels[$label]) && $broken == FALSE)
			{
				fprintf(STDERR, "ERR: jump to unreferenced %s in %s\r\n", $label, "$bin.script");
			}
			else
			{
				$str = $byteout[$addr] = substr($str,0,$offset). get4b($defined_labels[$label]).substr($str,$offset+4);
			}
		}
	}
	
	fwrite($fileout,implode('',$byteout));
	fclose($fileout);

	//var_dump($defined_labels);

}
#addSpaces("test test test test test test test test testaaaaaaaaa");

#exit(1);

$GLOSSARYEX = array_map('trim', file('40buildedglossary/allentries-index.txt'));


@mkdir('40buildedscript');
$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';
if (strlen($script))
{
	var_dump($script);
	buidscript($script);
}
else
{
	$dir = opendir('30insertedscript');
	while($file = readdir($dir))
	{
		if($file!='.' && $file!='..')
		{
			$info = pathinfo(basename($file));
			if($info['extension'] === 'script')
			{
				$file = $info['filename'];
				buidscript($file);
			}
		}
	}
	closedir($dir);
}
?>
