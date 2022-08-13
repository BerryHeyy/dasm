
class Prtn : InstructionBase
{
    public override bool Compile(string line, ref MEM memory, ref CPU processor, ref ushort compilerPointer, string[] tokens, ref Dictionary<string, List<ushort>> labelReferences, ref ClientTasks clientTasks)
    {
        if (Utility.DoesRegisterExist(tokens[1])) // Value is another register  
        {
            memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.REGISTRY];
            memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];
        }
        else if (tokens[1].StartsWith('[') && tokens[1].EndsWith(']')) // Value is register pointer 
        {
            string registerName = tokens[1].Substring(1, tokens[1].Length - 2);
            if (!Utility.DoesRegisterExist(registerName)) // Check if register exists
            {
                clientTasks.consoleBuffer += Utility.MakeUndefinedRegisterException(line, registerName);
                return false;
            }

            memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.REGISTRY_POINTER];
            memory[compilerPointer++] = CPU.registerEncoding[registerName];
        }
        else if (Utility.IsHexStringValid(tokens[1]))
        {
            UInt64 value = Convert.ToUInt64(tokens[1], 16);
            int valueBits = Utility.GetBits(value);
            if (valueBits <= 8)
            {
                memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.IMMEDIATE];
                memory[compilerPointer++] = (byte) value;
            }
            else
            {
                clientTasks.consoleBuffer += Utility.MakePossibleOverflowException(line, valueBits, 8);
                return false;
            }
        }
        else
        {
            clientTasks.consoleBuffer += Utility.MakeUndefinedArgumentException(line, tokens[1], 1);
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
            byte character = processor.ReadByte(memory);
            clientTasks.consoleBuffer += character;
        }break;
        case ADDRESSING_MODES.REGISTRY:
        {
            byte sourceEncoding = processor.ReadByte(memory);

            clientTasks.consoleBuffer += processor.getRegisterValue(sourceEncoding);
        }break;
        case ADDRESSING_MODES.REGISTRY_POINTER:
        {
            byte registerWithPointer = processor.ReadByte(memory);

            clientTasks.consoleBuffer += memory[(UInt16) processor.getRegisterValue(registerWithPointer)];
        }break;
        default: return false;
        }

        return true;
    }
}