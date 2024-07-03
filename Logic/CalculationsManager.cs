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

namespace AdvansysPOC.Logic
{
    internal static class CalculationsManager
    {
        static HPCalculationsView _calculationsView;
        const string filePath = @"D:\work\AdvansysRevit\HP CALC.xlsx";

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

        #region Results cells
        //const string A_CELL = "H11";
        //const string B_CELL = "H13";
        //const string EBP_CELL = "H15";
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

        public static void DoLiveRollerCalculations(double defaultLiveLoad, double length,
                                             double speed, double liveLoadOveeride,
                                             double rollerCenters, double slugLength,
                                             string adjustablePressureConveyor,
                                             double beltPullOfSlavedConveyor,
                                             string directDrive = "Dodge")
        {
            Microsoft.Office.Interop.Excel.Application xlApp = null;
            Excel.Workbook xlWorkBook = null;
            try
            {
                xlApp = new Microsoft.Office.Interop.Excel.Application();
                //xlApp.Visible = true;
                xlWorkBook = xlApp.Workbooks.Open(filePath);
                
                Excel.Worksheet xlWorkSheet = (Excel.Worksheet)xlWorkBook.Worksheets.get_Item(2);
                if (xlWorkSheet == null) return;
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

                dynamic x = xlWorkSheet.Range["A9:T24"].CopyPicture(XlPictureAppearance.xlScreen, XlCopyPictureFormat.xlBitmap);
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

            xlWorkBook.Close(SaveChanges: false);
            xlApp.Quit();

        }
    }
}
