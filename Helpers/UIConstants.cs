using System.IO;
using System.Reflection;

namespace AdvansysPOC
{
    public static class UIConstants
    {
        public static string ButtonIconsFolder
        {
            get
            {
                return AddinPath + "\\Images\\";
            }
        }
        public static string ButtonFamiliesFolder
        {
            get
            {
                return AddinPath + "\\Families\\";
            }
        }
        public static string AssemblyPath
        {
            get
            {
                return Assembly.GetExecutingAssembly().Location;
            }
        }

        public static string AddinPath
        {
            get
            {
                return Path.GetDirectoryName(AssemblyPath);
            }
        }
    }
}
