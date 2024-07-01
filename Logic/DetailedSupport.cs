using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdvansysPOC.Logic
{
    public class DetailedSupport
    {
        public XYZ Location { get; set; }
        public Floor Floor { get; set; }
        public FamilyInstance Bed { get; set; }
        public double Height { get; set; }

        public DetailedSupport(FamilyInstance bed, XYZ loc)
        {
            Bed = bed;
            Location = loc;
        }

        public void setBelowFloor()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Globals.Doc);
            collector.OfClass(typeof(Floor));
            double diff = double.MaxValue;
            foreach (Floor f in collector)
            {
                XYZ proj = f.GetVerticalProjectionPoint(Location, FloorFace.Top);
                if (proj.Z <= Location.Z)
                {
                    if (Location.DistanceTo(proj) < diff)
                    {
                        diff = Location.DistanceTo(proj);
                        Floor = f;
                    }
                }
            }
            Height = diff;
        }
        public void setBelowFloor(List<Floor> collection)
        { 
            double diff = double.MaxValue;
            foreach (Floor f in collection)
            {
                XYZ proj = f.GetVerticalProjectionPoint(Location, FloorFace.Top);
                if (proj.Z <= Location.Z)
                {
                    if (Location.DistanceTo(proj) < diff)
                    {
                        diff = Location.DistanceTo(proj);
                        Floor = f;
                    }
                }
            }

        }
    }
}
