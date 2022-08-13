public class MEM
{
    public static UInt16 MAX_MEM = 0xFFFF;
    public static UInt16 PC_START = 0x0200;
    public static UInt16 SP_START = 0x0000;
    byte[] data = new byte[MAX_MEM];

    public byte this[int i]
    {
        get { return data[i]; }
        set { data[i] = value; }
    }
}