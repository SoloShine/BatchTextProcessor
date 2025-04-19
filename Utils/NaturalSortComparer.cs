
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BatchTextProcessor.Utils
{
    public class NaturalSortComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var regex = new Regex(@"\d+|\D+");
            var xMatches = regex.Matches(x);
            var yMatches = regex.Matches(y);

            int minLength = Math.Min(xMatches.Count, yMatches.Count);
            for (int i = 0; i < minLength; i++)
            {
                string xPart = xMatches[i].Value;
                string yPart = yMatches[i].Value;

                if (int.TryParse(xPart, out int xNum) && int.TryParse(yPart, out int yNum))
                {
                    int result = xNum.CompareTo(yNum);
                    if (result != 0) return result;
                }
                else
                {
                    int result = string.Compare(xPart, yPart, StringComparison.OrdinalIgnoreCase);
                    if (result != 0) return result;
                }
            }

            return xMatches.Count.CompareTo(yMatches.Count);
        }
    }
}
