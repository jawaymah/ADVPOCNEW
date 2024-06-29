using Autodesk.Revit.DB;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Logic
{
    internal class convertToDetailed
    {

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


        public FamilySymbol getProperSymbol(string bedName, double bedWidth)
        {
            FamilySymbol familySymbol = null;

            //Do logic here to get the proper type based on the bed name and width and maybe other stuff...

            return familySymbol;
        }


        public void ConvertToDetailed(FamilyInstance instance)
        {
            Document doc = instance.Document;

            // Get parameter values
            Parameter lengthParam = instance.LookupParameter("Conveyor Length");
            Parameter widthParam = instance.LookupParameter("Conveyor Width");
            Parameter zoneLengthParam = instance.LookupParameter("Zone Length");
            Parameter typeParam = instance.LookupParameter("Conveyor Type");

            if (lengthParam != null && typeParam != null)
            {
                // Read all needed parameters values
                double conveyorLength = lengthParam.AsDouble();
                double conveyorWidth = widthParam.AsDouble();
                double zoneLength = zoneLengthParam.AsDouble();
                string conveyorType = typeParam.AsValueString();

                // Get geometry information
                XYZ startPoint, endPoint;
                Line cl = getGenericCL(instance);
                startPoint = cl.GetEndPoint(0);
                endPoint = cl.GetEndPoint(1);

                //Getting bed types to be placed...
                FamilySymbol entryBed, exitBed, ctfBed, withBrakeBed, repetitiveBed;


                //Placing beds in order...


                //Grouping beds into an assembly...


                //Pushing required props to the assembly...



            }
        }
    }
}
