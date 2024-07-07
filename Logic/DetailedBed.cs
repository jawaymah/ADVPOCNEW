using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Office.Interop.Excel;


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
        public XYZ Direction { get; set; }
        //public XYZ EndPoint { get; set; }
        public double Length { get; set; }
        public BedType BedType { get; set; }
        public bool HasDrive { get; set; }

        public XYZ GetEndPoint()
        {
            return StartPoint + Direction * Length;
            //to be implemented later...
            //return EndPoint.DistanceTo(StartPoint);
        }

        public FamilyInstance PlaceBed(int width)
        {
            string error = "";
            string familyName = Constants.CTFFamilyName;
            string fileName = Constants.CTFFamilyFileName;


            switch (BedType)
            {
                case BedType.None:
                    break;
                case BedType.Brake:
                    familyName = Constants.BrakeBedFamilyName;
                    fileName = Constants.BrakeBedFileName;
                    break;
                case BedType.C351CTF:
                    familyName = Constants.CTFFamilyName;
                    fileName = Constants.CTFFamilyFileName;
                    break;
                case BedType.C352:
                    familyName = Constants.IntermediateFamilyName;
                    fileName = Constants.IntermediateFamilyFileName;
                    break;
                case BedType.EntryBed:
                    familyName = Constants.EntranceBedFamilyName;
                    fileName = Constants.EntranceBedFileName;
                    break;
                case BedType.ExitBed:
                    familyName = Constants.ExitBedFamilyName;
                    fileName = Constants.ExitBedFileName;
                    break;
                case BedType.Drive:
                    familyName = Constants.DriveFamilyName;
                    fileName = Constants.DriveFamilyFileName;
                    break;
                default:
                    break;
            }

            FamilySymbol symbol = FamilyHelper.getFamilySymbolwithoutTransaction(familyName, fileName, null, width, ref error);
            return FamilyHelper.placePointFamilyWithSubTransaction(symbol, StartPoint, Length);
        }
    }
}
