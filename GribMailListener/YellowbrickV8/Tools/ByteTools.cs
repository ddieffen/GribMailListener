namespace YellowbrickV8
{
    internal static class ByteTools
    {
        internal static uint getUint8(this byte[] array, int position, bool little = false)
        {
           return array[position];
        }

        internal static uint getUint16(this byte[] array, int position, bool little = false)
        {
            if (!little)
                return (uint)(array[position] << 8 | array[position + 1]);
            else
                return (uint)(array[position + 1] << 8 | array[position]);
        }

        internal static uint getUint32(this byte[] array, int position, bool little = false)
        {
            if (!little)
                return (uint)(array[position] << 24 | array[position + 1] << 16 | array[position + 2] << 8 | array[position + 3]);
            else
                return (uint)(array[position + 3] << 24 | array[position + 2] << 16 | array[position + 1] << 8 | array[position]);
        }

        internal static int getInt8(this byte[] array, int position, bool little = false)
        {
            return -(~(array[position]) + 1);
        }

        internal static int getInt16(this byte[] array, int position, bool little = false)
        {
            int value = 0;
            if (!little)
                value = (array[position] << 8 | array[position + 1]);
            else
                value =  (array[position + 1] << 8 | array[position]);
            if (value >= 32768)
                return value - 65536;
            else
                return value;
        }

        internal static int getInt32(this byte[] array, int position, bool little = false)
        {
            if (!little)//need to work on this Javascrip uses 2's completement signed integers
                 return -(~(array[position] << 24 | array[position + 1] << 16 | array[position + 2] << 8 | array[position + 3]) + 1);
            else
                return -(~(array[position + 3] << 24 | array[position + 2] << 16 | array[position + 1] << 8 | array[position]) + 1);
        }

    }
}
