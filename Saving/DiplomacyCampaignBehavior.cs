using System;
using System.Collections.Generic;
using VanillaStyleDiplomacy.Managers;
using VanillaStyleDiplomacy.Saving.SavableInfo;
using TaleWorlds.CampaignSystem;

namespace VanillaStyleDiplomacy.Saving
{
    public class DiplomacyCampaignBehavior : CampaignBehaviorBase
    {
        private const string saveDataKey = "VanillaStyleDiplomacySaveKey";
        public List<AllianceInfo> allianceEvents = null;

        public override void RegisterEvents()
        {
            
        }

        public override void SyncData(IDataStore dataStore)
        {
            try
            {
                dataStore.SyncData("allianceEvents", ref allianceEvents);
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogMessage("Failed to sync saved mod data for 'VanillaStyleDiplomacy'!");
                LoggingManager.Instance.LogException(e);
            }
            finally
            {
                if (allianceEvents == null)
                    allianceEvents = new List<AllianceInfo>();
            }
        }
    }
}
