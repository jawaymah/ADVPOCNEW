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
        public DetailedUnit PrevUnit { get; set; }
        public DetailedUnit NextUnit { get; set; }
        public string unitId {  get; set; }
        public string Length {  get; set; }
        public string Width {  get; set; }
        public string Type {  get; set; }
        public string Speed {  get; set; }
        
        public DetailedUnit()
        {
            Beds = new List<DetailedBed>();
        }
        public DetailedUnit(AssemblyInstance instance)
        {
            Speed = "160";
            StartPoint = XYZ.Zero;
            EndPoint = XYZ.Zero;
            unitId = "CLR" + instance.LookupParameter(Constants.ConveyorNumber)?.AsValueString();
            foreach (var itemId in instance.GetMemberIds())
            {
                FamilyInstance inst = Globals.Doc.GetElement(itemId) as FamilyInstance;
                string name = inst.Symbol.FamilyName;
                if (name == Constants.EntranceBedFamilyName)
                {
                    StartPoint = (inst.Location as LocationPoint).Point;
                }
                if (name == Constants.ExitBedFamilyName)
                {
                    Parameter p = inst.LookupParameter(Constants.Bed_Length);
                    if (p != null)
                    {
                        EndPoint = (inst.Location as LocationPoint).Point + inst.HandOrientation * p.AsDouble();
                    }
                    p = inst.LookupParameter(Constants.Bed_Width);
                    if (p != null)
                    {
                        Width = p.AsValueString();
                    }
                }
                if (name == Constants.DriveFamilyName)
                {
                    Parameter p = inst.LookupParameter(Constants.Drive_Speed);
                    if (p != null)
                    {
                        Speed = p.AsValueString();
                    }
                }
                //Parameter p = inst.LookupParameter(Constants.Conveyor_Envelop);
                //if (p != null)
                //{
                //    if (p.AsInteger() == 0)
                //        p.Set(1);
                //    else
                //        p.Set(0);
                //}
            }
            Length = EndPoint.DistanceTo(StartPoint).ToString();
            Type = "CLR";
        }

        public List<string> ExportToSym3()
        {
            //"OBJECTNAME", "X", "Y", "Z", "Length", "Width", "DIRECTION", "TYPE", "Speed", "CONNECTION", "AUX"
            List<string> sym3 = new List<string>();
            sym3.Add(ObjectName());
            sym3.Add(X());
            sym3.Add(Y());
            sym3.Add(Z());
            sym3.Add(Length);
            sym3.Add(Width);
            sym3.Add(direction());
            sym3.Add(Type);
            sym3.Add(Speed);
            sym3.Add(Connection());
            sym3.Add(AUX());
            return sym3;
        }

        public string ObjectName()
        {
            return unitId;
        }

        public string X()
        {
            return StartPoint.X.ToString();
        }

        public string Y()
        {
            return StartPoint.Y.ToString();
        }

        public string Z()
        {
            return StartPoint.Z.ToString();
        }

        public string direction()
        {
            XYZ dir = EndPoint.Subtract(StartPoint).Normalize();
            return Math.Round(EndPoint.Subtract(StartPoint).Normalize().AngleTo(XYZ.BasisX) *180/Math.PI).ToString();
        }

        public string Connection()
        {
            return NextUnit?.unitId;
        }

        public string AUX()
        {
            return "";
        }
    }
}
