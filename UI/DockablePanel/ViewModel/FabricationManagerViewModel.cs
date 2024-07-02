using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Autodesk.Revit.DB;
using AdvansysPOC.UI;

namespace AdvansysPOC
{
    /// <summary>
    /// ViewModel for View 'FabricationManagerView.xaml'.
    /// </summary>
    public partial class FabricationManagerViewModel : NotifyPropertyBase
    {
        //RefreshProperty(nameof(AutoTagFittingsSameNumber));
        private DetailedUnitViewModel currentUnit;

        public DetailedUnitViewModel CurrentUnit
        {
            get { return currentUnit; }
            set { currentUnit = value; RefreshAllProperties(); }
        }


    }
}
