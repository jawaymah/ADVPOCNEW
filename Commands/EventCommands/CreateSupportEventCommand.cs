using AdvansysPOC.Helpers;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Commands.EventCommands
{
    public class CreateSupportEventCommand
    {
        public static Result Execute(UIApplication uiApp)
        {
            string error = "";
            FamilySymbol symbol = FamilyHelper.getFamilySymbol(Constants.SupportFamilyName, Constants.SupportFamilyFileName, null, ref error);
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
    public class CreateSupportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                UIApplication uiApp = commandData.Application;
                Document doc = commandData.Application.ActiveUIDocument.Document;

                CreateCutToFitEventCommand.Execute(uiApp);

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
