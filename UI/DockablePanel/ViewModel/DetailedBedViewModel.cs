using AdvansysPOC.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.UI
{
    public class DetailedBedViewModel : NotifyPropertyBase
    {

        private string type;

        public string Type
        {
            get { return bed.BedType.ToString(); }
            set { type = value; RefreshProperty(nameof(Type)); }
        }

        DetailedBed bed;

        public DetailedBedViewModel(DetailedBed bed)
        {
            this.bed = bed;
        }
    }
}
