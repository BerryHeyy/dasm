class Ret : InstructionBase
{
    public override bool Compile(string line, ref MEM memory, ref CPU processor, ref ushort compilerPointer, string[] tokens, ref Dictionary<string, List<ushort>> labelReferences, ref ClientTasks clientTasks)
    {
        memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.IMPLICIT];

        return true;
    }

    public override bool Run(ADDRESSING_MODES addresingMode, ref MEM memory, ref CPU processor, ref ClientTasks clientTasks)
    {
        switch (addresingMode)
        {
        case ADDRESSING_MODES.IMPLICIT:
        {
            processor.PC = processor.DI.Value;
        }break;
        default: return false;
        }

        return true;
    }
}