namespace HVR.Interface
{
    public static class ErrorCodes
    {
        public const int HVR_ERROR_SUCCESS = 0;
        public const int HVR_ERROR_UNKNOWN = 1;

        public const int HVR_ERROR_MANIFEST_NOT_FOUND = 200;
        public const int HVR_ERROR_MANIFEST_INVALID = 201;
        public const int HVR_ERROR_REPRESENTATION_NOT_FOUND = 202;
        public const int HVR_ERROR_REPRESENTATION_INVALID = 203;
        public const int HVR_ERROR_NO_VALID_DECODER_FOUND = 204;
        public const int HVR_ERROR_OFFLINE_CACHE_INVALID = 205;
        public const int HVR_ERROR_NO_VOLUMETRIC_TRACK = 206;
        public const int HVR_ERROR_FAILED_TO_READ_FRAMES = 207;
        public const int HVR_ERROR_FAILED_TO_DECODE_FRAME = 208;
    }
}