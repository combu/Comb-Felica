using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Felica_ChefSharp
{
    class excelOutput
    {
        public struct outPutExel{
            public string name;
            public DateTime dateTime;
            public string usePC;
        }
        static public void excel_OutPutEx(List<outPutExel> writeData, string writePath)
        {
            Application ExcelApp = new Application();
            ExcelApp.DisplayAlerts = false;
            ExcelApp.Visible = false;
            Workbook wb = ExcelApp.Workbooks.Add();

            Worksheet ws1 = wb.Sheets[1];
            ws1.Select(Type.Missing);
            //[縦, 横]

            Range rgn = ws1.Cells[1, 1];
            rgn.Value = "名前";
            rgn = ws1.Cells[1, 2];
            rgn.Value = "日時";
            rgn = ws1.Cells[1, 3];
            rgn.Value = "使用したPC";

            int i = 1;
            foreach (outPutExel write in writeData)
            {
                ++i;

                rgn = ws1.Cells[i, 1];
                rgn.Value = write.name;

                rgn = ws1.Cells[i, 2];
                rgn.Value = write.dateTime;

                rgn = ws1.Cells[i, 3];
                rgn.Value = write.usePC;
            }

            ExcelApp.Rows[1].AutoFit();
            ExcelApp.Rows[2].AutoFit();
            ExcelApp.Rows[3].AutoFit();


            ws1.StandardWidth = 18;

            wb.SaveAs(writePath);
            wb.Close(false);
            ExcelApp.Quit();
        }
    }
}
