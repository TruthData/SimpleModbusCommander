using System.Collections;
using System.IO.Ports;
using Newtonsoft.Json;

namespace SimpleModBusCommander
{
    public class ModBusCommander
    {
       
        public SerialPort SP { get; set; }
        public ushort DeviceAddress { get; set; }
        
        //some devices (Huawei bats) use 0x03, some (Epever) use 0x04 
        public byte GetRegFunctionCode { get; set; }
        public LogSeverity LogLevel { get; private set; }

        public ModBusCommander(string port = "COM1", int baud = 115200, ushort deviceAddress = 1, byte getRegFunctionCode = 0x04)
        {
            LogLevel = LogSeverity.WARN;
            SP = new SerialPort(port, baud, Parity.None, 8, StopBits.One);
            DeviceAddress = deviceAddress;
            GetRegFunctionCode = getRegFunctionCode;
            SP.Open();
        }


        
        public Dictionary<string, byte[]> GetRegisters(string address, uint numRegs)
        {
            var frame = Send(Gen_GetRegCmd(address, numRegs),(int)numRegs*2+3);  
           Log(frame.ToHexString());
            var regs = new Dictionary<string, byte[]>();
            ushort startAddress = Convert.ToUInt16(address, 16);
            for (int i=3;i<numRegs*2+3;i+=2)
            {
                var adr = Convert.ToHexString(startAddress.GetBytes());
                var data = new byte[] { frame[i], frame[i + 1] };
                regs[adr] = data;
               Log($"{adr}\t{data.ToHexString()}");
                startAddress++;
            }
            return regs;
        }
        public byte[] Gen_GetRegCmd(string address, uint numRegs)
        {

            byte[] frame = new byte[8];

            frame[0] = (byte)DeviceAddress;         // Slave Address
            frame[1] = GetRegFunctionCode;          // Function Read Reg             

            ushort startAddress = Convert.ToUInt16(address, 16);
            frame[2] = (byte)(startAddress >> 8);   // Starting Address High
            frame[3] = (byte)startAddress;          // Starting Address Low            
            frame[4] = (byte)(numRegs >> 8);        // Quantity of Registers High
            frame[5] = (byte)numRegs;               // Quantity of Registers Low
            byte[] crc = CalculateCRC(frame);       // Calculate CRC.
            frame[frame.Length - 2] = crc[0];       // Error Check Low
            frame[frame.Length - 1] = crc[1];       // Error Check High

            Log(frame.ToHexString());
            return frame;
        }
        public byte[] Send(byte[] frame, int minBytesToRead = 5)
        {


            Log($"--> {BitConverter.ToString(frame)}");
            SP.Write(frame, 0, frame.Length);
            for (int i = 0; i < 50; i++)
            {
                if (SP.BytesToRead < minBytesToRead)
                    Thread.Sleep(10); // Delay 100ms
                else
                {
                    Log($"SPResponse after {(i + 1) * 10}ms");
                    break;
                }
            }
            if (SP.BytesToRead < minBytesToRead)
            {
                throw new Exception($"Response must be >= 5 bytes - no response after {50 * 10}ms");
            }

            byte[] bufferReceiver = new byte[SP.BytesToRead];
            SP.Read(bufferReceiver, 0, SP.BytesToRead);
            SP.DiscardInBuffer();
            var response = BitConverter.ToString(bufferReceiver);
            Log($"<-- {BitConverter.ToString(bufferReceiver)}");
            return bufferReceiver;
        }


        private byte[] CalculateCRC(byte[] data)
        {
            ushort CRCFull = 0xFFFF; // Set the 16-bit register (CRC register) = FFFFH.
            byte CRCHigh = 0xFF, CRCLow = 0xFF;
            char CRCLSB;
            byte[] CRC = new byte[2];
            for (int i = 0; i < (data.Length) - 2; i++)
            {
                CRCFull = (ushort)(CRCFull ^ data[i]); // 

                for (int j = 0; j < 8; j++)
                {
                    CRCLSB = (char)(CRCFull & 0x0001);
                    CRCFull = (ushort)((CRCFull >> 1) & 0x7FFF);

                    if (CRCLSB == 1)
                        CRCFull = (ushort)(CRCFull ^ 0xA001);
                }
            }
            CRC[1] = CRCHigh = (byte)((CRCFull >> 8) & 0xFF);
            CRC[0] = CRCLow = (byte)(CRCFull & 0xFF);
            return CRC;
        }
        public void Log(string message, LogSeverity sev = LogSeverity.DEBUG)
        {
            if(LogLevel>=sev)
                Console.WriteLine(message);
        }

    }
    public enum LogSeverity
    {
        INFO =0,
        ERROR = 1,
        WARN = 2,
        DEBUG = 3,
    }
}
public static class HelperExtensions
{
    public static void Assert(this bool condition, string msg)
    {
        if (!condition)
            throw new Exception(msg);
    }
    public static string ToJsonIndented<T>(this T objectToSerialize)
    {
        return JsonConvert.SerializeObject(objectToSerialize, new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            TypeNameHandling = TypeNameHandling.Auto,
        });
    }

    public static string ToJson<T>(this T objectToSerialize, TypeNameHandling typeNameHandling = TypeNameHandling.Auto, Formatting formatting = Formatting.None)
    {
        return JsonConvert.SerializeObject(objectToSerialize, new JsonSerializerSettings
        {
            TypeNameHandling = typeNameHandling,
            Formatting = formatting,
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,

        });
    }

}

public static class ModBusByteExtensions
    {
    public static string ToHexString(this byte[] data, string delimitor = "")
    {
        if (data == null) return String.Empty;

        return BitConverter.ToString(data).Replace("-", delimitor);
    }
    public static string ToBitString(this BitArray bits, int startBit = 0, int stopBit = 0)
    {
        if (stopBit <= 0)
            stopBit = bits.Count - 1;

        char[] charArray = new char[stopBit - startBit + 1];
        for (int i = startBit, j = 0; i <= stopBit; i++, j++)
            charArray[j] = bits[i] ? '1' : '0';
        Array.Reverse(charArray);
        return new string(charArray);

    }
    public static byte[] Cat(this byte[] a, byte[] b)
    {
        return a.Concat(b).ToArray();
    }
    public static byte[] GetBytes(this UInt16 b, bool msb = true)
    {
        var bytes = BitConverter.GetBytes(b);
        if (msb)
            return bytes.Reverse().ToArray();
        return bytes;
    }
    public static int RegInt16(this byte[] data, int atIndex = 0)
    {
        (atIndex + 2 <= data.Length).Assert($"Cannot Parse RegInt16 from data [{data.ToHexString()}] at index {atIndex} - Too Short");

        return BitConverter.ToInt16(new byte[] { data[atIndex + 1], data[atIndex] }, 0);
    }
    public static int RegUInt16(this byte[] data, int atIndex = 0)
    {
        (atIndex + 2 <= data.Length).Assert($"Cannot Parse RegUInt16 from data [{data.ToHexString()}] at index {atIndex} - Too Short");
        return BitConverter.ToUInt16(new byte[] { data[atIndex + 1], data[atIndex] }, 0);
    }
    public static int RegInt32(this byte[] data, int atIndex = 0)
    {
        //two 16bit reverse-registers [ 93-E0|30-03 ] 
        (atIndex + 4 <= data.Length).Assert($"Cannot Parse RegInt32 from data [{data.ToHexString()}] at index {atIndex} - Too Short");
        return BitConverter.ToInt32(new byte[] { data[atIndex + 1], data[atIndex + 0], data[atIndex + 3], data[atIndex + 2] }, 0);
    }
    public static int RegUInt32(this byte[] data, int atIndex = 0)
    {
        //two 16bit reverse-registers [ 93-E0|30-03 ] 
        (atIndex + 4 <= data.Length).Assert($"Cannot Parse RegUInt32 from data [{data.ToHexString()}] at index {atIndex} - Too Short");
        return (int)BitConverter.ToUInt32(new byte[] { data[atIndex + 1], data[atIndex + 0], data[atIndex + 3], data[atIndex + 2] }, 0);
    }
    public static string RegBitString(this byte[] data, int startBit=0, int endBit=15 )
    {
        var bits = new BitArray(data.Reverse().ToArray());
        return bits.ToBitString(startBit,endBit);
    }
    public static bool RegHasFlag(this byte[] data, int bitNumber)
    {
        var bits = new BitArray(data.Reverse().ToArray());
        return bits[bitNumber];
    }
    public static float AsFloat(this int number, int times = 100)
    {
        return ((float)number) / times;
    }
}

