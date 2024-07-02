using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using AdvansysPOC.Events;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;


namespace AdvansysPOC
{
    public partial class FabricationManagerView : Page, IDockablePaneProvider
    {
        // fields
        public ExternalCommandData eData = null;
        public Document doc = null;
        public UIDocument uidoc = null;

        private ExternalEvent m_ExEvent;
        private DockablePanelEvent m_Handler;

        /// <summary>
        /// Ctor
        /// </summary>
        public FabricationManagerView()
        {
            InitializeComponent();
            this.DataContext = new FabricationManagerViewModel();

        }

        /// <summary>
        /// Ctor
        /// </summary>
        public FabricationManagerView(DockablePanelEvent handler, ExternalEvent e)
        {
            InitializeComponent();
            this.DataContext = new FabricationManagerViewModel();
            m_ExEvent = e;
            m_Handler = handler;
        }

        /// <summary>
        /// IDockablePaneProvider Implementation.
        /// </summary>
        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;

            data.InitialState = new DockablePaneState();
            data.InitialState.DockPosition = DockPosition.Right;
            data.InitialState.SetFloatingRectangle(new Autodesk.Revit.DB.Rectangle(200, 200, 800, 500));
            data.InitialState.MinimumWidth = 355;
        }

        // custom initiator
        public void CustomInitiator(ExternalCommandData e)
        {
            // ExternalCommandData and Doc
            eData = e;
            doc = e.Application.ActiveUIDocument.Document;
            uidoc = eData.Application.ActiveUIDocument;

            //// get the current document name
            //docName.Text = doc.PathName.ToString().Split('\\').Last();
            //// get the active view name
            //viewName.Text = doc.ActiveView.Name;
            //// call the treeview display method
            //DisplayTreeViewItem();
        }

        public void RemoveHandlers(FormClosedEventArgs e)
        {
            // we own both the event and the handler
            // we should dispose it before we are closed
            m_ExEvent.Dispose();
            m_ExEvent = null;
            m_Handler = null;
        }

        private void showMessageButton_Click(object sender, EventArgs e)
        {
            m_ExEvent.Raise();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_ExEvent.Raise();
        }

        public void UpdateSelection(List<ElementId> ids)
        {

        }
    }
}
