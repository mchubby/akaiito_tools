#include <cstdio>

#include <windows.h>
#include <tchar.h>

#include "libcvm.h"


// NEVER HAPPENS
bool t_xorgen0() { return false; }

// xorgen1 is made of XORs only
bool t_xorgen1()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x5C, 0x87, 0x4F, 0x91};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0xD3, 0xD5, 0xDB, 0xA6, 0x02, 0x56, 0x40, 0x26};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[1](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}


// xorgen2 is made of XORs only
bool t_xorgen2()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x5B, 0xF1, 0x4D, 0x51};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0xB7, 0x98, 0x56, 0xA5, 0x8A, 0xE9, 0x05, 0x40};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[2](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}

// NEVER HAPPENS
bool t_xorgen3() { return false; }

// xorgen4 is made of additions only
bool t_xorgen4()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x55, 0x3D, 0x66, 0x73};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0xD5, 0xD1, 0xA9, 0x93, 0x40, 0xE9, 0xDB, 0x1D};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[4](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}


// xorgen5 is made of additions only
bool t_xorgen5()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x4F, 0xB7, 0x5C, 0x6F};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0x54, 0x45, 0x56, 0x24, 0x5E, 0x6E, 0x40, 0x4A};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[5](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}

// NEVER HAPPENS
bool t_xorgen6() { return false; }


// xorgen7 is made of both XORs and additions
bool t_xorgen7()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x66, 0x95, 0x44, 0x51};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0xD5, 0x11, 0xE9, 0xFB, 0x5E, 0x54, 0x70, 0x30};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[7](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}

// xorgen8 is made of both XORs and additions
bool t_xorgen8()
{
	BYTE xorIV[8] = {0x5E, 0xB7, 0x54, 0xE9, 0x40, 0xDB, 0x56, 0xD5};
	BYTE xorgenValues[4] = {0x52, 0xE5, 0x57, 0x23};

	BYTE outputMutatedVector  [8] = {};
	BYTE expectedMutatedVector[8] = {0xE9, 0xBB, 0x56, 0x92, 0x77, 0xD5, 0xB7, 0x32};

	const CvmXorgenDelegate * const xorgen_vtable = ::CvmGetXorgenMethods();
	xorgen_vtable[8](xorIV, xorgenValues, outputMutatedVector);
	
	return memcmp(outputMutatedVector, expectedMutatedVector, sizeof(outputMutatedVector)) == 0;
}


typedef bool (*MY_TESTS)();

MY_TESTS tests[] = {
	t_xorgen0,
	t_xorgen1,
	t_xorgen2,
	t_xorgen3,
	t_xorgen4,
	t_xorgen5,
	t_xorgen6,
	t_xorgen7,
	t_xorgen8,
};

const char *results[] = { "FAILED", "SUCCEEDED" };


int _tmain()
{

	for ( int i = 0; i < sizeof(tests) / sizeof(tests[0]); ++i )
	{
		fprintf(stderr, "t_xorgen%d: %s\n", i, results[(size_t)tests[i]()]);
	}
	return 0;
}

