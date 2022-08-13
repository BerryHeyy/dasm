
class Cmp : InstructionBase
{
    public override bool Compile(string line, ref MEM memory, ref CPU processor, ref ushort compilerPointer, string[] tokens, ref Dictionary<string, List<ushort>> labelReferences, ref ClientTasks clientTasks)
    {
        if (Utility.DoesRegisterExist(tokens[1])) // Destination exists
        {
            if (Utility.IsHexStringValid(tokens[2])) // Value is a number literal
            {
                int regBits = Utility.GetRegisterSize(CPU.registerEncoding[tokens[1]]);
                int valueBits = Utility.GetBits(Convert.ToUInt64(tokens[2], 16));

                if (regBits >= valueBits)
                {
                    memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.IMMEDIATE];
                    memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];

                    Utility.WriteNumberToMemory(tokens[2], regBits, ref memory, ref compilerPointer);
                }
                else
                {
                    clientTasks.consoleBuffer += Utility.MakePossibleOverflowException(line, valueBits, regBits);
                    return false;
                }
            }
        }
        else
        {
            clientTasks.consoleBuffer += Utility.MakeUndefinedRegisterException(line, tokens[1]);
            return false;
        }

        return true;
    }

    public override bool Run(ADDRESSING_MODES addresingMode, ref MEM memory, ref CPU processor, ref ClientTasks clientTasks)
    {
        switch (addresingMode)
        {
            case ADDRESSING_MODES.IMMEDIATE:
            {
                byte destinationEncoding = processor.ReadByte(memory);

                List<byte> bytes = new List<byte>();

                for (int i = 0; i < Utility.GetRegisterSize(destinationEncoding) / 8; i++)
                {
                    bytes.Add(processor.ReadByte(memory));
                }

                UInt64 val = Utility.LittleEndianByteArrayToInt(bytes.ToArray());

                processor.CF = Convert.ToByte(processor.getRegisterValue(destinationEncoding) == val);
            }break;
            default: return false;
        }
        return true;
    }
}