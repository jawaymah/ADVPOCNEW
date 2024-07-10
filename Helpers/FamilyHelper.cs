using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Forms;

namespace AdvansysPOC.Helpers
{
    public static class FamilyHelper
    {
        public static Family FindFamilyByName(Document doc, string familyName)
        {
            FilteredElementCollector collector = new FilteredElementCollector(doc);
            collector.OfClass(typeof(Family));

            foreach (Family family in collector)
            {
                if (family.Name.Equals(familyName, StringComparison.OrdinalIgnoreCase))
                {
                    return family;
                }
            }

            return null;
        }
        public static void SetUnitId(this AssemblyInstance instance, string unitId)
        {
            ParametersHelper.SetUnitId(instance, unitId);
        }
        public static void SetUnitId(this FamilyInstance instance, string unitId)
        {
            ParametersHelper.SetUnitId(instance, unitId);
        }
        public static void SetUnitId(this FamilyInstance instance)
        {
            //TODO
            //ParametersHelper.SetParameter(instance, Constants.ConveyorNumber, "1001" /*ParametersHelper.GetLastUnitId()*/);
            ParametersHelper.SetLastUnitId(instance);
        }

        public static Line getFamilyCL(this FamilyInstance instance)
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

        public static FamilySymbol getFamilySymbol(string basicFamilyName, string basicFamilyNameWithExtension, FamilySymbol symbol, ref string message)
        {
            if (symbol == null)
            {
                Family family = FamilyHelper.FindFamilyByName(Globals.Doc, basicFamilyName);
                if (family == null)
                {
                    using (Transaction t = new Transaction(Globals.Doc, "Load Family Instance"))
                    {
                        t.Start();

                        // Absolute path to the family file
                        string familyPath = new Uri(Path.Combine(UIConstants.ButtonFamiliesFolder, basicFamilyNameWithExtension), UriKind.Absolute).AbsolutePath;

                        if (!Globals.Doc.LoadFamily(familyPath, out family))
                        {
                            message = "Could not load family.";
                            t.RollBack();
                        }

                        t.Commit();
                    }
                }
                // Assume the family has a family symbol (family type)
                FamilySymbol familySymbol = null;
                foreach (ElementId id in family.GetFamilySymbolIds())
                {
                    familySymbol = Globals.Doc.GetElement(id) as FamilySymbol;
                    break; // For simplicity, using the first available symbol
                }

                if (familySymbol == null)
                {
                    message = "No family symbols found in the family.";
                }

                symbol = familySymbol;
            }
            return symbol;
        }

        public static FamilySymbol getFamilySymbolwithoutTransaction(string basicFamilyName, string basicFamilyNameWithExtension, FamilySymbol symbol, int width, ref string message)
        {
            if (symbol == null)
            {
                Family family = FamilyHelper.FindFamilyByName(Globals.Doc, basicFamilyName);
                if (family == null)
                {
                    // Absolute path to the family file
                    string familyPath = new Uri(Path.Combine(UIConstants.ButtonFamiliesFolder, basicFamilyNameWithExtension), UriKind.Absolute).AbsolutePath;

                    if (!Globals.Doc.LoadFamily(familyPath, out family))
                    {
                        message = "Could not load family.";
                    }
                }
                // Assume the family has a family symbol (family type)
                FamilySymbol familySymbol = null;
                foreach (ElementId id in family.GetFamilySymbolIds())
                {
                    familySymbol = Globals.Doc.GetElement(id) as FamilySymbol;
                    if (familySymbol.Name.Contains(width.ToString()))
                    {
                        break;
                    }
                }

                if (familySymbol == null)
                {
                    message = "No family symbols found in the family.";
                }

                symbol = familySymbol;
            }
            return symbol;
        }


        public static FamilyInstance placePointFamilyWithSubTransaction(FamilySymbol symbol, XYZ location, double length)
        {
            if (symbol == null) return null;

            FamilyInstance instance = null;
            using (SubTransaction t = new SubTransaction(Globals.Doc))
            {
                t.Start();

                if (!symbol.IsActive)
                    symbol.Activate();

                // Create a new instance of the family symbol at the specified location
                instance = Globals.Doc.Create.NewFamilyInstance(location, symbol, StructuralType.NonStructural);

                // Setting the length of the placed bed...

                if (symbol.FamilyName.Contains("380"))
                {
                    Parameter zoneLengthParameter = instance.LookupParameter("CLR_ZONES");
                    if (zoneLengthParameter != null && zoneLengthParameter.StorageType == StorageType.Double)
                    {
                        zoneLengthParameter.Set(length-0.5);
                    }
                }
                else if (symbol.FamilyName.Contains("351") || symbol.FamilyName.Contains("352"))
                {
                    Parameter lengthParameter = instance.LookupParameter("Bed_Length");
                    if (lengthParameter != null && lengthParameter.StorageType == StorageType.Double)
                    {
                        lengthParameter.Set(length);
                    }
                }
                else if (symbol.FamilyName.Contains("C2000"))
                {
                    Parameter lengthParameter = instance.LookupParameter("GR_LENGTH");
                    if (lengthParameter != null && lengthParameter.StorageType == StorageType.Double)
                    {
                        lengthParameter.Set(length);
                    }
                }

                t.Commit();
            }
            return instance;
        }

        public static void RotateFamilyToDirection(this FamilyInstance fi, Document doc, XYZ newOrientation, XYZ origin, bool flip=false)
        {
            ElementId elId = fi.Id; 
            XYZ location = (fi.Location as LocationPoint).Point;
            XYZ currentOrientation = XYZ.BasisX;// fi.FacingOrientation;
            XYZ perpendicularRay = null;
            if (currentOrientation.IsAlmostEqualTo(newOrientation)) {
                if (flip)
                {
                    doc.Regenerate();
                    if (flip && fi.CanFlipFacing)
                        fi.flipFacing();
                }

                return; 
            }
            if (currentOrientation.IsAlmostEqualTo(newOrientation * -1))
            {
                perpendicularRay = XYZ.BasisZ;
            }
            else
            {
                perpendicularRay = currentOrientation.CrossProduct(newOrientation).Normalize();
            }
            Line line = Line.CreateBound(location, location + perpendicularRay);
            double angle = currentOrientation.AngleTo(newOrientation);

            try
            {
                ElementTransformUtils.RotateElement(doc, elId, line, angle);
                XYZ locationnew = (fi.Location as LocationPoint).Point;
                //(fi.Location as LocationPoint).Point = locationnew;
                ElementTransformUtils.MoveElement(doc, elId, origin.Subtract(locationnew));
                if(flip && fi.CanFlipHand)
                    fi.flipHand();
            }
            catch { //TaskDialog.Show("ERROR", "Failed trying to Rotate FamilyInstance");
            }

            return;
        }
    }
}
