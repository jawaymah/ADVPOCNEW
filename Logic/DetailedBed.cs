using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Logic
{
    public class DetailedBed
    {
        public DetailedBed() { }
		public DetailedBed NextBed { get; set; }
        public DetailedBed PrevBed { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public BedType BedType { get; set; }

        public double GetLength()
        {
            return EndPoint.DistanceTo(StartPoint);
        }
    }
}
