using System;
using System.Collections.Generic;
using VanillaStyleDiplomacy.Logging;
using VanillaStyleDiplomacy.Saving.SavableInfo;
using TaleWorlds.CampaignSystem;
using VanillaStyleDiplomacy.Config;
using VanillaStyleDiplomacy.Helpers;
using System.Reflection;
using TaleWorlds.CampaignSystem.Actions;
using VanillaStyleDiplomacy.Actions;

namespace VanillaStyleDiplomacy.Saving
{
    public class DiplomacyCampaignBehavior : CampaignBehaviorBase
    {
        private const string saveDataKey = "VanillaStyleDiplomacySaveKey";
        public List<AllianceInfo> allianceEvents = null;

        public override void RegisterEvents()
        {
            if (ConfigReader.Instance.TryGetConfigOption("EnableExecutionPatch", out object patchExecution) && (bool)patchExecution)
            {
                MethodInfo oldExecutionMI = typeof(KillCharacterAction).GetMethod("ApplyByExecution");
                MethodInfo newExecutionMI = typeof(KillCharacterActionPatch).GetMethod("ApplyByExecution");
                if (!DetourUtility.TryDetourFromTo(oldExecutionMI, newExecutionMI))
                    LoggingManager.Instance.LogMessage("Failed to apply execution patch! Please view log file for more information.");
            }
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
