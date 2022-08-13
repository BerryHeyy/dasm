class Mov : InstructionBase
{
    public override bool Compile(
        string line, 
        ref MEM memory, 
        ref CPU processor, 
        ref ushort compilerPointer, 
        string[] tokens, 
        ref Dictionary<string, List<ushort>> labelReferences,
        ref ClientTasks clientTasks)
    {
        if (Utility.DoesRegisterExist(tokens[1])) // Destination exists
        {
            if (Utility.DoesRegisterExist(tokens[2])) // Value is register
            {
                memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.REGISTRY];
                memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];
                memory[compilerPointer++] = CPU.registerEncoding[tokens[2]];
            }
            else if (tokens[2].StartsWith('[') && tokens[2].EndsWith(']')) // Value is register pointer 
            {
                string registerName = tokens[2].Substring(1, tokens[2].Length - 2);
                if (!Utility.DoesRegisterExist(registerName)) // Check if register exists
                {
                    clientTasks.consoleBuffer += Utility.MakeUndefinedRegisterException(line, registerName);
                    return false;
                }

                memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.REGISTRY_POINTER];
                memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];
                memory[compilerPointer++] = CPU.registerEncoding[registerName];
            }
            else if (Utility.IsHexStringValid(tokens[2])) // Value is number
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
            else // Value is label
            {
                memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.ABSOLUTE];
                memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];

                if (labelReferences.ContainsKey(tokens[2])) // Label has been referenced before
                {
                    labelReferences[tokens[2]].Add(compilerPointer);
                }
                else
                {
                    labelReferences.Add(tokens[2], new List<ushort>() {compilerPointer});
                }

                compilerPointer += 2; // Move compiler pointer 2 bytes forward to make space for the memory address during linking.
            }
        }
        else // Destination does not exist
        {
            clientTasks.consoleBuffer += Utility.MakeUndefinedRegisterException(line, tokens[1]);
            return false;
        }

        return true;
    }

    public override bool Run(ADDRESSING_MODES addresingMode, ref MEM memory, ref CPU processor, ref ClientTasks clientTasks)
    {
        switch(addresingMode)
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
            
            processor.SetRegisterValue(destinationEncoding, val);
        }break;
        case ADDRESSING_MODES.REGISTRY:
        {
            byte destinationEncoding = processor.ReadByte(memory);
            byte sourceEncoding = processor.ReadByte(memory);
            processor.SetRegisterValue(destinationEncoding, processor.getRegisterValue(sourceEncoding));
        }break;
        case ADDRESSING_MODES.REGISTRY_POINTER:
        {
            byte destinationEncoding = processor.ReadByte(memory);
            byte sourceEncoding = processor.ReadByte(memory);

            byte bytttt = memory[(UInt16) processor.getRegisterValue(sourceEncoding)];

            processor.SetRegisterValue(destinationEncoding, bytttt);
        }break;
        case ADDRESSING_MODES.ABSOLUTE:
        {
            byte destinationEncoding = processor.ReadByte(memory);
            UInt16 value = (UInt16) (processor.ReadByte(memory) | (processor.ReadByte(memory) << 8));

            processor.SetRegisterValue(destinationEncoding, value);
        }break;
        default: return false;
        }

        return true;
    }
}