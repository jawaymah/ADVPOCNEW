using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Logic
{
    public class LiverRollerConversionManager : ConversionManager
    {
        public new List<FamilyInstance> ConvertToDetail(FamilyInstance generic)
        {
            return null;
        }

        public new FamilyInstance ConvertBackToGeneric(List<FamilyInstance> detailedFamilies)
        {
            return null;
        }
    }
}
