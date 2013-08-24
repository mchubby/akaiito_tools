<?php

$PALETTE = array(
	//0 => "\xFF\xFF\xFF\x00",
	0 => "\x00\x00\x00\x00",
	1 => "\xFF\xFF\xFF\x33",
	2 => "\xFF\xFF\xFF\x55",
	3 => "\xFF\xFF\xFF\x77",
	4 => "\xFF\xFF\xFF\x99",
	5 => "\xFF\xFF\xFF\xBB",
	6 => "\xFF\xFF\xFF\xDD",
	7 => "\xFF\xFF\xFF\xFF",
);

@mkdir('20decodedfont');

$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';
if (strlen($script))
{
	$info = pathinfo(basename($argv[1]));
	
	$fp = fopen($script, 'rb');
	if(!$fp) exit(1);
	
	$NUMTILES=current(unpack('V',fread($fp,4))) - 1;
	$BYTESPERROW=current(unpack('v',fread($fp,4)));
	$NUMCOLS=current(unpack('V',fread($fp,4)));
	$NUMROWS=current(unpack('V',fread($fp,4)));
	$TILESIZE=$NUMROWS*$BYTESPERROW;

	switch ($info['filename'])
	{
	case 'font2008':
		// Settings for 2008 (20px, 10px-wide, 8-bit chars)
		//$NUMCOLS=10;
		//$NUMROWS=20;
		//$BYTESPERROW=5;
		//$TILESIZE=20*5;  // 10 half-bytes in 5 bytes
		//$NUMTILES=0x5F;  // 0x20 to 0x7E
		break;

	case 'font2016':
		// Settings for 2016 (20px, 22px-wide, high range)
		//$NUMCOLS=20;
		//$NUMROWS=20;
		//$BYTESPERROW=10;
		//$TILESIZE=20*10;  // 20 half-bytes in 10 bytes
		//$NUMTILES=6975;  // 0x813F to ...
		break;

	case 'font2208':
		// Settings for 2208 (22px, 11px-wide, 8-bit chars)
		//$NUMCOLS=11;
		//$NUMROWS=22;
		//$BYTESPERROW=6;
		//$TILESIZE=22*6;  // 11 half-bytes in 6 bytes
		//$NUMTILES=0x5F;  // 0x20 to 0x7E
		break;

	case 'font2216':
		// Settings for 2216 (22px, 22px-wide, high range)
		//$NUMCOLS=22;
		//$NUMROWS=22;
		//$BYTESPERROW=11;
		//$TILESIZE=22*11;  // 22 half-bytes in 11 bytes
		//$NUMTILES=6975;  // 0x813F to ...
		break;

	case 'font2408':
		// Settings for 2408 (24px, 12px-wide, 8-bit chars)
		//$NUMCOLS=12;
		//$NUMROWS=24;
		//$BYTESPERROW=6;
		//$TILESIZE=24*6;  // 12 half-bytes in 6 bytes
		//$NUMTILES=0x5F;  // 0x20 to 0x7E
		break;

	case 'font2416':
		// Settings for 2416 (24px, 24px-wide, high range)
		//$NUMCOLS=24;
		//$NUMROWS=24;
		//$BYTESPERROW=12;
		//$TILESIZE=24*12;  // 24 half-bytes in 12 bytes
		//$NUMTILES=6975;  // 0x813F to ...
		break;

	default:
		exit(2);
	}

    // Generate TGA bitmap
	$binpath = sprintf("20decodedfont/%s.tga", $info['filename']);
	$bin = "\x00";	//idLength           // Length of optional identification sequence.
	$bin .= "\x00";	//paletteType        // Is a palette present? (1=yes)
	$bin .= "\x02";	//imageType          // Image data type (0=none, 1=indexed, 2=rgb, 3=grey, +8=rle packed).
	$bin .= "\x00\x00";//firstPaletteEntry  // First palette index, if present.
	$bin .= "\x00\x00";//numPaletteEntries  // Number of palette entries, if present.
	$bin .= "\x00";	//paletteBits        // Number of bits per palette entry.
	$bin .= "\x00\x00";//x                  // Horiz. pixel coord. of lower left of image.
	$bin .= "\x00\x00";//y                  // Vert. pixel coord. of lower left of image.
	//$bin .= "\x0B\x00";//width              // Image width in pixels.
	$bin .= pack('v', $NUMCOLS);
	//$bin .= "\x16\x00";//height             // Image height in pixels.
	$bin .= pack('v', $NUMROWS * ($NUMTILES));
	$bin .= "\x20";	//depth              // Image color depth (bits per pixel).
	$bin .= "\x28";	//descriptor         // Image attribute flags.
	file_put_contents($binpath, $bin);

	for ( $tilescount = 0; $tilescount < $NUMTILES; ++$tilescount )
	{
		fseek($fp, 0x10 + ($tilescount) * $TILESIZE);
		
		$rowsbin = array();

		for ($irow = 0; $irow < $NUMROWS; ++$irow)
		{
			$rowsbin[$irow] = array();

			$nibbles = array();
			for ($inibb = 0; $inibb < $BYTESPERROW; ++$inibb)
			{
				$b = ord(fread($fp,1));
//var_dump(sprintf('%04X', ftell($fp)));
				$nibbles[] = ($b >> 4);
				$nibbles[] = ($b & 0xF);
			}
			
			for ($icol = 0; $icol < $NUMCOLS; ++$icol)
			{
//It really is 3bpp
//if($nibbles[$icol] > 7) { var_dump($nibbles[$icol]); exit;}
				$rowsbin[$irow][] = $PALETTE[ $nibbles[$icol]  ];
			}
			$rowsbin[$irow] = implode('', $rowsbin[$irow]);
		}
		file_put_contents($binpath, implode('', $rowsbin), FILE_APPEND);
	}

//var_dump(sprintf('%04X', ftell($fp)));
	fclose($fp);
}

