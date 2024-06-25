using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Helpers
{
    public static class FamilyHelper
    {
        public static Family FindFamilyByName(Document doc, string familyName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Family));

            foreach (Family family in collector)
            {
                if (family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return family;
                }
            }

            return null;
        }

        public static void SetUnitId(this FamilyInstance instance)
        {
            //TODO
            ParametersHelper.SetParameter(instance, ParametersConstants.ConveyorNumber, "1001" /*ParametersHelper.GetLastUnitId()*/);
            //ParametersHelper.SetLastUnitId();
        }
    }
}
