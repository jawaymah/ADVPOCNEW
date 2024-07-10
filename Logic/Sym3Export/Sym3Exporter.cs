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
using System.Diagnostics;
using Autodesk.Revit.DB;

namespace AdvansysPOC.Logic.Sym3Export
{
    public class Sym3Exporter
    {
        public static void RunTestFile()
        {
            string filePath = @"D:\testoutput2.xlsx";

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

                FileInfo excelfile = new FileInfo(filePath);
                // Save the Excel file
                //FileInfo excelFile = new FileInfo(IOHelper.GetSaveFilePath(filePath));
                package.SaveAs(excelfile);

                Process.Start(filePath);
            }

            Console.WriteLine($"Excel file '{filePath}' created successfully.");
        }

        public static void RunTestFile2()
        {
            string filePath = @"D:\testoutput2.xlsx";

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

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var utf8 = Encoding.GetEncoding("UTF-8");
                // Save the Excel file with UTF-8 encoding
                FileInfo excelFile = new FileInfo(filePath);
                //using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                //{
                //    package.SaveAs(fileStream);
                //}

                // Save to a MemoryStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    // Write MemoryStream to file
                    File.WriteAllBytes(filePath, memoryStream.ToArray());
                }

                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                Console.WriteLine($"Excel file '{filePath}' created successfully.");
            }
        }

        public static void Run()
        {
            List<DetailedUnit>  units = OrderBeds();
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

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var utf8 = Encoding.GetEncoding("UTF-8");
                // Save the Excel file with UTF-8 encoding
                FileInfo excelFile = new FileInfo(filePath);
                //using (var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                //{
                //    package.SaveAs(fileStream);
                //}

                // Save to a MemoryStream
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);

                    // Write MemoryStream to file
                    File.WriteAllBytes(filePath, memoryStream.ToArray());
                }

                Process.Start(new ProcessStartInfo(filePath) { UseShellExecute = true });
                Console.WriteLine($"Excel file '{filePath}' created successfully.");
            }

            Console.WriteLine($"Excel file '{filePath}' created successfully.");
        }

        public static List<DetailedUnit> OrderBeds()
        {
            List<DetailedUnit> units = GetUnits().OrderBy(s => s.unitId).ToList();
            foreach (var unit in units)
            {
                SetPrevandNextUnit(units, unit);
            }
            return units;
        }
        public static List<AssemblyInstance> CollectAssemblies()
        {
            List<AssemblyInstance> instances = new List<AssemblyInstance>();
            var lSpools = new FilteredElementCollector(Globals.Doc).OfClass(typeof(AssemblyInstance)).ToElements();
            foreach (AssemblyInstance unit in lSpools)
            {
                Parameter para = unit.LookupParameter(Constants.ConveyorNumber);
                if (para != null)
                {
                    instances.Add(unit);
                }
            }
            return instances;
        }
        public static List<DetailedUnit> GetUnits()
        {
            List < DetailedUnit > detailedBeds = new List<DetailedUnit>();
            foreach (var assembly in CollectAssemblies())
            {
                detailedBeds.Add(new DetailedUnit(assembly));
            }
            return detailedBeds;
        }
        public static void SetPrevandNextUnit(List<DetailedUnit> units, DetailedUnit unit)
        {
            foreach (var item in units)
            {
                if (item.unitId != unit.unitId)
                {
                    if (unit.PrevUnit == null)
                    {
                        if (unit.StartPoint.DistanceTo(item.EndPoint) < 0.01)
                        {
                            unit.PrevUnit = item;
                            item.NextUnit = unit;
                        }
                    }
                    if (unit.NextUnit == null)
                    {
                        if (unit.EndPoint.DistanceTo(item.StartPoint) < 0.01)
                        {
                            unit.NextUnit = item;
                            item.PrevUnit = unit;
                        }
                            
                    }
                }
            }


        }
    }
}
