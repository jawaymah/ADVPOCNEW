using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace AdvansysPOC.Logic
{
    public static class IOHelper
    {

        public static string GetSaveFilePath(string fileName= "Export.xlsx")
        {
            // Open file dialog to select where to save the Excel file
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Excel files (*.xlsx)|*.xlsx|All files (*.*)|*.*",
                Title = "Save Excel File",
                FileName = fileName
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }

            return null;
            //throw new InvalidOperationException("No file selected.");
        }
    }
}
