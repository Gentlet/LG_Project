#ifndef _COMMON_HEADERS_ERROR_CODES_H_
#define _COMMON_HEADERS_ERROR_CODES_H_

#include "Export.h"

typedef int HVRError;

static const HVRError HVR_ERROR_SUCCESS = 0;
static const HVRError HVR_ERROR_UNKNOWN = 1;

// Interface 100-199

// Asset 200-299
static const HVRError HVR_ERROR_MANIFEST_NOT_FOUND = 200;
static const HVRError HVR_ERROR_MANIFEST_INVALID = 201;
static const HVRError HVR_ERROR_REPRESENTATION_NOT_FOUND = 202;
static const HVRError HVR_ERROR_REPRESENTATION_INVALID = 203;
static const HVRError HVR_ERROR_NO_VALID_DECODER_FOUND = 204;
static const HVRError HVR_ERROR_OFFLINE_CACHE_INVALID = 205;
static const HVRError HVR_ERROR_NO_VOLUMETRIC_TRACK = 206;
static const HVRError HVR_ERROR_FAILED_TO_READ_FRAMES = 207;
static const HVRError HVR_ERROR_FAILED_TO_DECODE_FRAME = 208;
static const HVRError HVR_ERROR_UNSUPPORTED_ASSET_SOURCE = 209;
static const HVRError HVR_ERROR_INVALID_CACHE = 210;
static const HVRError HVR_ERROR_INVALID_FILE = 211;


// TODO: Get this exporting correctly when used in the player interface, Kieran
//API_INTERFACE const char* HVRError_ToString(HVRError error);

#endif // _COMMON_HEADERS_ERROR_CODES_H_
