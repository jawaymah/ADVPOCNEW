using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdvansysPOC.Helpers
{
    public static class ParametersHelper
    {

        public static void SetParameter(this FamilyInstance instance, string parameter, string value)
        {
            try
            {
                Parameter p = instance.LookupParameter(parameter);
                if (p != null)
                    p.Set(value);
            }
            catch (Exception ex)
            {

                
            }

        }

        public static void SetParameter(this FamilyInstance instance, string parameter, double value)
        {
            Parameter p = instance.LookupParameter(parameter);
            if (p != null)
                p.Set(value);
        }

        public static void SetParameter(this FamilyInstance instance, string parameter, int value)
        {
            Parameter p = instance.LookupParameter(parameter);
            if (p != null)
                p.Set(value);
        }

        public static bool isParameterEquals(this FamilyInstance instance, string parameter, string value)
        {
            Parameter p = instance.LookupParameter(parameter);
            if (p != null)
            {
                string currentValue = p.AsValueString();
                if (currentValue.ToLower() == value.ToLower())
                {
                    return true;
                }
            }
            return false;
        }

        public static void SetupProjectIfNeeded(Document doc)
        {
            ElementId projectId = doc.ProjectInformation.Id;
            Element projectInfoElement = doc.GetElement(projectId);
            Parameter param = projectInfoElement.LookupParameter(ParametersConstants.LastUnitId); // Refresh the parameter reference
            if (param != null && !param.IsReadOnly)
            {
                //TODO anything on existing project

            }
            else
            {
                setupProject(doc);
            }
        }

        public static void setupProject(Document doc)
        {

        }

        public static void CreateProjectParameter(Document doc, string name)
        {

        }

        //public static void SetProjectInformationParameterInt(Document doc, BuiltInParameter parameter, int value)
        //{
        //    // Get the Project Information element
        //    ElementId projectId = doc.ProjectInformation.Id;
        //    Element projectInfoElement = doc.GetElement(projectId);

        //    // Check if the parameter exists and is writable
        //    if (projectInfoElement.get_Parameter(parameter) != null && projectInfoElement.get_Parameter(parameter).IsReadOnly == false)
        //    {
        //        // Set the parameter value
        //        Parameter param = projectInfoElement.get_Parameter(parameter);
        //        param.Set(value);
        //    }
        //    else
        //    {
        //        TaskDialog.Show("Error", "Parameter " + parameter.ToString() + " not found or is read-only.");
        //    }
        //}

        public static Parameter GetProjectUnitIdParameter(Document doc)
        {
            ElementId projectId = doc.ProjectInformation.Id;
            Element projectInfoElement = doc.GetElement(projectId);
            Parameter param = projectInfoElement.LookupParameter(ParametersConstants.LastUnitId); // Refresh the parameter reference
            if (param != null && !param.IsReadOnly)
            {
                return param;
            }
            else
            {
                return null;
            }
        }

        public static int GetLastUnitId()
        {
            return GetProjectUnitIdParameter(Globals.Doc).AsInteger();
        }


        public static void SetLastUnitId(int id)
        {
            GetProjectUnitIdParameter(Globals.Doc).Set(id);
        }

        public static void SetLastUnitId()
        {
            var param = GetProjectUnitIdParameter(Globals.Doc);
            param.Set(param.AsInteger()+1);
        }
    }
}
