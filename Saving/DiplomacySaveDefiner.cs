using System.Collections.Generic;
using VanillaStyleDiplomacy.Config;
using VanillaStyleDiplomacy.Saving.SavableInfo;
using TaleWorlds.SaveSystem;

namespace VanillaStyleDiplomacy.Saving
{
    public class DiplomacySaveDefiner : SaveableTypeDefiner
    {
        public DiplomacySaveDefiner() : base((int)ConfigReader.Instance["SaveId"])
        {
        }

        protected override void DefineClassTypes()
        {
            AddClassDefinition(typeof(AllianceInfo), 1);
        }

        protected override void DefineContainerDefinitions()
        {
            ConstructContainerDefinition(typeof(List<AllianceInfo>));
        }
    }
}