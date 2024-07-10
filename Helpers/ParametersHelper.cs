using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace AdvansysPOC.Helpers
{
    public static class ParametersHelper
    {

        public static void SetParameter(this Element instance, string parameter, string value)
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

        public static void SetTypeParameter(this FamilyInstance instance, string parameter, double value)
        {
            Parameter p = instance.Symbol.LookupParameter(parameter);
            if (p != null)
                p.Set(value);
        }
        public static void SetParameter(this FamilyInstance instance, string parameter, double value)
        {
            Parameter p = instance.LookupParameter(parameter);
            if (p != null)
                p.Set(value);
        }

        public static void SetParameter(this Element instance, string parameter, int value, bool integer=true)
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
            if (!doc.IsFamilyDocument)
            {
                ElementId projectId = doc.ProjectInformation.Id;
                Element projectInfoElement = doc.GetElement(projectId);
                Parameter param = projectInfoElement.LookupParameter(Constants.LastUnitId); // Refresh the parameter reference
                if (param != null && !param.IsReadOnly)
                {
                    //TODO anything on existing project

                }
                else
                {
                    setupProject(doc);
                }
            }
        }

        public static void setupProject(Document doc)
        {
            using (TransactionGroup tg = new TransactionGroup(doc))
            {
                tg.Start("setup project data");
                CreateProjectParameter(doc, Constants.LastUnitId);
                CreateAssemblyInstanceParameter(doc, Constants.ConveyorNumber);
                CreateAssemblyInstanceParameter(doc, "HP");
                CreateAssemblyInstanceParameter(doc, "Center_Drive", SpecTypeId.String.Text);
                tg.Commit();
            }

        }

        public static void CreateProjectParameter(Document doc, string name)
        {
            using (Transaction trans = new Transaction(doc))
            {
                // The name of the transaction was given as an argument
                if (trans.Start("Create project parameter") != TransactionStatus.Started) return;

                Category materials = doc.Settings.Categories.get_Item(BuiltInCategory.OST_ProjectInformation);
                CategorySet cats1 = doc.Application.Create.NewCategorySet();
                cats1.Insert(materials);

                // parameter type => text ParameterType.Text
                //BuiltInParameterGroup.PG_IDENTITY_DATA
                using (SubTransaction subTR = new SubTransaction(doc))
                {
                    subTR.Start();
                    RawCreateProjectParameter(doc.Application, name, SpecTypeId.Int.Integer, true,
    cats1, true);
                    subTR.Commit();
                }

                doc.Regenerate();
                SetLastUnitId(1001);
                trans.Commit();
            }
        }
        public static void CreateAssemblyInstanceParameter(Document doc, string name, ForgeTypeId forgeTypeId=null)
        {
            using (Transaction trans = new Transaction(doc))
            {
                // The name of the transaction was given as an argument
                if (trans.Start("Create AssemblyInstance parameter") != TransactionStatus.Started) return;

                Category materials = doc.Settings.Categories.get_Item(BuiltInCategory.OST_Assemblies);
                CategorySet cats1 = doc.Application.Create.NewCategorySet();
                cats1.Insert(materials);
                if (forgeTypeId == null) forgeTypeId = SpecTypeId.Int.Integer;
                // parameter type => text ParameterType.Text
                //BuiltInParameterGroup.PG_IDENTITY_DATA
                using (SubTransaction subTR = new SubTransaction(doc))
                {
                    subTR.Start();
                    RawCreateProjectParameter(doc.Application, name, forgeTypeId, true,
    cats1, true);
                    subTR.Commit();
                }

                doc.Regenerate();
                trans.Commit();
            }
        }
        public static void RawCreateProjectParameter(Autodesk.Revit.ApplicationServices.Application app, string name,
            ForgeTypeId type, bool visible, CategorySet cats, bool inst)
        {
            string oriFile = app.SharedParametersFilename;
            string tempFile = Path.GetTempFileName() + ".txt";
            using (File.Create(tempFile)) { }
            app.SharedParametersFilename = tempFile;

            var defOptions = new ExternalDefinitionCreationOptions(name, type)
            {
                Visible = visible
            };
            ExternalDefinition def = app.OpenSharedParameterFile().Groups.Create("TemporaryDefintionGroup").
                Definitions.Create(defOptions) as ExternalDefinition;

            app.SharedParametersFilename = oriFile;
            File.Delete(tempFile);

            Autodesk.Revit.DB.Binding binding = app.Create.NewTypeBinding(cats);
            if (inst) binding = app.Create.NewInstanceBinding(cats);

            BindingMap map = (new UIApplication(app)).ActiveUIDocument.Document.ParameterBindings;
            if (!map.Insert(def, binding, GroupTypeId.IdentityData))
            {
                Trace.WriteLine($"Failed to create Project parameter '{name}' :(");
            }
        }

        public static Parameter GetProjectUnitIdParameter(Document doc)
        {
            Parameter param = doc.ProjectInformation.LookupParameter(Constants.LastUnitId);
            if (param != null && !param.IsReadOnly)
            {
                return param;
            }
            else
            {
                return null;
            }
        }

        //public static int GetLastUnitId()
        //{
        //    return GetProjectUnitIdParameter(Globals.Doc).AsInteger() + 5;
        //}


        public static void SetLastUnitId(int id)
        {
            GetProjectUnitIdParameter(Globals.Doc)?.Set(id);
        }

        public static void SetLastUnitId()
        {
            var param = GetProjectUnitIdParameter(Globals.Doc);
            if (param != null)
                param.Set(param.AsInteger());
        }

        public static void SetLastUnitId(FamilyInstance instance)
        {
            var param = GetProjectUnitIdParameter(Globals.Doc);
            if (param != null)
            {
                param.Set(param.AsInteger());
                SetParameter(instance, Constants.ConveyorNumber, param.AsValueString());
                param.Set(param.AsInteger() + 5);
            }

        }

        public static void SetUnitId(FamilyInstance instance, string unitId)
        {
            SetParameter(instance, Constants.ConveyorNumber, unitId);
        }

        public static void SetUnitId(AssemblyInstance instance, string unitId)
        {
            SetParameter(instance, Constants.ConveyorNumber, unitId);
        }
        public static void SetUnitId(AssemblyInstance instance, int unitId)
        {
            SetParameter(instance, Constants.ConveyorNumber, unitId);
        }
    }
}
