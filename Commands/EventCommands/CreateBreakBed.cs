using AdvansysPOC.Helpers;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace AdvansysPOC.Commands.EventCommands
{
    public class CreateBrakeBedEventCommand
    {
        public static Result Execute(UIApplication uiApp)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.BrakeBedFamilyName, Constants.BrakeBedFileName, null, ref error);
            try
            {
                uiApp.ActiveUIDocument.PostRequestForElementTypePlacement(symbol);
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException ex)
            {
                return Result.Cancelled;
            }
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class CreateBrakeBedCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                Document doc = commandData.Application.ActiveUIDocument.Document;

                CreateBrakeBedEventCommand.Execute(uiApp);

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }

}
