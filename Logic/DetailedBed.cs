using AdvansysPOC.Helpers;
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
		//public DetailedBed NextBed { get; set; }
  //      public DetailedBed PrevBed { get; set; }
        public XYZ StartPoint { get; set; }
        public XYZ EndPoint { get; set; }
        public BedType BedType { get; set; }

        public double GetLength()
        {
            return EndPoint.DistanceTo(StartPoint);
        }

        public FamilyInstance PlaceBed()
        {
            string error = "";
            string familyName = Constants.CTFFamilyName;
            string fileName = Constants.CTFFamilyFileName;


            switch (BedType)
            {
                case BedType.None:
                    break;
                case BedType.Spectial:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.Intermediate:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.TerminalStart:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.TerminalEnd:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.DriveBed:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                default:
                    break;
            }
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(familyName, fileName, null, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, StartPoint);
        }
    }
}
