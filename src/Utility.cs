static class Utility
{
    public static bool DoesInstructionExist(string instruction)
    {
        return CPU.instructionOpCodes.ContainsKey(instruction);
    }

    public static bool DoesRegisterExist(string register)
    {
        return CPU.registerEncoding.ContainsKey(register);
    }

    public static bool DoesDictContainKey<K, V>(Dictionary<K, V> dict, K key) where K: class
    {
        foreach (KeyValuePair<K, V> pair in dict)
        {
            if (pair.Key == key) return true;
        }

        return false;
    }

    public static bool IsHexStringValid(string hexString)
    {
        if (!hexString.StartsWith("0x")) return false;
        hexString = hexString.Substring(2);

        bool isHex; 
        foreach(char c in hexString)
        {
            isHex = ((c >= '0' && c <= '9') || 
                    (c >= 'a' && c <= 'f') || 
                    (c >= 'A' && c <= 'F'));

            if(!isHex)
                return false;
        }
        return true;
    }

    public static int GetBits(UInt64 number)
    {
        int i = 0;

        while (number != 0)
        {
            number = number >> 1;
            i++;
        }

        return i;
    }

    public static int GetRegisterSize(byte registerEncoding)
    {
        if (registerEncoding <= 0x07) return 8;
        else if (registerEncoding <= 0x0F) return 16;
        else return 64;
    }

    public static byte[] IntToLittleEndianByteArray(UInt64 value)
    {
        List<byte> bytes = new List<byte>();
        
        while (value != 0)
        {
            byte _byte = (byte) (value & 0xFF);
            bytes.Add(_byte);

            value >>= 8;
        }

        return bytes.ToArray();
    }

    public static UInt64 LittleEndianByteArrayToInt(byte[] array)
    {
        if (array.Length == 1) return array[0];
        else if (array.Length == 2) return System.Buffers.Binary.BinaryPrimitives.ReadUInt16LittleEndian(array);
        else if (array.Length == 8) return System.Buffers.Binary.BinaryPrimitives.ReadUInt64LittleEndian(array);
        else return 0;
    }

    public static void WriteNumberToMemory(string hexString, int regBits, ref MEM memory, ref UInt16 compilerPointer)
    {
        byte[] bytes = Utility.IntToLittleEndianByteArray(Convert.ToUInt64(hexString, 16));

        foreach (byte b in bytes)
        {
            memory[compilerPointer++] = b;
        }

        int remainingBytes = regBits / 8 - bytes.Length;

        if (remainingBytes > 0)
        {
            while (remainingBytes > 0)
            {
                memory[compilerPointer++] = 0x00;
                remainingBytes--;
            }
        }
    }

    // Exceptions
    public static string MakeUndefinedRegisterException(string line, string register)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. UndefinedRegistryException: Given registry: `{1}`, was not found.", line, register);
    }

    public static string MakeUndefinedInstructionException(string line, string instruction)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. UndefinedInstructionException: Given instruction: `{1}`, was not found.", line, instruction);
    }

    public static string MakePossibleOverflowException(string line, int provided, int expected)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. PossibleOverflowException: Max: `{1}` bits, provided: `{2}` bits.", line, provided, expected);
    }

    public static string MakeInstructionException(string line, int provided, int expected)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. InstructionException: Provided: `{1}` instructions, expected: `{2}` instructions.", line, provided, expected);
    }

    public static string MakeHexStringFormatException(string line, string provided)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. HexStringFormatException: Provided (`{1}`) hex number is not a valid hex number.", line, provided);
    }

    public static string MakeLabelAlreadyDefinedException(string line, string label, UInt16 address)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. LabelAlreadyProvidedException: Provided label (`{1}`) is already defined to point at `0x{2:X4}`.", line, label, address);
    }

    public static string MakeLabelNotFoundException(UInt16 address, string label)
    {
        return String.Format("Exception while compiling code. At address: `{0:X4}`. LabelNotFoundException: Provided label (`{1}`) has not been defined.", address, label);
    }

    public static string MakeUndefinedArgumentException(string line, string argument, int argumentIndex)
    {
        return String.Format("Exception while compiling code. At line: `{0}`. UndefinedArgumentException: Provided argument `{1}`, at index `{2}` is not supported.", line, argument, argumentIndex);
    }

}