using AdvansysPOC.Logic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.UI
{
    public class DetailedUnitViewModel : NotifyPropertyBase
    {
		private string unitId;

		public string UnitId
		{
			get { return unit.unitId; }
			set { unitId = value; RefreshProperty(nameof(UnitId)); }
		}

        private string type;

        public string Type
        {
            get { return unit.Type; }
            set { type = value; RefreshProperty(nameof(Type)); }
        }


        private ObservableCollection<DetailedBedViewModel> beds;

		public ObservableCollection<DetailedBedViewModel> Beds
        {
			get { return beds; }
			set 
			{ 
				beds = value;
                RefreshProperty(nameof(Beds));
            }
		}

        DetailedUnit unit;

        public DetailedUnitViewModel(DetailedUnit unit,ObservableCollection<DetailedBedViewModel> Beds)
		{
			this.Beds = Beds;
            this.unit = unit;
		}
        public DetailedUnitViewModel()
        {
            //test view and viewmodel
            this.unit = new DetailedUnit();
            this.unit.Type = "Live Roller";
            this.unit.unitId  = "1001";

            DetailedBedViewModel entry = new DetailedBedViewModel(new DetailedBed() { BedType = BedType.EntryBed});
            DetailedBedViewModel exit = new DetailedBedViewModel(new DetailedBed() { BedType = BedType.ExitBed});
            DetailedBedViewModel intermediate = new DetailedBedViewModel(new DetailedBed() { BedType = BedType.C352});
            DetailedBedViewModel CTF = new DetailedBedViewModel(new DetailedBed() { BedType = BedType.C351CTF});
            DetailedBedViewModel Drive = new DetailedBedViewModel(new DetailedBed() { BedType = BedType.Drive});
            this.Beds = new ObservableCollection<DetailedBedViewModel>() { entry ,exit, intermediate, CTF, Drive };
        }
        public void AddBed(DetailedBedViewModel bed)
		{
            Beds.Add(bed);
            RefreshProperty(nameof(Beds));
        }
    }
}
