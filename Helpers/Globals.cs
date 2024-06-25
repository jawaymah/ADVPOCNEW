using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC
{
    public static class Globals
    {
        public static Document Doc { get; set; }

        public static readonly string TempFolder = Environment.GetEnvironmentVariable("TMP", EnvironmentVariableTarget.User);
        public static string APOCFolderPath = Path.Combine(TempFolder, "APOC");
        public static string SettingsPath = Path.Combine(APOCFolderPath, "Settings");
        public static string ViewportsDataPath = Path.Combine(APOCFolderPath, "Viewports");

        //TODO
        //public static string APOCAppBaseUrl = "https://AdvansysPOCbim.azurewebsites.net/";
        //public static string SaveSpacesEndpoint = $"{APOCAppBaseUrl}api/saveForgeSpaces/";
        //public static string LicenseUrl = $"{APOCAppBaseUrl}api/revitlicense/";

        public static void SetupCurrentDocument(Document doc)
        {
            ParametersHelper.SetupProjectIfNeeded(doc);
            Doc = doc;
        }
    }
}
