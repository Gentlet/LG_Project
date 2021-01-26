#ifndef _PLAYER_TYPES_H
#define _PLAYER_TYPES_H

#include <stdint.h>

#include "CommonTypes.h"
#include "ErrorCodes.h"

typedef int32_t HVRID;
static const HVRID INVALID_HANDLE = 0;

typedef void(*LogCallback)(int messageType, const char* str);

typedef struct InterfaceInitialiseInfo
{
	uint32_t stuctSize;
	const char* appId;
	const char* appVersion;
	const char* apiKey;
	const char* extensionPath;
	const char* cachePath;
	int32_t threadPoolSize;
	LogCallback logCallback;
	int32_t logLevel;
} InterfaceInitialiseInfo;

typedef void(*OnAssetInitialised)(
	HVRError /*errorCode*/, 
	void* /*userData*/);

typedef bool(*OnAssetSelectRepresentation)(
	const HVRAdaptationSet* /*adaptationSet*/,
	uint32_t /*representationIndex*/,
	const HVRRepresentation* /*representations*/,
	uint32_t /*representationCount*/,
	void* /*userData*/);

typedef void(*OnAssetRepresentationDataRecieved)(
	const char* /*mimeType*/, 
	const char* /*codec*/, 
	float /*startTime*/, 
	const uint8_t* /*data*/, 
	uint32_t /*dataSze*/, 
	void* /*userData*/
);

typedef struct AssetCreationInfo
{
	uint32_t stuctSize;
	const char* assetPath;
	const char* cacheDir;
	void* userData;
	OnAssetInitialised onInitialised;
	OnAssetSelectRepresentation onSelectRepresentation;
	OnAssetRepresentationDataRecieved onRepresentationDataRecieved;
	float bufferTime;
	bool disableCaching;
} AssetCreationInfo;

typedef struct StatsData
{
	float current;
	float total;
	float maximum;
	float minimum;
	float avg;
	uint32_t count;
} StatsData;

#endif // _TYPES_H
