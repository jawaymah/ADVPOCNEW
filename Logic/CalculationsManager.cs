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
        const string filename = "HPCALC.xlsx";
        static string filePath = new Uri(Path.Combine(UIConstants.FilsFolder, filename), UriKind.Absolute).AbsolutePath;

        #region Cells numbers
        const int CONV_NUM_ROW = 12;
        const string CONV_NUM_COLUMN = "C";

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

        //public static void AssignInputParameters(Excel.Worksheet xlWorkSheet, int conveyorNumber, double length, double rollerCenters,
        //                                            double defaultLiveLoad = 25,
        //                                            double speed = 200,
        //                                            double liveLoadOveeride = 0,
        //                                            double slugLength = 0,
        //                                            string adjustablePressureConveyor = "N",
        //                                            double beltPullOfSlavedConveyor = 0,
        //                                            string directDrive = "Dodge")
        public static void AssignInputParameters(Excel.Worksheet xlWorkSheet, LiveRollerCalculationInputs input)
        {
            xlWorkSheet.Cells[CONV_NUM_ROW, CONV_NUM_COLUMN] = input.ConveyorNumber;
            xlWorkSheet.Cells[DEFAULT_LIVE_LOAD_ROW, DEFAULT_LIVE_LOAD_COLUMN] = input.DefaultLiveLoad;
            xlWorkSheet.Cells[LENGTH_ROW, LENGTH_COLUMN] = input.Length;
            xlWorkSheet.Cells[SPEED_ROW, SPEED_COLUMN] = input.Speed;
            xlWorkSheet.Cells[LIVE_LOAD_OVERRID_ROW, LIVE_LOAD_OVERRID_COLUMN] = input.LiveLoadOveride;
            xlWorkSheet.Cells[ROLLER_CENTERS_ROW, ROLLER_CENTERS_COLUMN] = input.RollerCenters;
            xlWorkSheet.Cells[SLUG_LENGTH_ROW, SLUG_LENGTH_COLUMN] = input.SlugLength;

            xlWorkSheet.Cells[ADJUSTABLE_PRESSURE_CONVEYOR_ROW, ADJUSTABLE_PRESSURE_CONVEYOR_COLUMN] = input.AdjustablePressureConveyor;
            xlWorkSheet.Cells[BELT_PULL_OF_SLAVED_CONVEYOR_ROW, BELT_PULL_OF_SLAVED_CONVEYOR_COLUMN] = input.BeltPullOfSlavedConveyor;
            xlWorkSheet.Cells[DIRECT_DRIVE_ROW, DIRECT_DRIVE_COLUMN] = input.DirectDrive;

            xlWorkSheet.Calculate();
        }

        static LiveRollerCalculationResult GetResults(Excel.Worksheet xlWorkSheet)
        {
            LiveRollerCalculationResult result = new LiveRollerCalculationResult();
            if (double.TryParse((xlWorkSheet.Cells[EBP_ROW, EBP_COLUMN] as Excel.Range).Text.ToString(), out double ebpValue))
            {
                result.EBP = ebpValue;
            }

            string HPString = (xlWorkSheet.Cells[HP_ROW, HP_COLUMN] as Excel.Range).Text.ToString();
            if (!string.IsNullOrEmpty(HPString) && HPString.Contains(" HP"))
            {
                result.HP = double.Parse(HPString.Replace("HP", "").TrimEnd());
            }

            if (int.TryParse((xlWorkSheet.Cells[LONG_SPRING_ROW, LONG_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int longSpring))
            {
                result.LongSpring = longSpring;
            }

            if (int.TryParse((xlWorkSheet.Cells[SHORT_SPRING_ROW, SHORT_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int shortSpring))
            {
                result.ShortSpring = shortSpring;
            }
            string driveSize = (xlWorkSheet.Cells[DRIVE_SIZE_ROW, DRIVE_SIZE_COLUMN] as Excel.Range).Text.ToString();
            if (!string.IsNullOrEmpty(driveSize)) result.DriveSize = driveSize.Replace(" DIRECT DRIVE", "");
            return result;
        }

        public static Tuple<LiveRollerCalculationResult, string> GetLiveRollerCalculationResult(LiveRollerCalculationInputs input)
        {
            string errorMessage = null;
            LiveRollerCalculationResult result = null;
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(filePath);

                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                AssignInputParameters(xlWorkSheet, input);

                //if (double.TryParse((xlWorkSheet.Cells[EBP_ROW, EBP_COLUMN] as Excel.Range).Text.ToString(), out double ebpValue))
                //{
                //    result.EBP = ebpValue;
                //}

                //string HPString = (xlWorkSheet.Cells[HP_ROW, HP_COLUMN] as Excel.Range).Text.ToString();
                //if (!string.IsNullOrEmpty(HPString) && HPString.Contains(" HP"))
                //{
                //    result.HP = double.Parse(HPString.Replace("HP", "").TrimEnd());
                //}

                //if (int.TryParse((xlWorkSheet.Cells[LONG_SPRING_ROW, LONG_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int longSpring))
                //{
                //    result.LongSpring = longSpring;
                //}

                //if (int.TryParse((xlWorkSheet.Cells[SHORT_SPRING_ROW, SHORT_SPRING_COLUMN] as Excel.Range).Text.ToString(), out int shortSpring))
                //{
                //    result.ShortSpring = shortSpring;
                //}
                //string driveSize = (xlWorkSheet.Cells[DRIVE_SIZE_ROW, DRIVE_SIZE_COLUMN] as Excel.Range).Text.ToString();
                //if (!string.IsNullOrEmpty(driveSize)) driveSize = driveSize.Replace(" DIRECT DRIVE", "");
                //result.DriveSize = driveSize;
                result = GetResults(xlWorkSheet);
            }
            catch (Exception e)
            {
                errorMessage = e.Message;
            }
            finally
            {
                if (xlWorkBook != null)
                {
                    xlWorkBook.Close(SaveChanges: false);
                }
                if (xlApp != null)
                {
                    xlApp.Quit();
                }
            }
            return Tuple.Create<LiveRollerCalculationResult, string>(result, errorMessage);
        }


        public static Tuple<List<LiveRollerCalculationResult>, string> DisplayLiveRollerCalculation(List<LiveRollerCalculationInputs> inputs)
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                xlWorkBook = xlApp.Workbooks.Open(filePath);

                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                List<Image> images = new List<Image>();
                List<LiveRollerCalculationResult> results = new List<LiveRollerCalculationResult>();
                for (int i = 0; i < inputs.Count; i++)
                {
                    AssignInputParameters(xlWorkSheet, inputs[i]);
                    results.Add(GetResults(xlWorkSheet));
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
                return Tuple.Create<List<LiveRollerCalculationResult>, string>(results, null);
            }
            catch (Exception e)
            {
                return Tuple.Create<List<LiveRollerCalculationResult>, string>(new List<LiveRollerCalculationResult>(), e.Message);
            }
            finally
            {
                if (xlWorkBook != null)
                    xlWorkBook.Close(SaveChanges: false);
                if (xlApp != null)
                    xlApp.Quit();
            }
            return null;
        }


        /// <summary>
        /// The inputs of the live roller drive calculation
        /// </summary>
        public class LiveRollerCalculationInputs
        {
            /// <summary>
            /// Conveyor unit Number
            /// </summary>
            public int ConveyorNumber { get; set; }

            /// <summary>
            /// Conveyor length
            /// </summary>
            public double Length { get; set; }

            /// <summary>
            /// Distance between conveyor rollers centers
            /// </summary>
            public double RollerCenters { get; set; } = 3;

            /// <summary>
            /// Default live load
            /// </summary>
            public double DefaultLiveLoad { get; set; } = 25;

            /// <summary>
            /// Conveyor speed
            /// </summary>
            public double Speed { get; set; } = 200;

            /// <summary>
            /// Conveyor live load override
            /// </summary>
            public double LiveLoadOveride { get; set; } = 0;

            /// <summary>
            /// Conveyor slug length
            /// </summary>
            public double SlugLength { get; set; } = 0;

            /// <summary>
            /// Adjustable Pressure Conveyor
            /// </summary>
            public string AdjustablePressureConveyor { get; set; } = "N";

            /// <summary>
            /// Belt Pull Of Slaved Conveyor (Conveyor pulled by this conveyor)
            /// </summary>
            public double BeltPullOfSlavedConveyor { get; set; } = 0;

            /// <summary>
            /// Is conveyor Direct Drive?
            /// </summary>
            public string DirectDrive { get; set; } = "Dodge";
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
