using System.Collections.Generic;

public class CPU
{
    // Registers
    public UInt16 PC; // Program Counter

    public Register AX, BX, CX, DX, SI, DI, SP, BP;

    public UInt64 R8, R9, R10, R11, R12, R13, R14, R15;

    public Byte CF; // Control flag for logical operations;

    // Definitions
    public static Dictionary<string, Dictionary<ADDRESSING_MODES, byte>> instructionOpCodes = 
        new Dictionary<string, Dictionary<ADDRESSING_MODES, byte>> 
        {
            {
                "mov", new Dictionary<ADDRESSING_MODES, byte>()
                { 
                    { ADDRESSING_MODES.ABSOLUTE, 0x01 },
                    { ADDRESSING_MODES.IMMEDIATE, 0x02 },
                    { ADDRESSING_MODES.REGISTRY, 0x03 },
                    { ADDRESSING_MODES.REGISTRY_POINTER, 0x04 }
                }
            },
            {
                "cmp", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMMEDIATE, 0x05 },
                } 
            },
            {
                "jne", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.ABSOLUTE, 0x06 },
                }
            },
            {
                "pusha", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMPLICIT, 0x07 },
                }
            },
            {
                "popa", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMPLICIT, 0x08 },
            }
            },
            {
                "jmp", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.ABSOLUTE, 0x09 },
                }
            },    
            {
                "call", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.ABSOLUTE, 0x0B },
                }
            },
            {
                "ret", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMPLICIT, 0x0C },
                } 
            },
            {
                "prtc", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMMEDIATE, 0x0D },
                    { ADDRESSING_MODES.REGISTRY, 0x0E },
                    { ADDRESSING_MODES.REGISTRY_POINTER, 0x0A },
                }
            },
            {
                "add", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMMEDIATE, 0x0F },
                    { ADDRESSING_MODES.REGISTRY, 0x16}             // <----- last changed.
                }
            },
            {
                "jeq", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.ABSOLUTE, 0x10 }
                }
            },
            {
                "prtn", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    { ADDRESSING_MODES.IMMEDIATE, 0x11 },
                    { ADDRESSING_MODES.REGISTRY, 0x12 },
                    { ADDRESSING_MODES.REGISTRY_POINTER, 0x13 }
                }
            },
            {
                "dec", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    {ADDRESSING_MODES.ABSOLUTE, 0x14}
                }
            },
            {
                "inc", new Dictionary<ADDRESSING_MODES, byte>()
                {
                    {ADDRESSING_MODES.ABSOLUTE, 0x15}
                }
            }
        };

    public static Dictionary<string, byte> registerEncoding = new Dictionary<string, byte>()
    {
        { "al", 0x00 }, { "bl", 0x01 }, { "cl", 0x02 }, { "dl", 0x03 }, { "ah", 0x04 }, { "bh", 0x05 },
        { "ch", 0x06 }, { "dh", 0x07 }, // 8-bit registers

        { "ax", 0x08 }, { "bx", 0x09 }, { "cx", 0x0A }, { "dx", 0x0B }, { "si", 0x0C }, { "di", 0x0D },
        { "sp", 0x0E }, { "bp", 0x0F }, // 16-bit registers

        { "r8", 0x10 }, { "r9", 0x11 }, { "r10", 0x12 }, { "r11", 0x13 }, { "r12", 0x14 },
        { "r13", 0x15 }, { "r14", 0x16 }, { "r15", 0x17 } // 64-bit registers
    }; // I check for sizes with these assigned encoding Values in the assembler. 
       // THIS IS VERY ERROR PRONE IF I CHANGE THIS MAP IN ANY WAY.

    public CPU()
    {
        Reset();
    }

    public void Reset()
    {
        PC = MEM.PC_START;
        SP.Value = MEM.SP_START;
        R8 = R9 = R10 = R11 = R12 = R13 = R14 = R15 = 0;
        CF = 0;
    }

    public byte ReadByte(in MEM memory)
    {
        return memory[PC++];
    }

    public void WriteByte(byte toWrite, ref MEM memory)
    {
        memory[PC++] = toWrite;
    }

    public byte PullStack(ref MEM memory) // TODO: Add check to see if pointer goes out of bounds
    {
        byte toReturn = memory[--SP.Value];
        memory[SP.Value] = 0x00;
        return toReturn;        
    }

    public UInt16 PullStackWord(ref MEM memory)
    {
        byte a = memory[--SP.Value];
        memory[SP.Value] = 0x00;
        byte b = memory[--SP.Value];
        memory[SP.Value] = 0x00;

        return (UInt16) ((a) | (b << 8));
    }

    public void PushStack(byte toPush, ref MEM memory)
    {
        memory[SP.Value++] = toPush;
    }

    public void PushStackWord(UInt16 toPush, ref MEM memory)
    {
        memory[SP.Value++] = (byte) (toPush & 0x00FF);
        memory[SP.Value++] = (byte) (toPush >> 8);
    }

    public UInt64 getRegisterValue(byte encoding)
    {
        switch (encoding)
        {
        case 0x00: return AX.Lower();
        case 0x01: return BX.Lower();
        case 0x02: return CX.Lower();
        case 0x03: return DX.Lower();
        case 0x04: return AX.Upper();
        case 0x05: return BX.Upper();
        case 0x06: return CX.Upper();
        case 0x07: return DX.Upper();

        case 0x08: return AX.Value;
        case 0x09: return BX.Value;
        case 0x0A: return CX.Value;
        case 0x0B: return DX.Value;
        case 0x0C: return SI.Value;
        case 0x0D: return DI.Value;
        case 0x0E: return SP.Value;
        case 0x0F: return BP.Value;

        case 0x10: return R8;
        case 0x11: return R9;
        case 0x12: return R10;
        case 0x13: return R11;
        case 0x14: return R12;
        case 0x15: return R13;
        case 0x16: return R14;
        case 0x17: return R15;
        }
        return 0;
    }

    public void SetRegisterValue(byte encoding, UInt64 value)
    {
        switch (encoding)
        {
        case 0x00: AX.SetLower((byte)value); break;
        case 0x01: BX.SetLower((byte)value); break;
        case 0x02: CX.SetLower((byte)value); break;
        case 0x03: DX.SetLower((byte)value); break;
        case 0x04: AX.SetUpper((byte)value); break;
        case 0x05: BX.SetUpper((byte)value); break;
        case 0x06: CX.SetUpper((byte)value); break;
        case 0x07: DX.SetUpper((byte)value); break;

        case 0x08: AX.Value = (UInt16) value; break;
        case 0x09: BX.Value = (UInt16) value; break;
        case 0x0A: CX.Value = (UInt16) value; break;
        case 0x0B: DX.Value = (UInt16) value; break;
        case 0x0C: SI.Value = (UInt16) value; break;
        case 0x0D: DI.Value = (UInt16) value; break;
        case 0x0E: SP.Value = (UInt16) value; break;
        case 0x0F: BP.Value = (UInt16) value; break;

        case 0x10: R8 = value; break;
        case 0x11: R9 = value; break;
        case 0x12: R10 = value; break;
        case 0x13: R11 = value; break;
        case 0x14: R12 = value; break;
        case 0x15: R13 = value; break;
        case 0x16: R14 = value; break;
        case 0x17: R15 = value; break;
        }
    }
}

public struct Register
{
    public UInt16 Value;

    public byte Lower()
    {
        return (byte) (Value & 0x00FF); 
    }

    public byte Upper()
    {
        return (byte) (Value >> 8);
    }

    public void SetLower(byte Lower)
    {
        Value = (UInt16) ((Value & (0xFF00)) | Lower);
    }

    public void SetUpper(byte Upper)
    {
        Value = (UInt16) (Value & (0xFF00) | Upper << 8);
    }

}