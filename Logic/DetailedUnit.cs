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

        public List<string> ExportToSym3()
        {
            List<string> sym3 = new List<string>();
            sym3.Add(ObjectName());
            sym3.Add(X());
            sym3.Add(Y());
            sym3.Add(Z());
            sym3.Add(direction());
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
            return EndPoint.AngleTo(StartPoint).ToString();
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
