using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using AdvansysPOC.Helpers;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Events;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AdvansysPOC
{
    [Transaction(TransactionMode.Manual)]
    public class GenericStraightConveyorCommand : IExternalCommand
    {
        const string basicFamilyName = "Straight";
        const string basicFamilyNameWithExtension = "Straight.rfa";
        const string basicFamilySymbolName = "straight";
        public static FamilySymbol symbol = null;
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                Document doc = commandData.Application.ActiveUIDocument.Document;

                uiApp.DialogBoxShowing += UiApp_DialogBoxShowing;

                if (symbol == null)
                {
                    Family family = FamilyHelper.FindFamilyByName(doc, basicFamilyName);
                    if (family == null)
                    {
                        using (Transaction t = new Transaction(doc, "Load Family Instance"))
                        {
                            t.Start();

                            // Absolute path to the family file
                            string familyPath = new Uri(Path.Combine(UIConstants.ButtonFamiliesFolder, basicFamilyNameWithExtension), UriKind.Absolute).AbsolutePath;

                            if (!doc.LoadFamily(familyPath, out family))
                            {
                                message = "Could not load family.";
                                t.RollBack();
                                return Result.Failed;
                            }

                            t.Commit();
                        }
                    }
                    uiApp.DialogBoxShowing -= UiApp_DialogBoxShowing;

                    // Assume the family has a family symbol (family type)
                    FamilySymbol familySymbol = null;
                    foreach (ElementId id in family.GetFamilySymbolIds())
                    {
                        familySymbol = doc.GetElement(id) as FamilySymbol;
                        break; // For simplicity, using the first available symbol
                    }

                    if (familySymbol == null)
                    {
                        message = "No family symbols found in the family.";
                        return Result.Failed;
                    }

                    symbol = familySymbol;
                }

                //Register document changed to get placed families and treat them as a unit
                try
                {
                    //PromptForFamilyInstancePlacementOptions options = new PromptForFamilyInstancePlacementOptions();
                    //options.FaceBasedPlacementType = FaceBasedPlacementType.PlaceOnWorkPlane;
                    //uiApp.ActiveUIDocument.PromptForFamilyInstancePlacement(symbol);

                    uiApp.ActiveUIDocument.PostRequestForElementTypePlacement(symbol);
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
                {

                }

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void UiApp_DialogBoxShowing(object sender, Autodesk.Revit.UI.Events.DialogBoxShowingEventArgs e)
        {

        }

        private void Application_DocumentChanged(object sender, DocumentChangedEventArgs e)
        {

        }
    }

}
