using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;


namespace AdvansysPOC
{
    public partial class FabricationManagerView : Page, IDockablePaneProvider
    {
        /// <summary>
        /// Ctor
        /// </summary>
        public FabricationManagerView()
        {
            InitializeComponent();
            this.DataContext = new FabricationManagerViewModel();

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
    }
}
