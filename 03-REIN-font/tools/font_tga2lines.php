<?php

$REVERSEPALETTE = array(
	"\xFF\xFF\xFF\x00" => 0x00,
	"\xFF\xFF\xFF\x33" => 0x01,
	"\xFF\xFF\xFF\x55" => 0x02,
	"\xFF\xFF\xFF\x77" => 0x03,
	"\xFF\xFF\xFF\x99" => 0x04,
	"\xFF\xFF\xFF\xBB" => 0x05,
	"\xFF\xFF\xFF\xDD" => 0x06,
	"\xFF\xFF\xFF\xFF" => 0x07,
);

@mkdir('40buildedfont');

$script = (isset($argv) && count($argv)>1) ? $argv[1] : '';
if (strlen($script))
{
	$info = pathinfo($argv[1]);

	$fp = fopen($script, 'rb');
	if(!$fp) exit(1);
	
	$name = strlen($argv[2])? $argv[2] : $info['filename'];

	switch ($name)
	{
	case 'font2008':
		// Settings for 2008 (20px, 10px-wide, 8-bit chars)
		$NUMCOLS=10;
		$NUMROWS=20;
		$BYTESPERROW=5;
		$TGATILESIZE=$NUMCOLS*$NUMROWS*4;  // bgra
		$NUMTILES=0x5F;  // 0x20 to 0x7E
		break;

	case 'font2016':
		// Settings for 2016 (20px, 22px-wide, high range)
		$NUMCOLS=20;
		$NUMROWS=20;
		$BYTESPERROW=10;
		$TGATILESIZE=$NUMCOLS*$NUMROWS*4;  // bgra
		$NUMTILES=6975;  // 0x813F to ...
		break;

	case 'font2208':
		// Settings for 2208 (22px, 11px-wide, 8-bit chars)
		$NUMCOLS=11;
		$NUMROWS=22;
		$BYTESPERROW=6;
		$TGATILESIZE=$NUMCOLS*$NUMROWS*4;  // bgra
		$NUMTILES=0x5F;  // 0x20 to 0x7E
		break;

	case 'font2216':
		// Settings for 2216 (22px, 22px-wide, high range)
		$NUMCOLS=22;
		$NUMROWS=22;
		$BYTESPERROW=11;
		$TGATILESIZE=$NUMCOLS*$NUMROWS*4;  // bgra
		$NUMTILES=6975;  // 0x813F to ...
		break;

	default:
        error_log("Unrecognised font, must be one of 'font2008', 'font2016', 'font2208', 'font2216'");
		exit(2);
	}
	$PADDINGX=str_repeat("\x00", $NUMROWS * $BYTESPERROW - 0x10);


	$tgaheader = unpack("cimage_id_len/ccolor_map_type/cimage_type/vcolor_map_origin/vcolor_map_len/" .
	  "ccolor_map_entry_size/vx_origin/vy_origin/vwidth/vheight/" .
	  "cpixel_size/cdescriptor", fread($fp, 0x12));

	if ($tgaheader['image_type'] != 2) // no palette, no RLE
	{
		error_log("Input TGA must be uncompressed TGA with no palette.");
		exit(3);
	}
    if ($tgaheader['pixel_size'] != 32)
	{
		error_log("Input TGA must be 32bpp (RGB+alpha)");
		exit(3);
	}
	$reverse = (($tgaheader['descriptor'] & 0x20) == 0);
	if ($reverse)
		fprintf(STDERR, "Info: Reading input TGA bottom-to-top byte layout - untested\n");

    // Generate TGA bitmap
	$binpath = sprintf("40buildedfont/%s.2bt", $info['filename']);
	$HEADER=pack('V', $NUMTILES + 1). pack('v', $BYTESPERROW)."\x00\x80". pack('V', $NUMCOLS). pack('V', $NUMROWS);
	file_put_contents($binpath, $HEADER);

	for ( $tilescount = 0; $tilescount < $NUMTILES; ++$tilescount )
	{
		if ($reverse) {
			$tileoffset = 0x12 + ($NUMTILES - 1 - $tilescount) * $TGATILESIZE;
		} else {
			$tileoffset = 0x12 + ($tilescount) * $TGATILESIZE;
		}
		fseek($fp, $tileoffset);
		
		$datatile = fread($fp, $NUMROWS * $NUMCOLS * 4);

		for ($irow = 0; $irow < $NUMROWS; ++$irow)
		{
			$icalc = $reverse? $NUMROW - 1 - $irow : $irow;
			$nibbles = array();
			$rowbase = $icalc * $NUMCOLS * 4;
			for ($icol = 0; $icol < $NUMCOLS; ++$icol)
			{
				$px = substr($datatile, $rowbase + $icol * 4, 4);
				$nibbles[] = $REVERSEPALETTE[ $px ];
			}
			if ((count($nibbles) & 0x01) == 0x01)
			{
				$nibbles[] = 0;
			}
			$tmp = array();
			for ($ipair = 0; $ipair < $BYTESPERROW; ++$ipair)
			{
				$tmp[] = chr(($nibbles[2*$ipair] << 0x04) | ($nibbles[2*$ipair+1]));
			}
			file_put_contents($binpath, implode('', $tmp), FILE_APPEND);
		}
	}
	fclose($fp);

	file_put_contents($binpath, $PADDINGX, FILE_APPEND);
}

?>