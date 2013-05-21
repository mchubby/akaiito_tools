#include <cstdio>

#include <string>
#include <fstream>
#include <iostream>

#include <windows.h>

#include "libcvm.h"

#include "cvmhead.bin.h"

#if defined(NEVER_DEFINED)
#include <cstddef> // __GLIBCXX__, _HAS_TR1
 
// GNU C++ or Intel C++ using libstd++.
#if defined (__GNUC__) && __GNUC__ >= 4 && defined (__GLIBCXX__)
#  include <tr1/memory>
//
// IBM XL C++.
#elif defined (__xlC__) && __xlC__ >= 0×0900
#  define __IBMCPP_TR1__
#  include <memory>
//
// VC++ or Intel C++ using VC++ standard library.
#elif defined (_MSC_VER) && (_MSC_VER == 1500 && defined (_HAS_TR1) || _MSC_VER > 1500)
#  include <memory>
using std::unique_ptr;
//
// Boost fall-back.
#else
#  include <boost/tr1/memory.hpp>
#endif
#endif

using std::string;




namespace {
	const char PRODUCT_CODE[] = "AKAIITO_DEMO";
	const size_t CDFS_SECTOR_SIZE = 0x800;

	inline void write4BE(BYTE *p, DWORD val)
	{
		DWORD shifts[4] = { 0x18, 0x10, 0x08, 0 };

		for ( int i = 0; i < 4; ++i)
			p[i] = static_cast<BYTE>((val >> shifts[i]) & 0xFF);
	}
};


void banner()
{
	string version = "cvmutil version 1.0";

	std::cerr << version << ", Copyright (C) 2011 Nanashi3." << std::endl
       << std::endl
       << "cvmutil comes with ABSOLUTELY NO WARRANTY." << std::endl
       << std::endl;
}

void usage()
{
	banner();

	std::cerr << "Usage: cvmutil [-e | -d] infile [outfile]" << std::endl
	   << "  -e\tencodes .iso to .cvm" << std::endl
	   << "  -d\tdecodes .cvm to .iso" << std::endl
	   << std::endl
	   << "If unspecified, name for outfile is automatically derived from infile" << std::endl
       << std::endl;
}




int main(int argc, char* argv[])
{
	string inpath, outpath;
	int mod_direction = 0;  // auto

	for (int iArg = 1; iArg < argc; ++iArg)
	{
		string thisarg(argv[iArg]);
		if( thisarg.length() > 1 && thisarg[0] == '-' )
		{
			switch(thisarg[1])
			{
			case 'e':
				mod_direction = 1;  // encode
				break;
			case 'd':
				mod_direction = -1;  // decode
				break;
			default:
				usage();
				exit(254);
			}
		}
		else
		{
			if (inpath.length() > 0 && outpath.length() > 0)
			{
				usage();
				exit(254);
			}
			string & argtgt = inpath.length() == 0 ? inpath : outpath;
			argtgt = thisarg;
		}
	}

	if (inpath.length() == 0)
	{
		usage();
		exit(254);
	}

	// read input data
	std::ifstream inArchive(inpath.c_str(), std::ios::binary | std::ios::ate);
	if (!inArchive)
	{
		fprintf(stderr, "Cannot open input path: %s\n", inpath.c_str());
		return 1;
	}

	// Put up some sensible limits
    std::fstream::pos_type insize = inArchive.tellg();
    inArchive.seekg(0);
	if (insize < 0x10000)
	{
		fprintf(stderr, "input file too small, unlikely to be an iso/cvm\n");
		return 1;
	}

	// Initialize xorIV
	BYTE xorIV[8] = {};
	CvmCreateXorIV(reinterpret_cast<const BYTE *const>(PRODUCT_CODE), -1, ::CvmGetXorLookupTable(), xorIV);
	//for ( size_t i = 0; i < 8; ++i)
	//{
	//	fprintf(stderr, "%02X", xorIV[i]);
	//}
	//fprintf(stderr, "\n");

	if (mod_direction == 0)
	{
		// Try to detect
		DWORD tag;
		size_t cvmOffset = 0;
		inArchive.seekg(cvmOffset);
		inArchive.read(reinterpret_cast<char *>(&tag), sizeof(DWORD));

		if (tag == 0x484d5643)  // "CVMH"
		{
			fprintf(stderr, "DECODING: auto-detected CVM header on input, outputting .iso\n");
			mod_direction = -1;
		}
		else
		{
			cvmOffset = 0x10 * CDFS_SECTOR_SIZE;  // CD001 partition header
			inArchive.seekg(cvmOffset);
			inArchive.read(reinterpret_cast<char *>(&tag), sizeof(DWORD));
			if (tag == 0x30444301)
			{
				fprintf(stderr, "ENCODING: auto-detected ISO9660 header on input, outputting .cvm\n");
				mod_direction = 1;
			}
			else
			{
				fprintf(stderr, "auto-detection requested, could not detect either CVM or ISO9660 header on input\n");
				return 2;
			}
		}
	}

	//fprintf(stderr, "in: %s\n", inpath.c_str());
	//fprintf(stderr, "out: %s\n", outpath.c_str());

	if (outpath.length() == 0)
	{
		outpath = inpath + (mod_direction < 0 ? ".iso" : ".cvm");
		//fprintf(stderr, "out': %s\n", outpath.c_str());
	}


	// open output
	std::ofstream outstream(outpath.c_str(), std::ios::binary | std::ios::trunc);
	if (!outstream)
	{
		fprintf(stderr, "Cannot create output path: %s\nIf input is located on a read-only filesystem, you should specify an output path.", outpath.c_str());
		return 1;
	}

	if (mod_direction < 0)
	{
		// decode header from 0x9800 to 0xBFFF
		// (writing @0x8000)
		for(size_t numSector = 0x10; numSector <= 0x14; ++numSector)
		{
			BYTE inbuf[CDFS_SECTOR_SIZE] = {};
			size_t cvmOffset = (3 + numSector) * CDFS_SECTOR_SIZE;  // skip 3 sectors for CVM ROFS header
			inArchive.seekg(cvmOffset);
			inArchive.read((char *)inbuf, CDFS_SECTOR_SIZE);

			// Decode input buffer in-place
			CvmDecodeSector(xorIV, inbuf, CDFS_SECTOR_SIZE, numSector);

			outstream.seekp(numSector * CDFS_SECTOR_SIZE);
			outstream.write((const char *)inbuf, CDFS_SECTOR_SIZE);
		}

		DWORD remainder(DWORD(insize) - (3+0x15) * CDFS_SECTOR_SIZE);

		while (remainder > 0)
		{
			BYTE inbuf[CDFS_SECTOR_SIZE] = {};

			DWORD nWriteCount = remainder > CDFS_SECTOR_SIZE ? CDFS_SECTOR_SIZE : remainder;
			
			inArchive.read((char *)inbuf, nWriteCount);
			outstream.write((const char *)inbuf, nWriteCount);
			remainder -= nWriteCount;
		}
		return 0;
	}
	
	// mod_direction > 0

	/// Write CVM info
	outstream.seekp(0);
	outstream.write((const char *)CVMHEADER, 3 * CDFS_SECTOR_SIZE);
	outstream.seekp(0x20);
	{
		BYTE sizebin[4] = {};
		write4BE(sizebin, DWORD(insize) + 3 * CDFS_SECTOR_SIZE);
		outstream.write((const char *)sizebin, 4);
	}
	outstream.seekp(CDFS_SECTOR_SIZE + 0x08);
	{
		BYTE sizebin[4] = {};
		write4BE(sizebin, DWORD(insize) + 2 * CDFS_SECTOR_SIZE - 0xC);
		outstream.write((const char *)sizebin, 4);
	}
	outstream.seekp(CDFS_SECTOR_SIZE + 0x34);
	{
		BYTE sizebin[4] = {};
		write4BE(sizebin, DWORD(insize));
		outstream.write((const char *)sizebin, 4);
	}

	// encode header from 0x8000 to 0xA7FF
	// (writing @0x9800)
	for(size_t numSector = 0x10; numSector <= 0x14; ++numSector)
	{
		BYTE inbuf[CDFS_SECTOR_SIZE] = {};
		size_t isoOffset = numSector * CDFS_SECTOR_SIZE;
		inArchive.seekg(isoOffset);
		inArchive.read((char *)inbuf, CDFS_SECTOR_SIZE);

		// Decode input buffer in-place
		CvmDecodeSector(xorIV, inbuf, CDFS_SECTOR_SIZE, numSector);

		outstream.seekp((3 + numSector) * CDFS_SECTOR_SIZE);  // skip 3 past CVM ROFS
		outstream.write((const char *)inbuf, CDFS_SECTOR_SIZE);
	}

	outstream.seekp((3 + 0x15) * CDFS_SECTOR_SIZE);
	DWORD remainder(DWORD(insize) - 0x15 * CDFS_SECTOR_SIZE);  // note: first 0x10 sectors are not read at all

	while (remainder > 0)
	{
		BYTE inbuf[CDFS_SECTOR_SIZE] = {};

		DWORD nWriteCount = remainder > CDFS_SECTOR_SIZE ? CDFS_SECTOR_SIZE : remainder;
		
		inArchive.read((char *)inbuf, nWriteCount);
		outstream.write((const char *)inbuf, nWriteCount);
		remainder -= nWriteCount;
	}
	


	return 0;
}

