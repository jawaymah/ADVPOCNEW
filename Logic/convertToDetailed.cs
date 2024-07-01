using AdvansysPOC.Helpers;
using Autodesk.Revit.DB;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.UI;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.IO.Packaging;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using Autodesk.Revit.DB.Structure;
using System.Windows.Forms;

namespace AdvansysPOC.Logic
{
    internal class convertToDetailed
    {

        const string entryBedFamilyName = "C380_ENTRY";
        const string exitBedFamilyName = "C380_EXIT";
        const string inter351BedFamilyName = "C351";
        const string inter352BedFamilyName = "C352";
        const string preExitBedFamilyName = "C353";



        public Line getGenericCL(FamilyInstance instance)
        {
            Options options = new Options();

            GeometryElement geomElem = instance.get_Geometry(options);

            // Loop through geometry elements to find the center line
            foreach (GeometryObject geomObj in geomElem)
            {
                if (geomObj is GeometryInstance geomInst)
                {
                    GeometryElement instGeom = geomInst.GetInstanceGeometry();

                    // Check if the instance geometry contains a curve (e.g., center line)
                    foreach (GeometryObject obj in instGeom)
                    {
                        if (obj is Curve curve)
                        {
                            // Check if it's a Line (assuming center line is a straight line)
                            if (curve is Line line)
                            {
                                return line;

                            }
                        }
                    }
                }
            }
            return null;
        }


        private FamilySymbol FindFamilySymbol(Document doc, Family family, string symbolName)
        {
            // Get all symbols (types) of the loaded family
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            IList<Element> symbols = collector.OfClass(typeof(FamilySymbol)).OfCategory(BuiltInCategory.OST_GenericModel).ToList();

            // Find the symbol with the specified name
            foreach (Element element in symbols)
            {
                FamilySymbol symbol = element as FamilySymbol;
                if (symbol != null && symbol.Family.Id == family.Id && symbol.Name == symbolName)
                {
                    return symbol;
                }
            }
            return null;
        }


        public XYZ PlaceBed(Document doc, FamilySymbol symbol, XYZ pos, double length)
        {
            if (symbol != null)
            {

                // Create a line representing the location and direction of the linear element
                // First assuming horizontal conveyor...
                XYZ endPoint = new XYZ(pos.X + length, pos.Y, pos.Z); // End point based on length
                Line line = Line.CreateBound(pos, endPoint);


                // Place the family instance
                using (Transaction tx = new Transaction(doc))
                {
                    tx.Start("Place Linear Family Instance");

                    // Place the family instance along the line
                    FamilyInstance instance = doc.Create.NewFamilyInstance(line, symbol, doc.ActiveView);

                    tx.Commit();
                    return endPoint;
                }

            }
            return pos;
        }


            public void ConvertToDetailed(FamilyInstance instance)
        {
            Document doc = instance.Document;

            // Get parameter values
            Parameter lengthParam = instance.LookupParameter("Conveyor_OAL");
            Parameter widthParam = instance.LookupParameter("Conveyor_OAW");
            Parameter zoneLengthParam = instance.LookupParameter("Zone_Length");
            Parameter typeParam = instance.LookupParameter("Conveyor_Type");

            if (lengthParam != null && typeParam != null)
            {
                // Read all needed parameters values
                double conveyorLength = lengthParam.AsDouble();
                int conveyorWidth = widthParam.AsInteger();
                double zoneLength = zoneLengthParam.AsDouble();
                string conveyorType = typeParam.AsValueString();

                // Get geometry information
                XYZ startPoint, endPoint;
                Line cl = getGenericCL(instance);
                startPoint = cl.GetEndPoint(0);
                endPoint = cl.GetEndPoint(1);

                //Gathering Families...
                Family family = null; // I need to get the family using its name or path and load it to the project...


                //Getting bed types to be placed...
                FamilySymbol entryBed, exitBed, ctfBed, withBrakeBed, repetitiveBed;

                entryBed = FindFamilySymbol(doc, family, "C380-ENTRY-" + conveyorWidth.ToString() + "\"");
                exitBed = FindFamilySymbol(doc, family, "C380-EXIT-" + conveyorWidth.ToString() + "\"");
                //entryBed = FindFamilySymbol(doc, family, "C380-ENTRY-" + conveyorWidth.ToString() + "\"");
                //entryBed = FindFamilySymbol(doc, family, "C380-ENTRY-" + conveyorWidth.ToString() + "\"");

                
                // We need to calculate how much instances we need for each symbol, and how long is each of them...
                // count may be zero for some symbols, constant one for entry and exit and ctf, varies for intermediates...

                //Placing beds in order...
                XYZ entryEndPoint = PlaceBed(doc, entryBed, startPoint, zoneLength == 24 ? 2.5 : 3.5);
                //XYZ 

                //Grouping beds into an assembly...


                //Pushing required props to the assembly...



            }
        }
    }
}
