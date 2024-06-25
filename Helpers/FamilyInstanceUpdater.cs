using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdvansysPOC.Helpers
{
    public class FamilyInstanceUpdater : IUpdater
    {
        static AddInId m_appId;
        static UpdaterId m_updaterId;
        FamilySymbol familySymbol = null;

        // constructor takes the AddInId for the add-in associated with this updater
        public FamilyInstanceUpdater(AddInId id)
        {
            m_appId = id;
            m_updaterId = new UpdaterId(m_appId, new Guid("FBFBF6B2-4C06-42d4-97C1-D1B4EB593EFF"));
        }

        public void Execute(UpdaterData data)
        {
            Document doc = data.GetDocument();

            foreach (ElementId addedElemId in data.GetAddedElementIds())
            {
                FamilyInstance family = doc.GetElement(addedElemId) as FamilyInstance;
                if (family != null)
                {
                    if (family.Symbol.FamilyName == "Straight")
                    {
                        family.SetUnitId();
                    }
                }
            }
        }

        public string GetAdditionalInformation()
        {
            return "Wall type updater example: updates all newly created walls to a special wall";
        }

        public ChangePriority GetChangePriority()
        {
            return ChangePriority.FloorsRoofsStructuralWalls;
        }

        public UpdaterId GetUpdaterId()
        {
            return m_updaterId;
        }

        public string GetUpdaterName()
        {
            return "Wall Type Updater";
        }
    }

}
