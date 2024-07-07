using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data;
using Microsoft.Office.Interop.Excel;
using Excel = Microsoft.Office.Interop.Excel;
using System.Drawing;
using AdvansysPOC.UI;
using System.IO;

namespace AdvansysPOC.Logic
{
    internal static class CalculationsManager
    {
        static HPCalculationsView _calculationsView;
        const string filename = "HPCALC.xlsx";
        static string filePath = new Uri(Path.Combine(UIConstants.FilsFolder, filename), UriKind.Absolute).AbsolutePath;

        #region Cells numbers
        const int DEFAULT_LIVE_LOAD_ROW = 7;
        const string DEFAULT_LIVE_LOAD_COLUMN = "E";

        const int LENGTH_ROW = 12;
        const string LENGTH_COLUMN = "F";

        const int SPEED_ROW = 13;
        const string SPEED_COLUMN = "F";

        const int LIVE_LOAD_OVERRID_ROW = 14;
        const string LIVE_LOAD_OVERRID_COLUMN = "F";

        const int ROLLER_CENTERS_ROW = 15;
        const string ROLLER_CENTERS_COLUMN = "F";

        const int DIRECT_DRIVE_ROW = 17;
        const string DIRECT_DRIVE_COLUMN = "F";

        const int ADJUSTABLE_PRESSURE_CONVEYOR_ROW = 18;
        const string ADJUSTABLE_PRESSURE_CONVEYOR_COLUMN = "F";

        const int SLUG_LENGTH_ROW = 20;
        const string SLUG_LENGTH_COLUMN = "F";

        const int BELT_PULL_OF_SLAVED_CONVEYOR_ROW = 21;
        const string BELT_PULL_OF_SLAVED_CONVEYOR_COLUMN = "F";

        const string CALCULATION_RANGE = "A9:T24";

        #region Results cells
        const int DRIVE_SIZE_ROW = 17;
        const string DRIVE_SIZE_COLUMN = "R";
        const int HP_ROW = 18;
        const string HP_COLUMN = "R";

        const int LONG_SPRING_ROW = 19;
        const string LONG_SPRING_COLUMN = "R";

        const int SHORT_SPRING_ROW = 20;
        const string SHORT_SPRING_COLUMN = "R";

        const int EBP_ROW = 15;
        const string EBP_COLUMN = "H";

        //const string HSBP_CELL = "H17";
        //const string EHP_CELL = "H19";
        //const string ACTUAL_TORQUE_CELL = "J23";
        //const string MAX_TORQUE_CELL = "N23";
        //const string DIAMETER_CELL = "J24";
        //const string EFFECTIVE_SF_CELL = "N24";
        //const string ERRORS_CELLS = "P17:P24";
        //const string RESULTS_CELLS = "R17:P24";
        //const string RESULTS_CELLS2 = "S1:S20";
        #endregion



        #endregion

        public static void AssignInputParameters(Excel.Worksheet xlWorkSheet, double length, double rollerCenters,
                                                    double defaultLiveLoad = 25,
                                                    double speed = 200,
                                                    double liveLoadOveeride = 0,
                                                    double slugLength = 0,
                                                    string adjustablePressureConveyor = "N",
                                                    double beltPullOfSlavedConveyor = 0,
                                                    string directDrive = "Dodge")
        {

            xlWorkSheet.Cells[DEFAULT_LIVE_LOAD_ROW, DEFAULT_LIVE_LOAD_COLUMN] = defaultLiveLoad;
            xlWorkSheet.Cells[LENGTH_ROW, LENGTH_COLUMN] = length;
            xlWorkSheet.Cells[SPEED_ROW, SPEED_COLUMN] = speed;
            xlWorkSheet.Cells[LIVE_LOAD_OVERRID_ROW, LIVE_LOAD_OVERRID_COLUMN] = liveLoadOveeride;
            xlWorkSheet.Cells[ROLLER_CENTERS_ROW, ROLLER_CENTERS_COLUMN] = rollerCenters;
            xlWorkSheet.Cells[SLUG_LENGTH_ROW, SLUG_LENGTH_COLUMN] = slugLength;

            xlWorkSheet.Cells[ADJUSTABLE_PRESSURE_CONVEYOR_ROW, ADJUSTABLE_PRESSURE_CONVEYOR_COLUMN] = adjustablePressureConveyor;
            xlWorkSheet.Cells[BELT_PULL_OF_SLAVED_CONVEYOR_ROW, BELT_PULL_OF_SLAVED_CONVEYOR_COLUMN] = beltPullOfSlavedConveyor;
            xlWorkSheet.Cells[DIRECT_DRIVE_ROW, DIRECT_DRIVE_COLUMN] = directDrive;

            xlWorkSheet.Calculate();


        }

        public static LiveRollerCalculationResult GetLiveRollerCalculationResult(double length, double rollerCenters)
        {
            LiveRollerCalculationResult result = new LiveRollerCalculationResult();
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                //xlApp.Visible = true;
                xlWorkBook = xlApp.Workbooks.Open(filePath);

                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                AssignInputParameters(xlWorkSheet, length, rollerCenters);

                if (double.TryParse((xlWorkSheet.Cells[EBP_ROW, EBP_COLUMN] as Excel.Range).Text.ToString(), out double ebpValue))
                {
                    result.EBP = ebpValue;
                }

                string HPString = (xlWorkSheet.Cells[HP_ROW, HP_COLUMN] as Excel.Range).Text.ToString();
                if (!string.IsNullOrEmpty(HPString))
                {
                    result.HP = double.Parse(HPString.Replace("HP", "").TrimEnd());
                }

                if (double.TryParse((xlWorkSheet.Cells[LONG_SPRING_ROW, LONG_SPRING_COLUMN] as Excel.Range).Text.ToString(), out double longSpring))
                {
                    result.EBP = longSpring;
                }

                if (double.TryParse((xlWorkSheet.Cells[SHORT_SPRING_ROW, SHORT_SPRING_COLUMN] as Excel.Range).Text.ToString(), out double shortSpring))
                {
                    result.EBP = shortSpring;
                }

                result.DriveSize = (xlWorkSheet.Cells[DRIVE_SIZE_ROW, DRIVE_SIZE_COLUMN] as Excel.Range).Text.ToString();
            }
            catch (Exception e)
            {

            }
            finally
            {
                xlWorkBook.Close();
                xlApp.Quit();
            }
            return result;
        }


        public static void DisplayLiveRollerCalculation(double length, double rollerCenters)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                //xlApp.Visible = true;
                xlWorkBook = xlApp.Workbooks.Open(filePath);

                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                AssignInputParameters(xlWorkSheet, 150, 2);
                dynamic x = xlWorkSheet.Range[CALCULATION_RANGE].CopyPicture(XlPictureAppearance.xlScreen, XlCopyPictureFormat.xlBitmap);
                bool b = System.Windows.Clipboard.ContainsImage();

                Image image = System.Windows.Forms.Clipboard.GetImage();
                if (_calculationsView != null && _calculationsView.IsVisible)
                {
                    _calculationsView.Hide();
                    _calculationsView = null;
                }
                _calculationsView = new HPCalculationsView();
                _calculationsView.AddImage(image);
                _calculationsView.Show();
            }
            catch (Exception e)
            {

            }
            finally
            {
                xlWorkBook.Close();
                xlApp.Quit();
            }
        }


        /// <summary>
        /// The result of the live roller drive calculation
        /// </summary>
        public class LiveRollerCalculationResult
        {
            /// <summary>
            /// Horse Power
            /// </summary>
            public double HP { get; set; }

            /// <summary>
            /// Drice Size
            /// </summary>
            public string DriveSize { get; set; }

            /// <summary>
            /// Number of required long springs
            /// </summary>
            public int LongSpring { get; set; }

            /// <summary>
            /// Number of required short springs
            /// </summary>
            public int ShortSpring { get; set; }

            /// <summary>
            /// Efficient Belt Pull
            /// </summary>
            public double EBP { get; set; }
        }
    }
}
