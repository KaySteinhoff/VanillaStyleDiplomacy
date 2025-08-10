using TaleWorlds.CampaignSystem;
using VanillaStyleDiplomacy.Config;
using VanillaStyleDiplomacy.Managers;

namespace VanillaStyleDiplomacy.Models
{
    public class ExecutionRelationModel
    {
        private static int familyPenalty = -30;
        private static int allyPenalty = -10;
        private static int neutralPenalty = 0;
        private static int victimEnemyPenalty = 15;
        private static int playerEnemyPenalty = -15;
        private static bool configLoaded = false;

        private static void LoadConfigOptions()
        {
            object tmp = null;
            if (!ConfigReader.Instance.TryGetConfigOption("ExecutionRelationPenaltyFamily", out tmp))
                LoggingManager.Instance.LogMessage($"No 'ExecutionRelationPenaltyFamily' option found! Using default...");
            else
                familyPenalty = (int)tmp;

            if (!ConfigReader.Instance.TryGetConfigOption("ExecutionRelationPenaltyAllies", out tmp))
                LoggingManager.Instance.LogMessage($"No 'ExecutionRelationPenaltyAllies' option found! Using default...");
            else
                allyPenalty = (int)tmp;

            if (!ConfigReader.Instance.TryGetConfigOption("ExecutionRelationPenaltyNeutral", out tmp))
                LoggingManager.Instance.LogMessage($"No 'ExecutionRelationPenaltyNeutral' option found! Using default...");
            else
                neutralPenalty = (int)tmp;

            if (!ConfigReader.Instance.TryGetConfigOption("ExecutionRelationPenaltyVictimEnemies", out tmp))
                LoggingManager.Instance.LogMessage($"No 'ExecutionRelationPenaltyVictimEnemies' option found! Using default...");
            else
                victimEnemyPenalty = (int)tmp;

            if (!ConfigReader.Instance.TryGetConfigOption("ExecutionRelationPenaltyKillerEnemies", out tmp))
                LoggingManager.Instance.LogMessage($"No 'ExecutionRelationPenaltyKillerEnemies' option found! Using default...");
            else
                playerEnemyPenalty = (int)tmp;
        }

        public static int GetRelationChangeForExecutingHero(Hero victim, Hero observer, out bool showNotification)
        {
            showNotification = true;
            if (!configLoaded)
            {
                configLoaded = true;
                LoadConfigOptions();
            }

            // Bandits, Mafias, and Outlaws are exempt from execution penalties
            if (observer.Clan.IsBanditFaction || observer.Clan.IsMafia || observer.Clan.IsOutlaw)
                return 0;

            if (victim.Clan == observer.Clan)
                    return familyPenalty;
                else if (victim.Clan.Kingdom == observer.Clan.Kingdom)
                    return allyPenalty;
                else if (victim.Clan.IsAtWarWith(observer.Clan))
                    return victimEnemyPenalty;
                else if (Hero.MainHero.Clan.IsAtWarWith(observer.Clan))
                    return playerEnemyPenalty;
            return neutralPenalty;
        }
    }
}