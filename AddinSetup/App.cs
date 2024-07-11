#region Namespaces
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using AdvansysPOC.Commands;
using AdvansysPOC.Commands.EventCommands;
using AdvansysPOC.Events;
using AdvansysPOC.Helpers;
using AdvansysPOC.PROPERTIES;
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

        protected string ManagerPanelName => "Daifuku Manager";
        private static DockablePaneId FabricationManagerPaneId { get; } = new DockablePaneId(new Guid("DB7FB22A-A5E5-4344-8009-048CCFEE679A"));
        FabricationManagerView FabricationManagerView;
        public Result OnStartup(UIControlledApplication a)
        {
            string tabName = "Daifuku";
            try
            {
                // Create a custom tab
                a.CreateRibbonTab(tabName);

                // Add panels to the custom tab
                AddRibbonPanel(a, tabName, "GenericConveyors");
                AddRibbonPanel(a, tabName, "DetailedConveyors");
                AddRibbonPanel(a, tabName, "Controls");
                AddRibbonPanel(a, tabName, "Manager");
                a.SelectionChanged += Elements_SelectionChanged;
                CurrentApplication = a;

                // A new handler to handle request posting by the dialog
                DockablePanelEvent handler = new DockablePanelEvent();
                // External Event for the dialog to use (to post requests)
                ExternalEvent exEvent = ExternalEvent.Create(handler);


                FabricationManagerView = new FabricationManagerView(handler, exEvent);
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
            var selected = e.GetSelectedElements().ToList();
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
            FabricationManagerView.UpdateSelection(selected);
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
                var buttonDataDimensions = RevitUi.AddPushButtonData("Create\n Generic Unit", typeof(GenericStraightConveyorCommand), Resources.add32, typeof(DocumentAvailablility));
                var buttonFlip = RevitUi.AddPushButtonData("Flip\n Generic Hand", typeof(FlipGenericHandCommand), Resources.element_move32, typeof(DocumentAvailablility));
                var buttonConvert = RevitUi.AddPushButtonData("Convert To Detail", typeof(ConvertToDetailCommand), Resources.convertToDetail, typeof(DocumentAvailablility));

                Autodesk.Revit.UI.RibbonItem PulldownButtons3 = panel.AddItem(buttonDataDimensions);
                Autodesk.Revit.UI.RibbonItem flipButton = panel.AddItem(buttonFlip);
                Autodesk.Revit.UI.RibbonItem convertButton = panel.AddItem(buttonConvert);
            }
            if (panelName == "DetailedConveyors")
            {
                var hpCalculationsButton = RevitUi.AddPushButtonData("HP Calculations", typeof(HPCalculationsCommand), Resources.excelIcon32, typeof(DocumentAvailablility));
                Autodesk.Revit.UI.RibbonItem PulldownButtons3 = panel.AddItem(hpCalculationsButton);

                var beltCalculationsButton = RevitUi.AddPushButtonData("Belt Calculations", typeof(BeltCalculationsCommand), Resources.excelIcon32, typeof(DocumentAvailablility));
                Autodesk.Revit.UI.RibbonItem PulldownButtons4 = panel.AddItem(beltCalculationsButton);

                //IList<Autodesk.Revit.UI.RibbonItem> stackedPulldownButtons = panel.AddStackedItems(pullButtonDataRegular, pullButtonBracingData, pullButtonSpecialData);
                var pullButtonDetailed = RevitUi.AddPullDownButtonData("DetailedConveyors", "Detailed Conveyors (CLR)");
                Autodesk.Revit.UI.PulldownButton PulldownButtons = panel.AddItem(pullButtonDetailed) as PulldownButton;

                //creat Detailed beds commands
                var enterenceData = RevitUi.AddPushButtonData("C380_ENTRY", typeof(CreateEnterenceBedCommand), Resources.add32, typeof(DocumentAvailablility));
                var ExitData = RevitUi.AddPushButtonData("C380_EXIT", typeof(CreateExitBedCommand), Resources.add32, typeof(DocumentAvailablility));
                var IntermediateData = RevitUi.AddPushButtonData("C352", typeof(CreateIntermediateBedCommand), Resources.add32, typeof(DocumentAvailablility));
                var CTFData = RevitUi.AddPushButtonData("C351", typeof(CreateCutToFitCommand), Resources.add32, typeof(DocumentAvailablility));
                var DriveData = RevitUi.AddPushButtonData("C370", typeof(CreateDriveCommand), Resources.add32, typeof(DocumentAvailablility));
                var SupportData = RevitUi.AddPushButtonData("C2101", typeof(CreateSupportCommand), Resources.add32, typeof(DocumentAvailablility));
                var GuideRailData = RevitUi.AddPushButtonData("C2000", typeof(CreateGuideRailCommand), Resources.add32, typeof(DocumentAvailablility));
                var BrakeBedData = RevitUi.AddPushButtonData("C353", typeof(CreateBrakeBedCommand), Resources.add32, typeof(DocumentAvailablility));

                PulldownButtons.AddPushButton(enterenceData);
                PulldownButtons.AddPushButton(BrakeBedData);
                PulldownButtons.AddPushButton(ExitData);
                PulldownButtons.AddPushButton(IntermediateData);
                PulldownButtons.AddPushButton(CTFData);
                PulldownButtons.AddSeparator();
                PulldownButtons.AddPushButton(DriveData);
                PulldownButtons.AddPushButton(SupportData);
                PulldownButtons.AddPushButton(GuideRailData);

                var sym3Convert = RevitUi.AddPushButtonData("Export To Sym3", typeof(Sym3ExportCommand), Resources.excelIcon32, typeof(DocumentAvailablility));
                Autodesk.Revit.UI.RibbonItem sym3Button = panel.AddItem(sym3Convert);

                var envelop = RevitUi.AddPushButtonData("Envelop \nShow/Hide", typeof(EnvelopShowHideCommand), Resources.filterIcoFilled, typeof(DocumentAvailablility));
                Autodesk.Revit.UI.RibbonItem envelopButton = panel.AddItem(envelop);
            }
            if (panelName == "Controls")
            {
                var pullButtonDetailed = RevitUi.AddPullDownButtonData("controls", "Controls");
                Autodesk.Revit.UI.PulldownButton PulldownButtons = panel.AddItem(pullButtonDetailed) as PulldownButton;

                //creat Detailed beds commands
                var DISCControlCOmmand = RevitUi.AddPushButtonData("DISC", typeof(DISCControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var EPCControlCOmmand = RevitUi.AddPushButtonData("EPC", typeof(EPCControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var IOControlCOmmand = RevitUi.AddPushButtonData("IO", typeof(IOControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var MOTORControlCOmmand = RevitUi.AddPushButtonData("MOTOR", typeof(MOTORControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var PEMControlCOmmand = RevitUi.AddPushButtonData("PEM", typeof(PEMControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var SIOControlCOmmand = RevitUi.AddPushButtonData("SIO", typeof(SIOControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var SOLControlCOmmand = RevitUi.AddPushButtonData("SOL", typeof(SOLControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var VFDControlCOmmand = RevitUi.AddPushButtonData("VFD", typeof(VFDControlCOmmand), Resources.add32, typeof(DocumentAvailablility));
                var ZIMControlCOmmand = RevitUi.AddPushButtonData("ZIM", typeof(ZIMControlCOmmand), Resources.add32, typeof(DocumentAvailablility));

                PulldownButtons.AddPushButton(DISCControlCOmmand);
                PulldownButtons.AddPushButton(EPCControlCOmmand);
                PulldownButtons.AddPushButton(IOControlCOmmand);
                PulldownButtons.AddPushButton(MOTORControlCOmmand);
                PulldownButtons.AddPushButton(PEMControlCOmmand);
                PulldownButtons.AddPushButton(SIOControlCOmmand);
                PulldownButtons.AddPushButton(SOLControlCOmmand);
                PulldownButtons.AddPushButton(VFDControlCOmmand);
                PulldownButtons.AddPushButton(ZIMControlCOmmand);

            }
            if (panelName == "Supports")
            {
                PulldownButtonData pullButtonDataRegular = new PulldownButtonData("Regular", "Regular");

                PulldownButtonData pullButtonBracingData = new PulldownButtonData("Bracing", "Bracing");

                PulldownButtonData pullButtonSpecialData = new PulldownButtonData("Special", "Special");

                IList<Autodesk.Revit.UI.RibbonItem> stackedPulldownButtons = panel.AddStackedItems(pullButtonDataRegular, pullButtonBracingData, pullButtonSpecialData);

            }
            if (panelName == "Manager")
            {
                var AddbuttonData = RevitUi.AddPushButtonData("Manager \n Show/Hide", typeof(FabricationManagerDisplayCommand), Resources.editViewports32, typeof(DocumentAvailablility));
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
