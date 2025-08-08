using VanillaStyleDiplomacy.Logging;
using TaleWorlds.MountAndBlade;
using VanillaStyleDiplomacy.Config;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.MountAndBlade.GauntletUI.Widgets.Nameplate;
using TaleWorlds.CampaignSystem.CampaignBehaviors;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Localization;
using System;
using TaleWorlds.Core;
using NetworkMessages.FromServer;
using VanillaStyleDiplomacy.Saving;
using VanillaStyleDiplomacy.Helpers;

namespace VanillaStyleDiplomacy
{
    public class VanillaStyleDiplomacyModule : MBSubModuleBase
    {
        private static bool loaded = false;
        public static bool Loaded => loaded;

        protected override void OnSubModuleLoad()
        {
            LoggingManager.Instance.LogMessage($"Loading {ConstantsHelper.Name} version {ConstantsHelper.Version}...");

            //CampaignEvents.BeforeHeroKilledEvent.ClearListeners(CampaignEvents.BeforeHeroKilledEvent);
        }

        protected override void OnBeforeInitialModuleScreenSetAsRoot()
        {
            if (loaded) // Check if we already here as it will be called everytime you move to the main menu
                return;

            loaded = true;
            LoggingManager.Instance.LogMessage($"Successfully loaded {ConstantsHelper.Name} version {ConstantsHelper.Version}!");
        }

        protected override void OnGameStart(Game game, IGameStarter gameStarterObject)
        {
            base.OnGameStart(game, gameStarterObject);

            if (game.GameType.GetType() != typeof(Campaign))
                return;

            CampaignGameStarter gameStarter = (CampaignGameStarter)gameStarterObject;
            gameStarter.AddBehavior(new DiplomacyCampaignBehavior());
        }

        protected override void OnSubModuleUnloaded()
        {
            LoggingManager.Instance.Dispose();
        }
    }
}