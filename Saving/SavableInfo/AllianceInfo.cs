using VanillaStyleDiplomacy.Interfaces;
using TaleWorlds.CampaignSystem;
using TaleWorlds.Library;
using TaleWorlds.SaveSystem;

namespace VanillaStyleDiplomacy.Saving.SavableInfo
{
    public class AllianceInfo : ISaveableInfo
    {
        public AllianceInfo(Kingdom initiator, Kingdom participator, bool allianceMade)
        {
            Initiator = initiator;
            Participator = participator;
            AllianceColor = Color.FromUint((uint)(initiator.StringId + participator.StringId).GetHashCode() & 0x00ffffff);
        }

        [SaveableProperty(0)]
        public Kingdom Initiator { get; set; }
        [SaveableProperty(1)]
        public Kingdom Participator { get; set; }
        [SaveableProperty(2)]
        public Color AllianceColor { get; set; }
    }
}