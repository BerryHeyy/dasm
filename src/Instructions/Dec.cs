
class Dec : InstructionBase
{
    public override bool Compile(string line, ref MEM memory, ref CPU processor, ref ushort compilerPointer, string[] tokens, ref Dictionary<string, List<ushort>> labelReferences, ref ClientTasks clientTasks)
    {

        if (Utility.DoesRegisterExist(tokens[1])) // Value is another register  
        {
            memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.ABSOLUTE];
            memory[compilerPointer++] = CPU.registerEncoding[tokens[1]];
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
        case ADDRESSING_MODES.ABSOLUTE:
        {
            byte destinationEncoding = processor.ReadByte(memory);
            processor.SetRegisterValue(destinationEncoding, processor.getRegisterValue(destinationEncoding) - 1);
        }break;
        default: return false;
        }

        return true;
    }
}
