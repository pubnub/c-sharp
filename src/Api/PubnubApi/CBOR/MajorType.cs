using System;

namespace PubnubApi.CBOR
{
    public enum MajorType : byte
    {
        UNSIGNED_INT = 0,
        NEGATIVE_INT = 1,
        BYTE_STRING = 2,
        TEXT_STRING = 3,
        ARRAY = 4,
        MAP = 5,
        TAG = 6,
        FLOATING_POINT_OR_SIMPLE = 7
    }
}
