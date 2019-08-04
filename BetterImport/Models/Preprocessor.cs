using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace BetterImport.Models
{
    public class Preprocessor
    {
        public int DataFileIndex { get; private set; }
        public Regex Regex { get; private set; }
        public string ReplacementValue { get; private set; }

        public Preprocessor(int dataFileIndex, string pattern, string replacementValue)
        {
            DataFileIndex = dataFileIndex;
            Regex = new Regex(pattern);
            ReplacementValue = replacementValue;
        }

        public string Replace(string s)
        {
            return Regex.Replace(s, ReplacementValue);
        }

        public static void BatchReplace(IList<Preprocessor> preprocessors, ref string[][] values)
        {
            if (preprocessors.Count == 0) return;

            for (int row = 0; row < values.Length; row++)
            {
                if (values[row] != null)
                {
                    for (int col = 0; col < values[row].Length; col++)
                    {
                        for (int p = 0; p < preprocessors.Count; p++)
                        {
                            // -1 for 1-indexing
                            if (values[row][col] != null && (preprocessors[p].DataFileIndex + -1) == col) values[row][col] = preprocessors[p].Replace(values[row][col]);
                        }
                    }
                }
            }
        }
    }
}
