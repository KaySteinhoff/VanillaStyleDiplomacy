namespace VanillaStyleDiplomacy.Helpers
{
    public static class ConstantsHelper
    {
        private static string moduleFolder = "../../Modules/VanillaStyleDiplomacy";
        private static string defaultConfigFile = "# Config options for the VanillaStyleDiplomacy mod\n" +
                                                    "# Options are to be defined as follows:\n" +
                                                    "# [option type] [option name]=[option value]\n" +
                                                    "# Note that at least one whitespace character(space, tab) between [option type] and [option name] is required where as whitespace characters between [option name], the equals sign and [option value] are optional\n" +
                                                    "# whitespace characters inside [option name] are prohibited as well as in [option value], except for the case that it is of type string\n" +
                                                    "# \n" +
                                                    "# Supported option types are: \n" +
                                                    "# int: for integers, example: 13, 5, 100, etc.\n" +
                                                    "# double: for floating point values, example: 3.141592653589, 8.25, etc.\n" +
                                                    "# bool: for trueness, example: true, false(constants are case insensitive)\n" +
                                                    "# string: for strings of characters, example: She sells sea shells on the sea shore, Who lives in a pineapple under the sea?\n" +
                                                    "# \n" +
                                                    "# Comments are always single line and are to be prefixed with a '#'\n" +
                                                    "# strings are cut starting from the first non-whitespace character after the equals sign to the last non-whitespace character of that line\n" +
                                                    "# \n" +
                                                    "int SaveId=853743654\n" +
                                                    "bool EnableExecutionPatch=true # All 'ExecutionRelation*' options are optional, depending on wether this option is set to 'true' or 'false'\n" +
                                                    "int ExecutionRelationPenaltyFamily=-30\n" +
                                                    "int ExecutionRelationPenaltyAllies=-10\n" +
                                                    "int ExecutionRelationPenaltyNeutral=0\n" +
                                                    "int ExecutionRelationPenaltyVictimEnemies=15\n" +
                                                    "int ExecutionRelationPenaltyKillerEnemies=-15\n" +
                                                    "bool EnableAllianceFeature=true\n";
        private static string name = typeof(VanillaStyleDiplomacyModule).Namespace;
        private static string version = typeof(VanillaStyleDiplomacyModule).Assembly.GetName().Version?.ToString(3);

        public static string Name => name;
        public static string Version => version;
        public static string ModuleFolder => moduleFolder;
        public static string DefaultConfigFile => defaultConfigFile;
    }
}