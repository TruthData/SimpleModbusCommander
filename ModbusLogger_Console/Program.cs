using System.Collections;
using System.Data;

namespace SimpleModBusCommander
{
    internal class Program
    {
        
        private static void Main(string[] args)
        {


            var batClient = new HuaweiModBusCommander("COM6", 214);
            batClient.EstablishConnection().Assert("Failed to connect to the Huawei battery... try again.");
            var ccClient = new EPEverXTRAModBusCommander("COM5");
            try
            {
                while (!Console.KeyAvailable)
                {
                    var batData = batClient.GetData();
                    logData(batData, ".\\BatData.log");

                    var ccData = ccClient.GetData();
                    logData(ccData, ".\\ccData.log");
                    
                    Thread.Sleep(1000);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            finally {
                batClient.SP.Close();
                ccClient.SP.Close();
            }

        }
        
        public static void logData(Object data, string file)
        {
            var jsonData = DateTime.Now.ToString() + "\t" + data.ToJson();
            Console.WriteLine(jsonData);
            File.AppendAllText(file,jsonData + "\r\n");
        }
       
    }
}