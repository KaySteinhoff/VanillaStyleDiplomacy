using System;
using System.Collections.Generic;
using System.Linq;
using Helpers;
using TaleWorlds.CampaignSystem;
using TaleWorlds.CampaignSystem.Actions;
using TaleWorlds.CampaignSystem.CharacterDevelopment;
using TaleWorlds.CampaignSystem.Election;
using TaleWorlds.CampaignSystem.Party;
using TaleWorlds.CampaignSystem.Roster;
using TaleWorlds.CampaignSystem.Settlements;
using TaleWorlds.CampaignSystem.Settlements.Locations;
using TaleWorlds.Core;
using TaleWorlds.Localization;
using VanillaStyleDiplomacy.Managers;

namespace VanillaStyleDiplomacy.Actions
{
    /// <summary>
    /// This class overrides the ApplyByExecution method of KillCharacterAction using the class DetourUtility
    /// The functions content are a straight rip from KillCharacterAction to achieve 100% control over the execution logic 
    /// </summary>
    public static class KillCharacterActionPatch
    {
        private static void ApplyInternal(Hero victim, Hero killer, KillCharacterAction.KillCharacterActionDetail actionDetail, bool showNotification = true, bool isForced = false)
        {
            // TODO: Check whats causing the crash when executing a prisoner and apply the correct penalty as defined by the config file
            if (!victim.CanDie(actionDetail) && !isForced)
            {
                return;
            }

            if (!victim.IsAlive)
            {
                LoggingManager.Instance.LogMessage($"Victim: {victim.Name} is already dead!");
                return;
            }

            if (victim.IsNotable && victim.Issue?.IssueQuest != null)
            {
                LoggingManager.Instance.LogMessage("Trying to kill a notable that has quest!");
            }

            if ((victim.PartyBelongedTo?.MapEvent != null || victim.PartyBelongedTo?.SiegeEvent != null) && victim.DeathMark == KillCharacterAction.KillCharacterActionDetail.None)
            {
                victim.AddDeathMark(killer, actionDetail);
                return;
            }

            CampaignEventDispatcher.Instance.OnBeforeHeroKilled(victim, killer, actionDetail, showNotification);
            if (victim.IsHumanPlayerCharacter && !isForced)
            {
                CampaignEventDispatcher.Instance.OnBeforeMainCharacterDied(victim, killer, actionDetail, showNotification);
                return;
            }

            victim.AddDeathMark(killer, actionDetail);
            victim.EncyclopediaText = CreateObituary(victim, actionDetail);
            if (victim.Clan != null && (victim.Clan.Leader == victim || victim == Hero.MainHero))
            {
                if (!victim.Clan.IsEliminated && victim != Hero.MainHero && victim.Clan.Heroes.Any((Hero x) => !x.IsChild && x != victim && x.IsAlive && x.IsLord))
                {
                    ChangeClanLeaderAction.ApplyWithoutSelectedNewLeader(victim.Clan);
                }

                if (victim.Clan.Kingdom != null && victim.Clan.Kingdom.RulingClan == victim.Clan)
                {
                    List<Clan> list = victim.Clan.Kingdom.Clans.Where((Clan t) => !t.IsEliminated && t.Leader != victim && !t.IsUnderMercenaryService).ToList();
                    if (list.IsEmpty())
                    {
                        if (!victim.Clan.Kingdom.IsEliminated)
                        {
                            DestroyKingdomAction.ApplyByKingdomLeaderDeath(victim.Clan.Kingdom);
                        }
                    }
                    else if (!victim.Clan.Kingdom.IsEliminated)
                    {
                        if (list.Count > 1)
                        {
                            Clan clanToExclude = ((victim.Clan.Leader == victim || victim.Clan.Leader == null) ? victim.Clan : null);
                            victim.Clan.Kingdom.AddDecision(new KingSelectionKingdomDecision(victim.Clan, clanToExclude), ignoreInfluenceCost: true);
                            if (clanToExclude != null)
                            {
                                Clan randomElementWithPredicate = victim.Clan.Kingdom.Clans.GetRandomElementWithPredicate((Clan t) => t != clanToExclude && Campaign.Current.Models.DiplomacyModel.IsClanEligibleToBecomeRuler(t));
                                ChangeRulingClanAction.Apply(victim.Clan.Kingdom, randomElementWithPredicate);
                            }
                        }
                        else
                        {
                            ChangeRulingClanAction.Apply(victim.Clan.Kingdom, list[0]);
                        }
                    }
                }
            }

            if (victim.PartyBelongedTo != null && (victim.PartyBelongedTo.LeaderHero == victim || victim == Hero.MainHero))
            {
                MobileParty partyBelongedTo = victim.PartyBelongedTo;
                if (victim.PartyBelongedTo.Army != null)
                {
                    if (victim.PartyBelongedTo.Army.LeaderParty == victim.PartyBelongedTo)
                    {
                        DisbandArmyAction.ApplyByArmyLeaderIsDead(victim.PartyBelongedTo.Army);
                    }
                    else
                    {
                        victim.PartyBelongedTo.Army = null;
                    }
                }

                if (partyBelongedTo != MobileParty.MainParty)
                {
                    partyBelongedTo.Ai.SetMoveModeHold();
                    if (victim.Clan != null && victim.Clan.IsRebelClan)
                    {
                        DestroyPartyAction.Apply(null, partyBelongedTo);
                    }
                }
            }

            MakeDead(victim);
            if (victim.GovernorOf != null)
            {
                ChangeGovernorAction.RemoveGovernorOf(victim);
            }

            if (actionDetail == KillCharacterAction.KillCharacterActionDetail.Executed && killer == Hero.MainHero && victim.Clan != null)
            {
                if (victim.GetTraitLevel(DefaultTraits.Honor) >= 0)
                {
                    TraitLevelingHelper.OnLordExecuted();
                }

                foreach (Clan clan in Clan.All)
                {
                    if (!clan.IsEliminated && !clan.IsBanditFaction && clan != Clan.PlayerClan)
                    {
                        bool showQuickNotification;
                        int relationChangeForExecutingHero = Models.ExecutionRelationModel.GetRelationChangeForExecutingHero(victim, clan.Leader, out showQuickNotification);
                        if (relationChangeForExecutingHero != 0)
                        {
                            ChangeRelationAction.ApplyPlayerRelation(clan.Leader, relationChangeForExecutingHero, showQuickNotification);
                        }
                    }
                }
            }

            if (victim.Clan != null && !victim.Clan.IsEliminated && !victim.Clan.IsBanditFaction && victim.Clan != Clan.PlayerClan)
            {
                if (victim.Clan.Leader == victim)
                {
                    DestroyClanAction.ApplyByClanLeaderDeath(victim.Clan);
                }
                else if (victim.Clan.Leader == null)
                {
                    DestroyClanAction.Apply(victim.Clan);
                }
            }

            CampaignEventDispatcher.Instance.OnHeroKilled(victim, killer, actionDetail, showNotification);
            if (victim.Spouse != null)
            {
                victim.Spouse = null;
            }

            if (victim.CompanionOf != null)
            {
                RemoveCompanionAction.ApplyByDeath(victim.CompanionOf, victim);
            }

            if (victim.CurrentSettlement != null)
            {
                if (victim.CurrentSettlement == Settlement.CurrentSettlement)
                {
                    LocationComplex.Current?.RemoveCharacterIfExists(victim);
                }

                if (victim.StayingInSettlement != null)
                {
                    victim.StayingInSettlement = null;
                }
            }

        }

        public static void ApplyByExecution(Hero victim, Hero executer, bool showNotification = true, bool isForced = false)
        {
            try
            {
                ApplyInternal(victim, executer, KillCharacterAction.KillCharacterActionDetail.Executed, showNotification, isForced);
            }
            catch (Exception e)
            {
                LoggingManager.Instance.LogException(e);
            }
        }

        private static void MakeDead(Hero victim, bool disbandVictimParty = true)
        {
            victim.ChangeState(Hero.CharacterStates.Dead);
            victim.DeathDay = CampaignTime.Now;
            if (!victim.IsHumanPlayerCharacter)
            {
                victim.ClearAttributes();
            }

            if (victim.PartyBelongedToAsPrisoner != null)
            {
                EndCaptivityAction.ApplyByDeath(victim);
            }

            if (victim.PartyBelongedTo != null)
            {
                MobileParty partyBelongedTo = victim.PartyBelongedTo;
                if (partyBelongedTo.LeaderHero == victim)
                {
                    bool flag = false;
                    if (!partyBelongedTo.IsMainParty)
                    {
                        foreach (TroopRosterElement item in partyBelongedTo.MemberRoster.GetTroopRoster())
                        {
                            if (item.Character.IsHero && item.Character != victim.CharacterObject)
                            {
                                partyBelongedTo.ChangePartyLeader(item.Character.HeroObject);
                                flag = true;
                                break;
                            }
                        }
                    }

                    if (!flag)
                    {
                        if (!partyBelongedTo.IsMainParty)
                        {
                            partyBelongedTo.RemovePartyLeader();
                        }

                        if (partyBelongedTo.IsActive && disbandVictimParty)
                        {
                            if (partyBelongedTo.Party.Owner?.CompanionOf == Clan.PlayerClan)
                            {
                                partyBelongedTo.Party.SetCustomOwner(Hero.MainHero);
                            }

                            partyBelongedTo.MemberRoster.RemoveTroop(victim.CharacterObject);
                            DisbandPartyAction.StartDisband(partyBelongedTo);
                        }
                    }
                }

                if (victim.PartyBelongedTo != null)
                {
                    partyBelongedTo.MemberRoster.RemoveTroop(victim.CharacterObject);
                }

                if (partyBelongedTo.IsActive && partyBelongedTo.MemberRoster.TotalManCount == 0)
                {
                    DestroyPartyAction.Apply(null, partyBelongedTo);
                }
            }
            else if (victim.IsHumanPlayerCharacter && !MobileParty.MainParty.IsActive)
            {
                DestroyPartyAction.Apply(null, MobileParty.MainParty);
            }
        }

        private static TextObject CreateObituary(Hero hero, KillCharacterAction.KillCharacterActionDetail detail)
        {
            TextObject textObject;
            if (hero.IsLord)
            {
                if (hero.Clan != null && hero.Clan.IsMinorFaction)
                {
                    textObject = new TextObject("{=L7qd6qfv}{CHARACTER.FIRSTNAME} was a member of the {CHARACTER.FACTION}. {FURTHER_DETAILS}.");
                }
                else
                {
                    textObject = new TextObject("{=mfYzCeGR}{CHARACTER.NAME} was {TITLE} of the {CHARACTER_FACTION_SHORT}. {FURTHER_DETAILS}.");
                    textObject.SetTextVariable("CHARACTER_FACTION_SHORT", hero.MapFaction.InformalName);
                    textObject.SetTextVariable("TITLE", HeroHelper.GetTitleInIndefiniteCase(hero));
                }
            }
            else if (hero.HomeSettlement != null)
            {
                textObject = new TextObject("{=YNXK352h}{CHARACTER.NAME} was a prominent {.%}{PROFESSION}{.%} from {HOMETOWN}. {FURTHER_DETAILS}.");
                textObject.SetTextVariable("PROFESSION", HeroHelper.GetCharacterTypeName(hero));
                textObject.SetTextVariable("HOMETOWN", hero.HomeSettlement.Name);
            }
            else
            {
                textObject = new TextObject("{=!}{FURTHER_DETAILS}.");
            }

            StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, textObject, includeDetails: true);
            TextObject empty = new TextObject();
            switch (detail)
            {
                case KillCharacterAction.KillCharacterActionDetail.DiedInBattle: empty = new TextObject("{=6pCABUme}{?CHARACTER.GENDER}She{?}He{\\?} died in battle in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                case KillCharacterAction.KillCharacterActionDetail.DiedInLabor: empty = new TextObject("{=7Vw6iYNI}{?CHARACTER.GENDER}She{?}He{\\?} died in childbirth in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                case KillCharacterAction.KillCharacterActionDetail.Executed: empty = new TextObject("{=9Tq3IAiz}{?CHARACTER.GENDER}She{?}He{\\?} was executed in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                case KillCharacterAction.KillCharacterActionDetail.Lost: empty = new TextObject("{=SausWqM5}{?CHARACTER.GENDER}She{?}He{\\?} disappeared in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                case KillCharacterAction.KillCharacterActionDetail.Murdered: empty = new TextObject("{=TUDAvcTR}{?CHARACTER.GENDER}She{?}He{\\?} was assassinated in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                case KillCharacterAction.KillCharacterActionDetail.WoundedInBattle: empty = new TextObject("{=LsBCQtVX}{?CHARACTER.GENDER}She{?}He{\\?} died of war-wounds in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
                default: empty = new TextObject("{=HU5n5KTW}{?CHARACTER.GENDER}She{?}He{\\?} died of natural causes in {YEAR} at the age of {CHARACTER.AGE}. {?CHARACTER.GENDER}She{?}He{\\?} was reputed to be {REPUTATION}"); break;
            }
            ;
            StringHelpers.SetCharacterProperties("CHARACTER", hero.CharacterObject, empty, includeDetails: true);
            empty.SetTextVariable("REPUTATION", CharacterHelper.GetReputationDescription(hero.CharacterObject));
            empty.SetTextVariable("YEAR", CampaignTime.Now.GetYear.ToString());
            textObject.SetTextVariable("FURTHER_DETAILS", empty);
            return textObject;
        }
    }
}