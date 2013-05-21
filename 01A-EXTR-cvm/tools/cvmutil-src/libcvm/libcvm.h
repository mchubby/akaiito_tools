#if !defined(LIBCVM_H__INCLUDED_)
#define LIBCVM_H__INCLUDED_
 

#if _MSC_VER > 1000
#pragma once
#endif // _MSC_VER > 1000

#include <windows.h>




typedef void (*CvmXorgenDelegate)(LPBYTE /*in IV*/, LPBYTE /*in xorgenValues*/, LPBYTE /*out mutatedVector*/);




const WORD * const CvmGetXorLookupTable();

const CvmXorgenDelegate * const CvmGetXorgenMethods();

// Creates an initialization vector for the Xor operations
// Uses a seed (pInitializer), which is the product code of specified length. If length == -1, it is calculated based on string length of 0-terminated pInitializer.
// The vector (pDestBuffer), which is supposed to be at least 8 bytes long, is modified in-place.
void CvmCreateXorIV(const BYTE* const pInitializer, int length, const WORD* const pLookupTable, LPBYTE /*out*/ pDestBuffer);


// A sector is usually 0x800 bytes in ISO9660
void CvmDecodeSector(LPBYTE IV, LPBYTE /*in/out*/ pData, int dataLength, BYTE sectorIndex);


#endif
