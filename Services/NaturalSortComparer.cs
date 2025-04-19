
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace TextMergingTool.Services
{
    public class NaturalSortComparer : IComparer<string?>
    {
        public int Compare(string? x, string? y)
        {
            if (x == null && y == null) return 0;
            if (x == null) return -1;
            if (y == null) return 1;

            var regex = new Regex(@"(\d+)");
            var xMatch = regex.Match(x);
            var yMatch = regex.Match(y);

            if (xMatch.Success && yMatch.Success)
            {
                if (int.TryParse(xMatch.Value, out int xNum) && int.TryParse(yMatch.Value, out int yNum))
                {
                    return xNum.CompareTo(yNum);
                }
            }

            return string.Compare(x, y, StringComparison.OrdinalIgnoreCase);
        }
    }
}
