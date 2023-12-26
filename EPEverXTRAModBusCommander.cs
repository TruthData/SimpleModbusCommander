using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SimpleModBusCommander
{

    public class EPEverXTRAModBusCommander : ModBusCommander
    {
        //Epever spec says 11520 baud, and uses 0x04 as the function code to get registers.
        public EPEverXTRAModBusCommander(string port = "COM1", int baud = 115200, ushort deviceAddress = 1) : base(port, baud, deviceAddress,0x04)
        {
        }
        public EPEverXTRAData GetData(int deviceID=1)
        {
            var data = new EPEverXTRAData();

            //get registers  3100 - 311D    and parse out important data.
            var registers = GetRegisters("3100", 4);
            
            data.PV_Volts       = registers["3100"].RegUInt16().AsFloat();
            data.PV_Current     = registers["3101"].RegUInt16().AsFloat();
            data.PV_Power       = registers["3102"].Cat(registers["3103"]).RegUInt32().AsFloat();

            registers = GetRegisters("310C", 6);
            data.Load_Volts     = registers["310C"].RegUInt16().AsFloat();
            data.Load_Current   = registers["310D"].RegUInt16().AsFloat();
            data.Load_Power     = registers["310E"].Cat(registers["310F"]).RegUInt32().AsFloat();

            data.Bat_Temp       = registers["3110"].RegUInt16().AsFloat();
            data.Charger_Temp   = registers["3111"].RegUInt16().AsFloat();

            //Get the 3 status registers
            registers = GetRegisters("3200", 3);


            //Bat_Status:   D3 - D0: 00H Normal,01H OverVoltage. , 02H Under Voltage, 03H Over discharge, 04H Fault
            data.Bat_Status_WrongRatedVoltage = registers["3200"].RegHasFlag(15);
            data.Bat_Status_InternalResistanceAbnormal = registers["3200"].RegHasFlag(8);
            switch (registers["3200"].RegBitString(4, 7))
            {
                case "0000": data.Bat_Status_Temp = "Normal"; break;
                case "0001": data.Bat_Status_Temp = "OverTemp"; break;
                case "0010": data.Bat_Status_Temp = "UnderTemp"; break;
                default: data.Bat_Status = "Unknown Status"; break;
            };
            switch (registers["3200"].RegBitString(0, 3))
            {
                case "0000": data.Bat_Status = "Normal"; break;
                case "0001": data.Bat_Status = "OverVoltage"; break;
                case "0010": data.Bat_Status = "UnderVoltage"; break;
                case "0011": data.Bat_Status = "OverDischarge"; break;
                case "0100": data.Bat_Status = "Fault"; break;
                default:     data.Bat_Status = "Unknown Status"; break;
            };

            
            //Charger Status

            switch (registers["3201"].RegBitString(14, 15)) 
            {
                case "00": data.Charger_Status_PV = "Normal"; break;
                case "01": data.Charger_Status_PV = "Disconected"; break;
                case "10": data.Charger_Status_PV = "OverVoltage"; break;
                case "11": data.Charger_Status_PV = "Error"; break;
                default:   data.Charger_Status_PV = "Unknown Status"; break;
            };
            data.Charger_Status_MOSFET_ErrorFlags = registers["3201"].RegBitString(11,13);
            data.Charger_Status_PV_OverCurrent = registers["3201"].RegHasFlag(10);
            data.Charger_Status_Load_OverCurrent = registers["3201"].RegHasFlag(9);
            data.Charger_Status_Load_ShortCircuit = registers["3201"].RegHasFlag(8);
            data.Charger_Status_Load_MosfetShort = registers["3201"].RegHasFlag(7);
            
            data.Charger_Status_Disequilibrium3Circuits = registers["3201"].RegHasFlag(6);
            data.Charger_Status_PvShortCircuit          = registers["3201"].RegHasFlag(4);
            switch (registers["3201"].RegBitString(2, 3)) //D3 - D2: Charging status. 00H Nocharging,01H Float,02H Boost, 03HEqualization.
            {
                case "00": data.Charger_Status = "Not Charging"; break;
                case "01": data.Charger_Status = "FLOAT"; break;
                case "10": data.Charger_Status = "BOOST"; break;
                case "11": data.Charger_Status = "EQUALIZATION"; break;
                default:   data.Charger_Status = "Unknown Status"; break;
            };
            data.Charger_Status_HasFault = registers["3201"].RegHasFlag(1);
            data.Charger_Status_IsRunning = registers["3201"].RegHasFlag(0);

            //Load Status
            data.DischargeEquiptment_Status = registers["3201"].RegBitString();
            Log($"DischargeEquiptment_Status: {data.DischargeEquiptment_Status}");


            //Stats
            registers = GetRegisters("3302", 18);
            data.Bat_Volts_TodayMax = registers["3302"].RegUInt16().AsFloat();
            data.Bat_Volts_TodayMin = registers["3303"].RegUInt16().AsFloat();
            
            //Consumed KWH
            data.Consumed_KWH_Today = registers["3304"].Cat(registers["3305"]).RegUInt32().AsFloat();
            data.Consumed_KWH_Month = registers["3306"].Cat(registers["3307"]).RegUInt32().AsFloat();
            data.Consumed_KWH_Year  = registers["3308"].Cat(registers["3309"]).RegUInt32().AsFloat();
            data.Consumed_KWH_Total = registers["330A"].Cat(registers["330B"]).RegUInt32().AsFloat();
            //Generated KWH
            data.Generated_KWH_Today = registers["330C"].Cat(registers["330D"]).RegUInt32().AsFloat();
            data.Generated_KWH_Month = registers["330E"].Cat(registers["330F"]).RegUInt32().AsFloat();
            data.Generated_KWH_Year  = registers["3310"].Cat(registers["3311"]).RegUInt32().AsFloat();
            data.Generated_KWH_Total = registers["3312"].Cat(registers["3313"]).RegUInt32().AsFloat();
            
            
            registers = GetRegisters("331A", 3);
                        data.Bat_Volts      = registers["331A"].RegUInt16().AsFloat();
            data.Bat_Current    = registers["331B"].Cat(registers["331C"]).RegUInt32().AsFloat();
            data.Bat_Power = data.Bat_Volts * data.Bat_Current;
            return data;

        }
       
        
    }
    public class EPEverXTRAData
    {

        public float PV_Volts { get; set; }
        public float PV_Current { get; set; }
        public float PV_Power { get; set; }
        public float Load_Volts { get; set; }
        public float Load_Current { get; set; }
        public float Load_Power { get; set; }

        public float Bat_Volts { get; set; }
        public float Bat_Current { get; set; }
        public float Bat_Power { get; set; }

        public string Bat_Status { get; set; }
        public string Charger_Status { get; set; }
        public string DischargeEquiptment_Status { get; set; }
        
        public float Bat_Temp { get; internal set; }
        public float Charger_Temp { get; internal set; }
        public bool Bat_Status_WrongRatedVoltage { get; internal set; }
        public bool Bat_Status_InternalResistanceAbnormal { get; internal set; }
        public string Bat_Status_Temp { get; internal set; }

        public string Charger_Status_PV { get; internal set; }
        public bool Charger_Status_HasFault { get; internal set; }
        public bool Charger_Status_PvShortCircuit { get; internal set; }
        public bool Charger_Status_Disequilibrium3Circuits { get; internal set; }
        public string Charger_Status_MOSFET_ErrorFlags { get; internal set; }
        public bool Charger_Status_PV_OverCurrent { get; internal set; }
        public bool Charger_Status_Load_OverCurrent { get; internal set; }
        public bool Charger_Status_Load_ShortCircuit { get; internal set; }
        public bool Charger_Status_Load_MosfetShort { get; internal set; }
        public bool Charger_Status_IsRunning { get; internal set; }

        public float Bat_Volts_TodayMax { get; internal set; }
        public float Bat_Volts_TodayMin { get; internal set; }
        
        public float Consumed_KWH_Today { get; internal set; }
        public float Consumed_KWH_Month { get; internal set; }
        public float Consumed_KWH_Year { get; internal set; }
        public float Consumed_KWH_Total { get; internal set; }
        public float Generated_KWH_Today { get; internal set; }
        public float Generated_KWH_Month { get; internal set; }
        public float Generated_KWH_Year { get; internal set; }
        public float Generated_KWH_Total { get; internal set; }        
    }
   

}
