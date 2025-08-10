using VanillaStyleDiplomacy.Managers;
using TaleWorlds.MountAndBlade;
using VanillaStyleDiplomacy.Config;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Core;
using VanillaStyleDiplomacy.Saving;
using VanillaStyleDiplomacy.Helpers;
using TaleWorlds.Library;
using System.Threading.Tasks;
using System.IO;
using System.Text;
using System.Reflection;
using TaleWorlds.CampaignSystem.Conversation;
using TaleWorlds.CampaignSystem.Actions;
using VanillaStyleDiplomacy.Actions;

namespace VanillaStyleDiplomacy
{
    public class VanillaStyleDiplomacyModule : MBSubModuleBase
    {
        private static bool loaded = false;
        public static bool Loaded => loaded;

        protected override void OnSubModuleLoad()
        {
            LoggingManager.Instance.LogMessage($"Loading {ConstantsHelper.Name} version {ConstantsHelper.Version}...");
            UpdateManager.Instance.CheckNewestVersion();
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (loaded) // Check if we already here as it will be called everytime you move to the main menu
                return;

            loaded = true;
            LoggingManager.Instance.LogMessage($"Successfully loaded {ConstantsHelper.Name} version {ConstantsHelper.Version}!");
        }

        public override void OnInitialState()
        {
            if (UpdateManager.Instance.CanUpdate)
                InformationManager.ShowInquiry(new InquiryData("New version available!",
                "A new version of VanillaStyleDpilomacy is available! Do you want to update? (Restart required)",
                true,
                true,
                "Update now",
                "Dismiss",
                () =>
                {
                    Task.Run(() =>
                    {
                        UpdateManager.Instance.Update();
                    });
                },
                () => { }));
            else if (File.Exists($"{ConstantsHelper.ModuleFolder}/PatchNotes.txt"))
            {
                StreamReader reader = new StreamReader($"{ConstantsHelper.ModuleFolder}/PatchNotes.txt", Encoding.UTF8);
                InformationManager.ShowInquiry(new InquiryData("VSD Patch notes",
                reader.ReadToEnd(),
                true,
                false,
                "Ok",
                "",
                () => { File.Delete($"{ConstantsHelper.ModuleFolder}/PatchNotes.txt"); },
                () => { }));
                reader.Close();
            }            
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType.GetType() != typeof(Campaign))
                return;

            CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
            gameStarter.AddBehavior(new DiplomacyCampaignBehavior());

            if (ConfigReader.Instance.TryGetConfigOption("EnableExecutionPatch", out object patchExecution) && (bool)patchExecution)
            {
                MethodInfo oldExecutionMI = typeof(KillCharacterAction).GetMethod("ApplyByExecution");
                MethodInfo newExecutionMI = typeof(KillCharacterActionPatch).GetMethod("ApplyByExecution");
                if (!DetourUtility.TryDetourFromTo(oldExecutionMI, newExecutionMI))
                    LoggingManager.Instance.LogMessage("Failed to apply execution patch! Please view log file for more information.");
                else
                    LoggingManager.Instance.LogMessage("Execution patch applied!");
            }

            if (ConfigReader.Instance.TryGetConfigOption("EnableAllianceFeature", out object allianceFeature) && (bool)allianceFeature)
            {
                gameStarter.AddPlayerLine("alliance_proposal", "hero_main_options", "proposal_result", "Don't you think an alliance would benefit our peoples?", new ConversationSentence.OnConditionDelegate(allianceCondition), null);
                gameStarter.AddDialogLine("proposal_result", "proposal_result", "hero_main_options", "Unfortunately I don't think my vassals would agree with you.", null, null);
            }
        }

        private bool allianceCondition()
        {
            if (!Hero.MainHero.IsKingdomLeader || !Hero.OneToOneConversationHero.IsKingdomLeader)
                return false;
            return true;
        }

        protected override void OnSubModuleUnloaded()
        {
            LoggingManager.Instance.Dispose();
        }
    }
}