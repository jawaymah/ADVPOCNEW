using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OfficeOpenXml;
using System;
using System.IO;

namespace AdvansysPOC.Logic.Sym3Export
{
    public class Sym3Exporter
    {
        static void RunTestFile()
        {
            string filePath = "output.xlsx";

            // Create Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("in");

                // Define headers
                string[] headers = { "OBJECTNAME", "X", "Y", "Z", "Length", "Width", "DIRECTION", "TYPE", "Speed", "CONNECTION", "AUX" };

                // Populate headers in the first row
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Data rows
                string[,] data = {
                {"LRPE01", "0", "0", "0", "", "", "LRPE", "", "CURVE01", ""},
                {"MRG01", "-20", "-12", "0", "", "", "90", "BELT", "LRPE01", "MERGE|PE:11,MRG01_PE"},
                {"MRG02", "-10", "-12", "0", "", "", "90", "BELT", "LRPE01", "MERGE|PE:11,MRG02_PE"},
                {"MRG03", "0", "-12", "0", "", "", "90", "BELT", "LRPE01", "MERGE|PE:11,MRG03_PE"},
                {"CURVE01", "10", "0", "0", "", "", "0", "CURVE", "INCU", "RADIUS:4|ARC:90"},
                {"INCU", "14", "4", "0", "", "", "90", "BELT", "INCTILTBLT", "PE:2,INCU_PE"},
                {"INCTILTBLT", "14", "7", "0", "", "", "90", "BELT", "INCD", ""}
            };

                // Populate data rows starting from the second row
                int rowCount = data.GetLength(0);
                int colCount = data.GetLength(1);
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = data[row, col];
                    }
                }

                // Save the Excel file
                FileInfo excelFile = new FileInfo(IOHelper.GetSaveFilePath(filePath));
                package.SaveAs(excelFile);
            }

            Console.WriteLine($"Excel file '{filePath}' created successfully.");
        }
        public static void Run(List<DetailedUnit> units)
        {
            string filePath = "output.xlsx";

            // Create Excel package
            using (var package = new ExcelPackage())
            {
                // Add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("in");

                // Define headers
                string[] headers = { "OBJECTNAME", "X", "Y", "Z", "Length", "Width", "DIRECTION", "TYPE", "Speed", "CONNECTION", "AUX" };

                // Populate headers in the first row
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1].Value = headers[i];
                }

                // Data rows
                string[,] data = new string[units.Count, headers.Length];
                for (int i = 0; i < units.Count; i++)
                {
                    List<string> unitSym3 = units[i].ExportToSym3();
                    for (int j = 0; j < headers.Length; j++)
                    {
                        data[i, j] = unitSym3[j];
                    }
                }

                // Populate data rows starting from the second row
                int rowCount = data.GetLength(0);
                int colCount = data.GetLength(1);
                for (int row = 0; row < rowCount; row++)
                {
                    for (int col = 0; col < colCount; col++)
                    {
                        worksheet.Cells[row + 2, col + 1].Value = data[row, col];
                    }
                }

                // Save the Excel file
                FileInfo excelFile = new FileInfo(IOHelper.GetSaveFilePath(filePath));
                package.SaveAs(excelFile);
            }

            Console.WriteLine($"Excel file '{filePath}' created successfully.");
        }
    }
}
