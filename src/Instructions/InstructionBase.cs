abstract class InstructionBase
{
    public abstract bool Compile(
        string line,
        ref MEM memory,
        ref CPU processor,
        ref UInt16 compilerPointer,
        string[] tokens,
        ref Dictionary<string, List<UInt16>> labelReferences,
        ref ClientTasks clientTasks
    );

    public abstract bool Run(
        ADDRESSING_MODES addressingMode,
        ref MEM memory,
        ref CPU processor,
        ref ClientTasks clientTasks
    );
}