<?php

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

function read2b($file)
{
	$bo1 = (ord(fread($file,1)));
	$bo2 = (ord(fread($file,1))) << 8;
	return $bo1 | $bo2;
}

function read4b($file)
{
	$bo1 = read2b($file);
	$bo2 = read2b($file) << 16;
	return $bo1 | $bo2;
}

function readline($file,$len)
{
	$out = '';
	while($len>0)
	{
		$byte = ord(fread($file,1));
		$len--;
		if($byte==0x23)  // '#'
		{
			$byte2 = ord(fread($file,1));
			if($byte2==0x63)  // 'c'
			{
				$tag = dechex_symbols((ord(fread($file,1))),2) . dechex_symbols((ord(fread($file,1))),2);
				if($tag=='7230')
					$out .= '[CR]^CRLF^';
				else
					$out .= "[TAG$tag]";
				$len -= 3;
			}
			else
			{
				fprintf(STDERR, "Extended tag (other than #cr0 #c..) used at %08X\r\n", ftell($file) - 2);
				$out .= chr($byte). chr($byte2);
				$len--;
			}
		}
		elseif($byte==ord('='))
		{
			$out .= '^EQ^';
		}
		else
		{
			// read 1 byte, do not assume sjis text
			$out .= chr($byte);
		}
	}
	return $out;
}


function handle_generic_packet($file, $addr, $byte, $byte2, &$byteout, &$lineout)
{
	$initlen = read2b($file);
	
	assert(ftell($file) + $initlen < 1024 * 1024);
	
	$pktgen = sprintf('[P]%02X%02X,%d', $byte, $byte2, $initlen);
	$extra = '';
	
	if ($initlen > 0)
	{
		$extra .= ',';
		while ($initlen-- > 0)
		{
			$extra .= dechex_symbols(ord(fread($file,1)),2);
		}
	}
	$byteout[$addr] = "$pktgen$extra\r\n";
}

function add_labels($addrs, $defined_labels, &$byteout, &$lineout)
{
	$unused_labels = $defined_labels;
	foreach ($addrs as $addr)
	{
		if(isset($defined_labels[$addr]))
		{
			unset($unused_labels[$addr]);
			$labels_here = $defined_labels[$addr];
			$labelscr = '';
			foreach($labels_here as $label)
			{
				$labelscr .= "$label:\r\n";
				$lineout[$addr] = "// #----- $label -----# //\r\n". $lineout[$addr];
			}

			$byteout[$addr] = "\r\n$labelscr". $byteout[$addr];
		}
	}
	if (count($unused_labels))
	{
		fprintf(STDERR, "There are %d unreachable labels\r\n", count($unused_labels));
		var_dump($unused_labels);
	}
}

function decodescript($bin)
{
	global $AFSFILES;
	global $GLOSSARY;
	global $GLOSSARYREAD;

	$file = fopen('10rawscript/'.$bin,'rb');
	$size = filesize('10rawscript/'.$bin);

	$byteout = array();
	$lineout = array();
	$addrs = array();
	$defined_labels = array();

	$linerescount = 0;
	$jmplabelcount = 0;
	$latest_multi = array();
	$has_locstr = FALSE;

	unset($varmod_addr);
	
	while(ftell($file)<$size)
	{
		$addrs[] = $addr = ftell($file);
		$byte = ord(fread($file,1));

		if($byte==0x1A)
		{
			assert(ftell($file)>=$size);
			$byteout[$addr] = '[EOF]';
		}
		elseif($byte==0x03)
		{
			$byte2 = ord(fread($file,1));
			if($byte2 == 0x1D)
			{
// Handle: dialogue
				$initlen = read2b($file);
				fseek($file,2,SEEK_CUR);
				$type = ord(fread($file,1));
				fseek($file,3,SEEK_CUR);
				$len = read2b($file);
				$line = readline($file,$len);
				$idx = ++$linerescount;

				if($type)
				{
					$type = '[N]';

					$typeline1 = "\x81\x79";
					$typeline2 = "\x81\x7A";
				}
				else
				{
					$type = '[S]';

					$typeline1 = '';
					$typeline2 = '';
				}
				$has_locstr = TRUE;
				$lineout[$addr] = sprintf("<DLG:%d>%s=", $idx, "$typeline1$line$typeline2");
				$byteout[$addr] = "$type$idx\r\n";
				fseek($file,$initlen - $len - 8,SEEK_CUR);
			}
			elseif($byte2 == 0x1F)
			{
// Handle: multi-choice
				$initlen = read2b($file);
				$endpos = ftell($file) + $initlen;
				fseek($file,2,SEEK_CUR);
				$numfoo = read2b($file);
				fseek($file,2,SEEK_CUR);
				$numstr = read2b($file);
				$numchoice = 1;
				$choices = array();

				$cmt = "// Multi:\r\n";

				$items = array();
				$items[] = "\r\n// --- Begin Multi-choice Selection";
				
				while($numstr > 0)
				{
					$len = read2b($file);
					$line = readline($file,$len);
					$idx = ++$linerescount;
					$items[] = sprintf("<SEL:%d#%02d>%s=", $idx, $numchoice, $line);
					$cmt .= '// - '. $line. "\r\n";
					$choices[] = $idx;
					$remng_byt -= (2 + $len);
					--$numstr;
					++$numchoice;
				}
				$latest_multi = $choices;

				$items[] = "// --- End   Multi-choice Selection";

				$remainseek = $endpos - ftell($file);
				if($remainseek > 0)
				{
					fseek($file,$remainseek,SEEK_CUR);
				}

				$choices = sprintf('[M]%d:%s', $numfoo, implode(',', $choices));

				$has_locstr = TRUE;
				$lineout[$addr] = implode("\r\n", $items);
				$byteout[$addr] = "$cmt$choices\r\n";
			}
			else
			{
// Handle: genpkt
				handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
			}
		}
		elseif($byte==0x0C)
		{
			$byte2 = ord(fread($file,1));
			$cmds = array(
				0x01 => '0C01-ToLabel',
				0x02 => '0C02-GoSub',
			);
			if(isset($cmds[$byte2]))
			{
// Handle: ToLabel, GoSub
				$initlen = read2b($file);
				$endpos = ftell($file) + $initlen;
				$offset = read4b($file);
						
				if(isset($defined_labels[$offset]))
				{
					$label = '';
					// do not generate a label if a similar one is present
					foreach($defined_labels[$offset] as $value)
					{
						if(strpos($value,'InnLabel')!==FALSE)
						{
							$label = $value;
							break;
						}
					}
					if($label === '')
					{
						$label = sprintf("InnLabel_%02d", ++$jmplabelcount);
						$defined_labels[$offset][] = $label;
					}
				}
				else
				{
					$label = sprintf("InnLabel_%02d", ++$jmplabelcount);
					$defined_labels[$offset] = array($label);
				}

				$cmt = '';
				if($offset < ftell($file))
				{
					$cmt = sprintf("// %s is earlier^^ location of script\r\n", $label);
				}
				
				$remainseek = $endpos - ftell($file);
				if($remainseek > 0)
				{
					fseek($file,$remainseek,SEEK_CUR);
				}
				
				$scriptstr = sprintf("[J]%s,%s\r\n",$cmds[$byte2],$label);

				$lineout[$addr] = sprintf("// ~~%s %s",$cmds[$byte2],$label);
				$byteout[$addr] =  "$cmt$scriptstr\r\n";
			}
			elseif($byte2 == 0x03)
			{
// Handle: Return
				if (0 < ($initlen = read2b($file)))
				{
					fread($file,$initlen);
				}
				$byteout[$addr] = "[J]Return\r\n";
			}
			elseif($byte2 == 0x04 || $byte2 == 0x05)
			{
				$cmds = array(
					0x04 => '0C04-ExecScript',
					0x05 => '0C05-CallScript',
				);
// Handle: ExecScript,CallScript
				$initlen = read2b($file);
				$endpos = ftell($file) + $initlen;
				$len = read2b($file);
				$line = readline($file,$len);
				
				$remainseek = $endpos - ftell($file);
				if($remainseek > 0)
				{
					fseek($file,$remainseek,SEEK_CUR);
				}

				$scriptname = $AFSFILES[intval($line)];
				$cmt = sprintf("// ^ %s to %s\r\n",$cmds[$byte2],$scriptname);
				
				// non localizable string
				$scriptstr = sprintf("[J]%s,%s\r\n",$cmds[$byte2],$line);

				$lineout[$addr] = "// Open script: $scriptname";
				$byteout[$addr] = "$cmt$scriptstr\r\n";
			}
			elseif($byte2 == 0x06)
			{
// Handle: EndSub
				if (0 < ($initlen = read2b($file)))
				{
					fread($file,$initlen);
				}
				$byteout[$addr] = "[J]EndSub\r\n";
			}
			elseif($byte2 == 0x0C || $byte2 == 0x0D)
			{
				$cmds = array(
					0x0C => 'SetOptScript',
					0x0D => 'SetTitleScript',
				);
// Handle: SetOptScript,SetTitleScript
				$initlen = read2b($file);
				$endpos = ftell($file) + $initlen;
				$len = read2b($file);
				$line = readline($file,$len);
				
				$remainseek = $endpos - ftell($file);
				if($remainseek > 0)
				{
					fseek($file,$remainseek,SEEK_CUR);
				}
				
				$scriptname = $AFSFILES[intval($line)];
				$cmt = sprintf("// %s to %s\r\n",$cmds[$byte2],$scriptname);

				$scriptstr = sprintf("[J]%s,%s",$cmds[$byte2],$line);

				$lineout[$addr] = "// Scriptref: $scriptname";
				$byteout[$addr] = "$cmt$scriptstr\r\n";
			}
			else
			{
// Handle: genpkt
				handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
			}
		}
		elseif($byte==0x0E)
		{
			$byte2 = ord(fread($file,1));
			if($byte2 == 0x02)
			{
// Handle: flowchartstr
				$initlen = read2b($file);
				$endpos = ftell($file) + $initlen;
				$len = read2b($file);
				$line = readline($file,$len);
				$idx = ++$linerescount;

				$remainseek = $endpos - ftell($file);
				if($remainseek > 0)
				{
					fseek($file,$remainseek,SEEK_CUR);
				}

				$flowstr = "[F]$idx";
				
				$has_locstr = TRUE;
				$lineout[$addr] = sprintf("<FLO:%d>%s=", $idx, $line);
				$byteout[$addr] = "$flowstr\r\n";
			}
			else
			{
// Handle: genpkt
				handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
			}
		}
		elseif($byte==0x12)
		{
			$byte2 = ord(fread($file,1));
			$fpos = ftell($file);
			$ishandled = FALSE;
			if($byte2 == 0x00)
			{
				$initlen = read2b($file);
				assert(ftell($file) + $initlen < 1024 * 1024);
				if ($initlen > 0)
				{
					$tmpvardata = fread($file,$initlen);
				}
				if(ftell($file) + 13 <$size &&
				  (ord(fread($file,1)) == 0x0C && ord(fread($file,1)) == 0x0B)
				)
				{
					//1200 followed by 0C0B, check second packet
					$initlen = read2b($file);
					$endpos = ftell($file) + $initlen;
					$dword1 = read4b($file);
					$dword2 = read4b($file);
					$dword3 = read4b($file);
					
					if($dword1 == 0 && $dword2 == 0 && $dword3 == 0x0D)
					{
// Handle: SpecialFunc Glossary
						$ishandled = TRUE;
						$strid = current(unpack('V', substr($tmpvardata, -4, 4)));
						$scriptstr = sprintf('[G]"%s"',$GLOSSARY[$strid]);
						$cmt = '';
						if ($GLOSSARYREAD[$strid] != '--')
						{
							$cmt = '// - '. $GLOSSARYREAD[$strid]. "\r\n";
						}
						
						$byteout[$addr] = "$cmt$scriptstr\r\n";
						
						$remainseek = $endpos - ftell($file);
						if($remainseek > 0)
						{
							fseek($file,$remainseek,SEEK_CUR);
						}
					}
				}
			}
			if(!$ishandled)
			{
				// Let normal processing happen
				fseek($file,$fpos);

				$cmds = array(
					0x0C => 'JumpNE',
					0x0D => 'JumpEQ',
					0x0E => 'JumpGE',
					0x0F => 'JumpGT',
					0x10 => 'JumpLE',
					0x11 => 'JumpLT',
					0x12 => '12-JumpCond-LE',
					0x13 => '12-JumpCond-GT',
				);
				if(isset($cmds[$byte2]))
				{
// Handle: JumpNE,JumpEQ,JumpGE,JumpGT,JumpLE,JumpLT,12-JumpCond12,
					$initlen = read2b($file);
					$endpos = ftell($file) + $initlen;
					$what = fread($file,2);
					$what2 = read2b($file);
					fseek($file,0x2,SEEK_CUR);
					$whatrhs = fread($file,2);
					$whatrhs2 = sprintf("%d", read2b($file));
					if($whatrhs != "\x00\x00")
					{
						$whatrhs2 = $whatrhs.'_'.$whatrhs2;
					}
					fseek($file,0x2,SEEK_CUR);
					$offset = read4b($file);
							

					if(isset($defined_labels[$offset]))
					{
						$label = '';
						// do not generate a label if a similar one is present
						foreach($defined_labels[$offset] as $value)
						{
							if(strpos($value,'JmpLabel')!==FALSE)
							{
								$label = $value;
								break;
							}
						}
						if($label === '')
						{
							$label = sprintf("JmpLabel_%02d", ++$jmplabelcount);
							$defined_labels[$offset][] = $label;
						}
					}
					else
					{
						$label = sprintf("JmpLabel_%02d", ++$jmplabelcount);
						$defined_labels[$offset] = array($label);
					}
							
					$cmt = '';
					if($offset < ftell($file))
					{
						$cmt = sprintf("// %s is earlier^^ location of script\r\n", $label);
					}

					$remainseek = $endpos - ftell($file);
					if($remainseek > 0)
					{
						fseek($file,$remainseek,SEEK_CUR);
					}
					
					$scriptstr = sprintf("[J]%s,%s_%d,%s,%s\r\n",$cmds[$byte2],$what,$what2,$whatrhs2,$label);

					$lineout[$addr] = sprintf("// ~TT~ %s %s%d %s %s",$cmds[$byte2],$what,$what2,$whatrhs2,$label);
					$byteout[$addr] = "$cmt$scriptstr\r\n";
				}
				elseif($byte2 == 0x14)
				{

					if(count($latest_multi) == 0)
					{
// Handle: genpkt (in EDLST_JP.DAT)
						handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
					}
					else
					{
// Handle: switch
						$initlen = read2b($file);
						$endpos = ftell($file) + $initlen;
						fseek($file,6,SEEK_CUR);
						$branchcount = read2b($file);
						assert($branchcount == count($latest_multi));
						$tgtlabels = array();

						$items = array();
						$items[] = '// --- Branch from Multi';
						
						foreach($latest_multi as $multiid)
						{
							$offset = read4b($file);
							
							$label = sprintf("Case_%02d", $multiid);
							if(!isset($defined_labels[$offset]))
							{
								$defined_labels[$offset] = array($label);
							}
							else
							{
								$defined_labels[$offset][] = $label;
							}
							$tgtlabels[] = $label; 
							
							$items[] = sprintf("// ~~ To: %s", $label);
						}
						$items[] = "\r\n";

						$remainseek = $endpos - ftell($file);
						if($remainseek > 0)
						{
							fseek($file,$remainseek,SEEK_CUR);
						}

						$branchstr = '[J]multi,'. implode(',',$tgtlabels);
						
						$lineout[$addr] = implode("\r\n", $items);
						$byteout[$addr] = "$branchstr\r\n";
					}
				}
				else
				{
// Handle: genpkt
					handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
				}
			} // !$ishandled
		}
		else
		{
// note: movie playback is 0x0A .. (8,9) but trial exe does not bundle this feature.
			if($byte > 0x12)
			{
				fprintf(STDERR, "Stop: %s, unexpected byte %02X encountered at %08X\r\n", $bin, $byte, ftell($file) - 1);
				break;
			}
			$byte2 = ord(fread($file,1));
// Handle: genpkt
			handle_generic_packet($file, $addr, $byte, $byte2, $byteout, $lineout);
		}
	} // end of loop
	fclose($file);

	add_labels($addrs, $defined_labels, $byteout, $lineout);

	
	$bin = basename($bin);
	ksort($byteout);
	file_put_contents("20decodedscript/$bin.script",implode('',$byteout));
	if ($has_locstr)
	{
		ksort($lineout);
		file_put_contents("20decodedscript/$bin.l10n.txt",implode("\r\n",$lineout));
	}
} // end of function


$AFSFILES = array_map('trim', file('10rawscript/_filelist.txt'));
$GLOSSARY = array_map('trim', file('20decodedglossary/allentries.txt'));
$GLOSSARYREAD = array_map('trim', file('20decodedglossary/allentries-read.txt'));

@mkdir('20decodedscript');

$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';
if (strlen($script))
{
	var_dump($script);
	decodescript($script);
}
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
				decodescript($file);
			}
		}
	}
	closedir($dir);
}
?>