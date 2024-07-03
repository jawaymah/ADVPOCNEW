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
        public DetailedBed() 
        {
            this.HasDrive = false;    
        }
		//public DetailedBed NextBed { get; set; }
        //public DetailedBed PrevBed { get; set; }
        public XYZ StartPoint { get; set; }
        //public XYZ EndPoint { get; set; }
        public double Length { get; set; }
        public BedType BedType { get; set; }
        public bool HasDrive { get; set; }

        public XYZ GetEndPoint()
        {
            //to be implemented later...
            return new XYZ();
            //return EndPoint.DistanceTo(StartPoint);
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
                case BedType.Brake:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.C351CTF:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.EntryBed:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.ExitBed:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.Drive:
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
