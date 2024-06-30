using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Logic
{
    public class DetailedUnit
    {
        public List<DetailedBed> Beds { get; set; }
        public double TotalLength { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }

        public DetailedUnit()
        {
            Beds = new List<DetailedBed>();
        }
    }
}
