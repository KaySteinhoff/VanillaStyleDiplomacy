using System;
using System.Text.RegularExpressions;

namespace VanillaStyleDiplomacy.Extensions
{
    public static class GroupCollectionExtensions
    {
        public static bool Any(this GroupCollection collection, Func<Group, bool> predicate)
        {
            for (int i = 0; i < collection.Count; ++i)
                if (predicate(collection[i]))
                    return true;
            return false;
        }
    }
}