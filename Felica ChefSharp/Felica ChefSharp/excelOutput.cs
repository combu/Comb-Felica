using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using System.IO;

namespace Felica_ChefSharp
{
    class excelOutput
    {
        public struct outPutExel{
            public string name;
            public DateTime dateTime;
            public string usePC;
        }
        static public bool excel_OutPutEx(List<outPutExel> writeData, string writePath)
        {
            string[] title = new string[] { "名前", "日時", "使用したPC"};
            try
            {
                Encoding enc = Encoding.UTF8;
                StreamWriter sw = new StreamWriter(writePath, false, enc);
                sw.WriteLine("{0},{1},{2},", title[0], title[1], title[2]);

                foreach (outPutExel write in writeData)
                {
                    sw.WriteLine("{0},{1},{2},", write.name, write.dateTime, write.usePC);
                }

                sw.Close();

                return true;
            }
            catch (Exception e)
            {
                return false;
            }
        }
    }
}
