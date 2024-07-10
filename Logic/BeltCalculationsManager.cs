using AdvansysPOC.UI;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;
using AdvansysPOC.UI;
using System.IO;
using static AdvansysPOC.Logic.CalculationsManager;

namespace AdvansysPOC.Logic
{
    internal class BeltCalculationsManager
    {
        const string filename = "BeltLength.xlsx";
        static string filePath = new Uri(Path.Combine(UIConstants.FilsFolder, filename), UriKind.Absolute).AbsolutePath;

        #region Cells numbers
        const int Unit_NUMBER_ROW = 4;
        const string Unit_NUMBER_COLUMN = "D";

        const int BED_WIDTH_ROW = 6;
        const string BED_WIDTH_COLUMN = "D";

        const int CONVEYOR_TYPE_ROW = 7;
        const string CONVEYOR_TYPE_COLUMN = "D";

        const int TE18BedQuantity_ROW = 10;
        const string TE18BedQuantity_COLUMN = "D";

        const int TE30BedQuantity_ROW = 11;
        const string TE30BedQuantity_COLUMN = "D";

        const int TE42BedQuantity_ROW = 12;
        const string TE42BedQuantity_COLUMN = "D";

        const int CD6Quantity_ROW = 13;
        const string CD6Quantity_COLUMN = "D";

        const int CD8Quantity_ROW = 14;
        const string CD8Quantity_COLUMN = "D";

        const int CD10Quantity_ROW = 15;
        const string CD10Quantity_COLUMN = "D";

        const int INT_BEDS_LENGTH_ROW = 24;
        const string INT_BEDS_LENGTH_COLUMN = "D";

        const int EXTRALENGTH_ROW = 25;
        const string EXTRA_LENGTH_COLUMN = "D";



        const int BELT_TYPE_ROW = 27;
        const string BELT_TYPE_COLUMN = "D";

        const int BELT_WIDTH_ROW = 28;
        const string BELT_WIDTH_COLUMN = "D";

        const int BELT_LENGTH_ROW = 29;
        const string BELT_LENGTH_COLUMN = "D";



        const string CALCULATION_RANGE = "A4:D29";
        #endregion


        public static void AssignInputParameters(Excel.Worksheet xlWorkSheet, BeltCalculationInputs inputs)
        {

            xlWorkSheet.Cells[Unit_NUMBER_ROW, Unit_NUMBER_COLUMN] = inputs.ConveyorNumber;
            xlWorkSheet.Cells[BED_WIDTH_ROW, BED_WIDTH_COLUMN] = inputs.BedWidth;
            xlWorkSheet.Cells[CONVEYOR_TYPE_ROW, CONVEYOR_TYPE_COLUMN] = inputs.ConveyorType;

            xlWorkSheet.Cells[TE18BedQuantity_ROW, TE18BedQuantity_COLUMN] = inputs.TE18BedQuantity;
            xlWorkSheet.Cells[TE30BedQuantity_ROW, TE30BedQuantity_COLUMN] = inputs.TE30BedQuantity;
            xlWorkSheet.Cells[TE42BedQuantity_ROW, TE42BedQuantity_COLUMN] = inputs.TE42BedQuantity;

            xlWorkSheet.Cells[CD6Quantity_ROW, CD6Quantity_COLUMN] = inputs.CD6Quantity;
            xlWorkSheet.Cells[CD8Quantity_ROW, CD8Quantity_COLUMN] = inputs.CD8Quantity;
            xlWorkSheet.Cells[CD10Quantity_ROW, CD10Quantity_COLUMN] = inputs.CD10Quantity;

            xlWorkSheet.Cells[INT_BEDS_LENGTH_ROW, INT_BEDS_LENGTH_COLUMN] = inputs.InterBedsLength;
            xlWorkSheet.Cells[EXTRALENGTH_ROW, EXTRA_LENGTH_COLUMN] = inputs.ExtraLength;

            xlWorkSheet.Calculate();


        }

        //public static LiveRollerCalculationResult GetLiveRollerCalculationResult(double length, double rollerCenters)
        //{
        //    LiveRollerCalculationResult result = new LiveRollerCalculationResult();
        //    Microsoft.Office.Interop.Excel.Application xlApp = null;
        //    Excel.Workbook xlWorkBook = null;
        //    try
        //    {
        //        xlApp = new Microsoft.Office.Interop.Excel.Application();
        //        xlWorkBook = xlApp.Workbooks.Open(filePath);

        //        Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
        //        AssignInputParameters(xlWorkSheet, length, rollerCenters);

        //        if (double.TryParse((xlWorkSheet.Cells[EBP_ROW, EBP_COLUMN] as Excel.Range).Text.ToString(), out double ebpValue))
        //        {
        //            result.EBP = ebpValue;
        //        }

        //        string HPString = (xlWorkSheet.Cells[HP_ROW, HP_COLUMN] as Excel.Range).Text.ToString();
        //        if (!string.IsNullOrEmpty(HPString) && HPString.Contains(" HP"))
        //        {
        //            result.HP = double.Parse(HPString.Replace("HP", "").TrimEnd());
        //        }

        //        if (int.TryParse((xlWorkSheet.Cells[LONG_SPRING_ROW, LONG_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int longSpring))
        //        {
        //            result.LongSpring = longSpring;
        //        }

        //        if (int.TryParse((xlWorkSheet.Cells[SHORT_SPRING_ROW, SHORT_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int shortSpring))
        //        {
        //            result.ShortSpring = shortSpring;
        //        }

        //        result.DriveSize = (xlWorkSheet.Cells[DRIVE_SIZE_ROW, DRIVE_SIZE_COLUMN] as Excel.Range).Text.ToString();
        //    }
        //    catch (Exception e)
        //    {

        //    }
        //    finally
        //    {
        //        xlWorkBook.Close(SaveChanges: false);
        //        xlApp.Quit();
        //    }
        //    return result;
        //}


        public static bool DisplayBeltCalculation(List<BeltCalculationInputs> inputs)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(filePath);

                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                List<Image> images = new List<Image>();
                for (int i = 0; i < inputs.Count; i++)
                {
                    AssignInputParameters(xlWorkSheet, inputs[i]);
                    dynamic x = xlWorkSheet.Range[CALCULATION_RANGE].CopyPicture(XlPictureAppearance.xlScreen, XlCopyPictureFormat.xlBitmap);
                    if (System.Windows.Clipboard.ContainsImage())
                    {
                        images.Add(System.Windows.Forms.Clipboard.GetImage());
                    }
                }
                if (HPCalculationsView.Instance != null && HPCalculationsView.Instance.IsVisible)
                {
                    HPCalculationsView.Instance.Close();
                }
                HPCalculationsView calculationsView = new HPCalculationsView();
                calculationsView.AddImages(images);
                calculationsView.Show();
            }
            catch (Exception e)
            {
                return false;
            }
            finally
            {
                xlWorkBook.Close(SaveChanges: false);
                xlApp.Quit();
            }
            return true;
        }


        /// <summary>
        /// The inputs of the live roller drive calculation
        /// </summary>
        public class BeltCalculationInputs : LiveRollerCalculationInputs
        {
            /// <summary>
            /// 
            /// </summary>
            public double BedWidth { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public string ConveyorType { get; set; } = "PE";

            /// <summary>
            /// 
            /// </summary>
            public int TE18BedQuantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int TE30BedQuantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int TE42BedQuantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int CD6Quantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int CD8Quantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public int CD10Quantity { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double InterBedsLength { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double ExtraLength { get; set; } = 24;


        }

        /// <summary>
        /// The result of the live roller drive calculation
        /// </summary>
        public class BeltCalculationResult
        {
            /// <summary>
            /// 
            /// </summary>
            public string BeltType { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double BeltWidth { get; set; }

            /// <summary>
            /// 
            /// </summary>
            public double BeltLength { get; set; }
        }
    }
}
