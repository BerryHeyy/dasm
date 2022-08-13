
class Call : InstructionBase
{
    public override bool Compile(string line, ref MEM memory, ref CPU processor, ref ushort compilerPointer, string[] tokens, ref Dictionary<string, List<ushort>> labelReferences, ref ClientTasks clientTasks)
    {
        memory[compilerPointer++] = CPU.instructionOpCodes[tokens[0]][ADDRESSING_MODES.ABSOLUTE];

        if (labelReferences.ContainsKey(tokens[1])) // Label has been referenced before
        {
            labelReferences[tokens[1]].Add(compilerPointer);
        }
        else
        {
            labelReferences.Add(tokens[1], new List<ushort>() {compilerPointer});
        }
        compilerPointer += 2;

        return true;
    }

    public override bool Run(ADDRESSING_MODES addresingMode, ref MEM memory, ref CPU processor, ref ClientTasks clientTasks)
    {
        switch (addresingMode)
        {
        case ADDRESSING_MODES.ABSOLUTE:
        {
            UInt16 address = (UInt16) (processor.ReadByte(memory) | (processor.ReadByte(memory) << 8));
            processor.DI.Value = processor.PC;
            processor.PC = address;
        }break;
        default: return false;
        }

        return true;
    }
}