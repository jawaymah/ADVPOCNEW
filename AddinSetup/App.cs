#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AdvansysPOC.Helpers;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Windows;
#endregion

namespace AdvansysPOC
{
    class App : IExternalApplication
    {

        protected string ManagerPanelName => "Advnasys Manager";
        private static DockablePaneId FabricationManagerPaneId { get; } = new DockablePaneId(new Guid("DB7FB22A-A5E5-4344-8009-048CCFEE679A"));

        public Result OnStartup(UIControlledApplication a)
        {
            string tabName = "Advansys";
            try
            {
                // Create a custom tab
                a.CreateRibbonTab(tabName);

                // Add panels to the custom tab
                AddRibbonPanel(a, tabName, "GenericConveyors");

                AddRibbonPanel(a, tabName, "Manager");
                a.SelectionChanged += Elements_SelectionChanged;
                CurrentApplication = a;
                FabricationManagerView FabricationManagerView = new FabricationManagerView();
                a.RegisterDockablePane(FabricationManagerPaneId, ManagerPanelName, FabricationManagerView);

                //return Result.Succeeded;
            }
            catch (Exception ex)
            {
                Autodesk.Revit.UI.TaskDialog.Show("Error", ex.Message);
                return Result.Failed;
            }
            
            a.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;


            // Register familyInstance updater with Revit
            FamilyInstanceUpdater updater = new FamilyInstanceUpdater(a.ActiveAddInId);
            UpdaterRegistry.RegisterUpdater(updater);

            // Change Scope = any familyInstance element
            ElementClassFilter familyInstanceFilter = new ElementClassFilter(typeof(FamilyInstance));

            // Change type = element addition
            UpdaterRegistry.AddTrigger(updater.GetUpdaterId(), familyInstanceFilter,
                                        Element.GetChangeTypeElementAddition());

            //a.ControlledApplication.DocumentChanged += ControlledApplication_DocumentChanged;
            //a.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
            //a.ControlledApplication.DocumentCreated += ControlledApplication_DocumentCreated;
            //a.ControlledApplication.DocumentOpened += ControlledApplication_DocumentOpened;
            a.ViewActivated += A_ViewActivated;

            return Result.Succeeded;
        }

        private void ControlledApplication_DocumentChanged(object sender, Autodesk.Revit.DB.Events.DocumentChangedEventArgs e)
        {
            //var ids = e.GetAddedElementIds();
            //foreach (var id in ids)
            //{
            //    Element elem = e.GetDocument().GetElement(id);
            //    if (elem != null && elem is FamilyInstance)
            //    {
            //        FamilyInstance familyInstance = (FamilyInstance)elem;
            //        if (familyInstance.Symbol.FamilyName == "Straight")
            //        {
            //            Globals.addedFamilies.Add(familyInstance);
            //            familyInstance.SetUnitId();
            //        }
            //    }
            //}
        }

        private void A_ViewActivated(object sender, Autodesk.Revit.UI.Events.ViewActivatedEventArgs e)
        {
            if (Globals.Doc != e.Document)
            {
                Globals.SetupCurrentDocument(e.Document);
            }
        }

        private void Elements_SelectionChanged(object sender, Autodesk.Revit.UI.Events.SelectionChangedEventArgs e)
        {
            var selected = e.GetSelectedElements();
            //foreach (var id in selected)
            //{
            //    Element elem = e.GetDocument().GetElement(id);
            //    if (elem != null && elem is FamilyInstance)
            //    {
            //        FamilyInstance familyInstance = (FamilyInstance)elem;
            //        if (familyInstance.Symbol.FamilyName == "Straight")
            //            familyInstance.SetUnitId();
            //    }
            //}
            var doc = e.GetDocument();
        }


        private static RibbonTab ModifyTab;


        /// <summary>
        /// Public method for get Fabrication Manager Pane.
        /// </summary>
        public static DockablePane GetFabricationManagerPane()
        {
            try
            {
                return CurrentApplication.GetDockablePane(FabricationManagerPaneId);
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Current Application.
        /// </summary>
        private static UIControlledApplication CurrentApplication { get; set; }



        public Result OnShutdown(UIControlledApplication a)
        {
            a.ViewActivated -= A_ViewActivated;
            a.ControlledApplication.DocumentChanged -= ControlledApplication_DocumentChanged;

            FamilyInstanceUpdater updater = new FamilyInstanceUpdater(a.ActiveAddInId);
            UpdaterRegistry.UnregisterUpdater(updater.GetUpdaterId());

            return Result.Succeeded;
        }

        private void AddRibbonPanel(UIControlledApplication application, string tabName, string panelName)
        {
            Autodesk.Revit.UI.RibbonPanel panel = application.CreateRibbonPanel(tabName, panelName);

            if (panelName == "GenericConveyors")
            {

                PushButtonData buttonDataDimensions = new PushButtonData("Generic", "Generic", Assembly.GetExecutingAssembly().Location, "AdvansysPOC.GenericStraightConveyorCommand");
                //                buttonDataDimensions.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //"dimension32.png"), UriKind.Absolute));
                //                buttonDataDimensions.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //                    "dimension32.png"), UriKind.Absolute));
                //buttonDataDimensions.LargeImage = new BitmapImage(new Uri(Path.Combine(@"C:\Users\Jemmy\source\repos\AdvansysPOC\AdvansysPOC\Resources", "add32.png"), UriKind.Absolute));
                Autodesk.Revit.UI.RibbonItem PulldownButtons3 = panel.AddItem(buttonDataDimensions);

                PushButtonData buttonFlip = new PushButtonData("FlipGenericSide", "Flip Generic Hand", Assembly.GetExecutingAssembly().Location, "AdvansysPOC.FlipGenericHandCommand");
                //                buttonDataDimensions.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //"dimension32.png"), UriKind.Absolute));
                //                buttonDataDimensions.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //                    "dimension32.png"), UriKind.Absolute));
                //buttonDataDimensions.LargeImage = new BitmapImage(new Uri(Path.Combine(@"C:\Users\Jemmy\source\repos\AdvansysPOC\AdvansysPOC\Resources", "add32.png"), UriKind.Absolute));
                Autodesk.Revit.UI.RibbonItem flipButton = panel.AddItem(buttonFlip);
            }
            if (panelName == "Supports")
            {
                PulldownButtonData pullButtonDataRegular = new PulldownButtonData("Regular", "Regular");
                //pullButtonDataRegular.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "support16.png"), UriKind.Absolute));
                //pullButtonDataRegular.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "support16.png"), UriKind.Absolute));



                PulldownButtonData pullButtonBracingData = new PulldownButtonData("Bracing", "Bracing");
                //pullButtonBracingData.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "supportbracing16.png"), UriKind.Absolute));
                //pullButtonBracingData.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "supportbracing16.png"), UriKind.Absolute));

                PulldownButtonData pullButtonSpecialData = new PulldownButtonData("Special", "Special");
                //pullButtonSpecialData.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "supportspecial16.png"), UriKind.Absolute));
                //pullButtonSpecialData.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //    "supportspecial16.png"), UriKind.Absolute));

                IList<Autodesk.Revit.UI.RibbonItem> stackedPulldownButtons = panel.AddStackedItems(pullButtonDataRegular, pullButtonBracingData, pullButtonSpecialData);

            }
            if (panelName == "Manager")
            {
                PushButtonData AddbuttonData = new PushButtonData("Manager", "Manager", Assembly.GetExecutingAssembly().Location, "AdvansysPOC.FabricationManagerDisplayCommand");
                //                AddbuttonData.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //"add32.png"), UriKind.Absolute));
                //                AddbuttonData.Image = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder,
                //                    "add32.png"), UriKind.Absolute));
                Autodesk.Revit.UI.RibbonItem PulldownButtons3 = panel.AddItem(AddbuttonData);
            }
            // Add other panels as necessary
        }

        private void AddConveyorItem(PulldownButton pdButton, string name, string imagePath, string className)
        {
            PushButtonData buttonData = new PushButtonData(name, name, Assembly.GetExecutingAssembly().Location, className);

            //buttonData.LargeImage = new BitmapImage(new Uri(imagePath, UriKind.Absolute));
            //buttonData.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder, imagePath), UriKind.Absolute));

            buttonData.LargeImage = new BitmapImage(new Uri(Path.Combine(UIConstants.ButtonIconsFolder, imagePath), UriKind.Absolute));

            pdButton.AddPushButton(buttonData);
        }


    }
    class DocumentAvailablility : IExternalCommandAvailability
    {
        public bool IsCommandAvailable(UIApplication applicationData, CategorySet selectedCategories)
        {
            if (applicationData.ActiveUIDocument != null && applicationData.ActiveUIDocument.Document != null)
                return true;
            return false;
        }
    }
}
