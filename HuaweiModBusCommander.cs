
namespace SimpleModBusCommander
{

    public class HuaweiModBusCommander : ModBusCommander
    {
        //Huawei spec uses function code 0x03 to get registers, and baud 9600.
        public HuaweiModBusCommander(string port = "COM1", ushort deviceAddress = 214) : base(port, 9600, deviceAddress, 0x03)
        {
            //todo get rid of device Address default..
        }

        //The Huawei batteries don't always immediately respond to the first few requests made... once they start responding they respond reliably.. You sort of have to wake them up for some reason.  Not sure if it's a "security" scheme or what.
        //The OEM HUAWEI modbus battery tool also seems to try several times to connect at first... and only to deviceID 214... I have two bats, both communicate on id 214.
        public bool EstablishConnection()
        {
            for (int i = 0; i < 5; i++)
            {
                try
                {
                    Log("EstablishConnection...",LogSeverity.INFO);
                    var registers = GetRegisters("0000", 1);
                    return true;
                }
                catch
                {
                    SP.Close();
                    Thread.Sleep(1000);
                    SP.Open();

                }
            }
            return false;
        }
        public HuaweiBatData GetData()
        {
            var data = new HuaweiBatData();
            //note Huawei's implementation's responses do not include a checksum... what should be the checksum bytes is actually data!
            var registers = GetRegisters("0000", 5);
            
            //     id  func  len   reg1    reg2    reg3    reg4   reg5
            //     id  func  len   busv    batv    batA    SOC    SOH
            //<-- [d6 | 03 | 0a  | 13 27 | 13 30 | fb 65 | 00 4f| 00 64]
            data.BusVoltage     = registers["0000"].RegUInt16().AsFloat();
            data.BattVoltage    = registers["0001"].RegUInt16().AsFloat();
            data.BattCurrent    = registers["0002"].RegInt16().AsFloat();
            data.BattPower = data.BattVoltage * data.BattCurrent;
            data.BattSOC        = registers["0003"].RegUInt16();
            data.BattSOH        = registers["0004"].RegUInt16();

           //CELL VOLTAGES & TEMPS
            var voltages = GetCellVoltages();
            var temps = GetCellTemps();
            (voltages.Count == temps.Count).Assert("An unequal number of cell voltages & cell temps were returned.");
            for(int i=0;i<voltages.Count;i++)
                data.Cells.Add(new HuaweiBatCellData() { Volts = voltages[i], TempC = temps[i] });

            return data;
        }
        public List<float> GetCellVoltages()
        {
            //CELL VOLTAGES & TEMPS
            //--> d6|03|0022|000f|b7e3  //GET the 15 registers storing 15 cell voltages.   Note hex 0d1b--> dec 3355 --> 3.355v
            //<-- id|fc|ln| ---15 reg isters / 30 bytes ---| NO checksum!
            //          30|3355|3354|3361|3367|3362|3350|3357|3357|3371|3355|3383|3378|3375|3378|3377|   
            //<-- d6|03|1e|0d1b|0d1a|0d21|0d27|0d22|0d16|0d1d|0d1d|0d2b|0d1b|0d37|0d32|0d2f|0d32|0d31|

            var registers = GetRegisters("0022", HuaweiBatData.NumCells);
            var voltageList = new List<float>();
            foreach (var v in registers.Values)
                voltageList.Add(v.RegUInt16().AsFloat(1000));
            return voltageList;
        }
        public List<int> GetCellTemps()
        {
            //d6|03|0012|000f|b7ec  //GET 15 registers storing 15 cell temps
            //d6|03|1e|001b|001b|001b|001c|001c|001c|001c|001b|001b|001b|001b|001d|001d|001d|001d|

            var registers = GetRegisters("0012", HuaweiBatData.NumCells);
            var tempList = new List<int>();
            foreach (var v in registers.Values)
                tempList.Add(v.RegUInt16());
            return tempList;
        }

        
    }
    public class HuaweiBatData
    {
        public const int NumCells = 15;
        public float BusVoltage { get; set; } //0001
        public float BattVoltage { get; set; }//0002
        public float BattCurrent { get; set; }//0003
        public float BattPower { get; set; } //calculated
        public int   BattSOC { get; set; }      //0004
        public int   BattSOH { get; set; }      //0004
        public int   Discharges { get; set; }   //0042 (2 registers / 4 bytes)
        public string BarCode { get; set; }   //0211 (10 registers / 20 bytes, ascii encoded hex with ff padding.   sample: 455832303630303030353335ffffffffffffffff = "ES2060000535"

        public List<HuaweiBatCellData> Cells { get; set; } = new List<HuaweiBatCellData>();
        public static string GetCSVHeader()
        {
            var header = "Time,BusVoltage,BattVoltage,BattCurrent,BattSOC,BattSOH,Discharges,BarCode,";
            var cellHeaders = string.Join(',', Enumerable.Range(1, NumCells).Select(i => $"C{i}Volt,C{i}TempC"));

            return header + cellHeaders;
        }

        public string GetCSVRecordrecord()
        {
            var batData = $"{DateTime.Now},{BusVoltage},{BattVoltage},{BattCurrent},{BattSOC},{BattSOH},{Discharges},{BarCode},";
            var cellsData = string.Join(',', Cells.Select(c => $"{c.Volts},{c.TempC}"));
            return batData + cellsData;
        }
    }
    public class HuaweiBatCellData
    {
        public float Volts { get; set; } //15cell bat 48v -Regs: 0034 - 0048     
        public int TempC { get; set; }   //15cell bat 48v -Regs: 0018 - 0032
    }

}
