using System;

namespace clurun
{
    internal class CommandMatcher
    {
        public static object GetMatchScore(string args1, string args2)
        {
            string args1ToCompare = string.Join(";", args1.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            string args2ToCompare = string.Join(";", args2.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));
            return MatchScore(args1, args2);
        }

        internal static int MatchScore(string semiColonSeparatedArgs, string semiColonSeparatedCommand)
        {
            int score = 0;
            for (int charPos = 0; charPos < Math.Min(semiColonSeparatedArgs.Length, semiColonSeparatedCommand.Length); ++charPos)
            {
                if (semiColonSeparatedArgs[charPos] != semiColonSeparatedCommand[charPos])
                {
                    break;
                }
                else
                {
                    score = charPos + 1;
                }
            }

            return score;
        }
    }
}