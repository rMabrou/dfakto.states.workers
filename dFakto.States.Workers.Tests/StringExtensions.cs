using System;
using System.Linq;

namespace dFakto.StepFunctions.Workers.Tests
{
    public static class StringUtils
    {
        private static readonly Random random = new Random();
        public static string Random(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}