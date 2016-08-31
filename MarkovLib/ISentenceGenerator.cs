using System;
using System.Collections.Generic;

namespace MarkovLib
{
    public interface ISentenceGenerator
    {
        /// <summary>
        /// Analyzes the words in a line.
        /// </summary>
        /// <param name="line">The line to analyze.</param>
        void FeedLine(string line);

        /// <summary>
        /// Generates a random sentence.
        /// </summary>
        /// <param name="random">The random number generator.</param>
        /// <returns>The words in the sentence.</returns>
        IEnumerable<string> GenerateSentence(Random random);
    }
}