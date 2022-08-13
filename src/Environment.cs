using System.Text;
using Discord.WebSocket;

class Environment
{
    private MEM memory = new MEM();
    private CPU processor = new CPU();

    public bool dumpMemory, dumpFull;
    public string programCode = "";
    public string preMemoryDump = "";
    public string postMemoryDump = "";
    
    public ISocketMessageChannel channelSent;
    public ClientTasks clientTasks = new ClientTasks();

    public Dictionary<string, InstructionBase> instructionClasses = new Dictionary<string, InstructionBase>()
    {
        {"mov", new Mov()}, {"jmp", new Jmp()}, {"add", new Add()}, {"cmp", new Cmp()},
        {"call", new Call()}, {"jne", new Jne()}, {"ret", new Ret()}, {"prtc", new Prtc()},
        {"jeq", new Jeq()}, {"prtn", new Prtn()}, {"dec", new Dec()}, {"inc", new Inc()}
    };

    public Environment(ISocketMessageChannel channelSent)
    {
        this.channelSent = channelSent;
    }

    public bool Compile()
    {
        Dictionary<string, UInt16> labels = new Dictionary<string, ushort>();
        Dictionary<string, List<UInt16>> labelReferences = new Dictionary<string, List<UInt16>>();

        UInt16 compilerPointer = MEM.PC_START;
        
        string[] lines = programCode.Split('\n');
        lines = lines.Where(line => line.Length > 0).ToArray(); // Remove empty lines.

        foreach (string line in lines)
        {
            string[] tokens = line.Split(' ');
            tokens = tokens.Where(token => token.Length > 0).ToArray(); // Remove empty strings.
            int commentStartIndex = -1;
            for (int i = 0; i < tokens.Length; i++) // Remove comma from arguments
            {
                if (tokens[i].StartsWith('#') && commentStartIndex == -1) commentStartIndex = i;
                if (tokens[i].EndsWith(','))
                {
                    tokens[i] = tokens[i].Substring(0, tokens[i].Length - 1);
                }
            }
            if (tokens.Length == 0) continue;
            if (commentStartIndex != -1) tokens = tokens[0..(commentStartIndex)];

        begin_check:

            if (Utility.DoesInstructionExist(tokens[0]))
            {
                if (!instructionClasses[tokens[0]].Compile(line, ref memory, ref processor, ref compilerPointer, tokens, ref labelReferences, ref clientTasks)) 
                    return false;
            }
            else if (tokens[0] == "db") // Checck if instruction is pseudo op
            {
                for (int j = 1; j < tokens.Length; j++)
                {
                    if (tokens[j].StartsWith('"') && tokens[j].EndsWith('"')) // String
                    {
                        string inside = tokens[j].Substring(1, tokens[j].Length - 2);
                        byte[] bytes = Encoding.UTF8.GetBytes(inside);
                        Array.ForEach(bytes, b => memory[compilerPointer++] = b);
                    }
                    else if (Utility.IsHexStringValid(tokens[j])) // Byte literal
                    {
                        UInt64 number = Convert.ToUInt64(tokens[j], 16);
                        if (Utility.GetBits(number) <= 8)
                        {
                            memory[compilerPointer++] = (byte) number;
                        }
                        else
                        {
                            clientTasks.consoleBuffer += Utility.MakePossibleOverflowException(line, (int) number, 8);
                            return false;
                        }
                    }
                }
            }
            else if (tokens[0].EndsWith(':')) // Token is label
            {
                string labelName = tokens[0].Substring(0, tokens[0].Length - 1);
                if (!labels.ContainsKey(labelName))
                {
                    labels.Add(labelName, compilerPointer); 
                }
                else 
                {
                    clientTasks.consoleBuffer += Utility.MakeLabelAlreadyDefinedException(line, tokens[0], compilerPointer);
                    return false;
                }

                if (tokens.Length > 1) // Line is token + inline code
                {
                    tokens = tokens.Skip(1).ToArray();

                    goto begin_check;
                }
            }
            else
            {
                clientTasks.consoleBuffer += Utility.MakeUndefinedInstructionException(line, tokens[0]);
                return false;
            }
        }

        // Link Labels
        foreach (KeyValuePair<string, List<UInt16>> label in labelReferences)
        {
            foreach (UInt16 reference in label.Value)
            {
                if (labels.ContainsKey(label.Key)) // Label has been declared
                {
                    memory[reference] = (byte) (labels[label.Key] & 0x00FF);
                    memory[reference + 1] = (byte) (labels[label.Key] >> 8);
                }
                else
                {
                    clientTasks.consoleBuffer += Utility.MakeLabelNotFoundException(reference, label.Key);
                    return false;
                }
            }
        }

        if (dumpMemory) preMemoryDump = DumpMemory();

        return true;
    }

    public bool Run()
    {
        while (processor.PC < MEM.MAX_MEM)
        {
        start_run_loop:

            byte instruction = processor.ReadByte(memory);

            foreach (KeyValuePair<string, Dictionary<ADDRESSING_MODES, byte>> addressingModes in CPU.instructionOpCodes)
            {
                foreach (KeyValuePair<ADDRESSING_MODES, byte> opCodes in addressingModes.Value)
                {
                    if (opCodes.Value == instruction)
                    {
                        instructionClasses[addressingModes.Key].Run(opCodes.Key, ref memory, ref processor, ref clientTasks);
                        goto start_run_loop;
                    }
                }
            }
        }

        if (dumpMemory) postMemoryDump = DumpMemory();

        return true;
    }

    private string DumpMemory()
    {
        string format = 
@"Processor State:      CPU-flags:  CF = {16}
    16-bit registers: AX = {0:X4} | BX = {1:X4} | CX = {2:X4}
                      DX = {3:X4} | SI = {4:X4} | DI = {5:X4}
                      SP = {6:X4} | BP = {7:X4}
    64-bit registers: R8 = {8:X16} | R9 = {9:X16}
                      R10 = {10:X16} | R11 = {11:X16}
                      R12 = {12:X16} | R13 = {13:X16}
                      R14 = {14:X16} | R15 = {15:X16}";
        string toReturn = String.Format(format, processor.AX.Value, processor.BX.Value, processor.CX.Value,
            processor.DX.Value, processor.SI.Value, processor.DI.Value, processor.SP.Value, processor.BP.Value, processor.R8,
            processor.R9, processor.R10, processor.R11, processor.R12, processor.R13, processor.R14, processor.R15,
            processor.CF);

        toReturn += "\n\nOffset  00 01 02 03 04 05 06 07 08 09 0a 0b 0c 0d 0e 0f\n";

        StringBuilder sb = new StringBuilder(); // Use StringBuilder so memory doesnt have to be reallocated on each concatination.
        sb.Capacity = 3 * MEM.MAX_MEM + 11 * MEM.MAX_MEM / 16;

        for (int i = 0; i < MEM.MAX_MEM; i++)
        {   
            if (i % 16 == 0) sb.Append(String.Format("\n0x{0:X4} ", i));
            sb.Append(String.Format(" {0:X2}", memory[i]));
        }

        toReturn += sb;

        return toReturn;
    }
}

struct ClientTasks
{
    public string consoleBuffer = "";
    public List<UInt64> userToBan = new List<ulong>();

    public ClientTasks()
    {

    }
}