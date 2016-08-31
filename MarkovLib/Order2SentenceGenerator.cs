using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovLib
{
    /// <summary>
    /// Generates sentences using an order-2 Markov chain of words.
    /// </summary>
    public class Order2SentenceGenerator : ISentenceGenerator
    {
        private readonly IMarkovChain<WordPair> _chain;
        private readonly int _beginMarker;
        private readonly int _endMarker;

        private static readonly WordPair BeginMarkerPair = new WordPair("^");
        private static readonly WordPair EndMarkerPair = new WordPair("$");

        /// <summary>
        /// Creates a sentence generator which is backed by a Markov chain.
        /// </summary>
        /// <param name="chain">The chain to use. It may be mutated by the generator.</param>
        public Order2SentenceGenerator(IMarkovChain<WordPair> chain)
        {
            if (chain == null)
                throw new ArgumentNullException(nameof(chain));

            _chain = chain;

            // Create marker states used to represent the beginning and end of a sentence
            _beginMarker = _chain.FindOrCreateState(StateType.Marker, BeginMarkerPair);
            _endMarker = _chain.FindOrCreateState(StateType.Marker, EndMarkerPair);
        }

        /// <summary>
        /// Indicates to the generator that three words have appeared next to each other in a sentence.
        /// </summary>
        /// <param name="word1">The first word.</param>
        /// <param name="word2">The second word.</param>
        /// <param name="word3">The third word.</param>
        public void FeedWordTriplet(string word1, string word2, string word3)
        {
            if (word1 == null)
                throw new ArgumentNullException(nameof(word1));
            if (word2 == null)
                throw new ArgumentNullException(nameof(word2));
            if (word3 == null)
                throw new ArgumentNullException(nameof(word3));

            var fromIndex = GetOrCreatePair(word1, word2);
            var toIndex = GetOrCreatePair(word2, word3);
            _chain.AddLink(fromIndex, toIndex, 1);
        }

        /// <summary>
        /// Indicates to the generator that two words have appeared next to each other at the beginning of a sentence.
        /// </summary>
        /// <param name="word1">The first word.</param>
        /// <param name="word2">The second word.</param>
        public void FeedStartPair(string word1, string word2)
        {
            if (word1 == null)
                throw new ArgumentNullException(nameof(word1));
            if (word2 == null)
                throw new ArgumentNullException(nameof(word2));

            var toIndex = GetOrCreatePair(word1, word2);
            _chain.AddLink(_beginMarker, toIndex, 1);
        }

        /// <summary>
        /// Indicates to the generator that two words have appeared next to each other at the end of a sentence.
        /// </summary>
        /// <param name="word1">The first word.</param>
        /// <param name="word2">The second word.</param>
        public void FeedEndPair(string word1, string word2)
        {
            if (word1 == null)
                throw new ArgumentNullException(nameof(word1));
            if (word2 == null)
                throw new ArgumentNullException(nameof(word2));

            var fromIndex = GetOrCreatePair(word1, word2);
            _chain.AddLink(fromIndex, _endMarker, 1);
        }

        /// <summary>
        /// Indicates to the generator that a word is the only word in a sentence.
        /// </summary>
        /// <param name="word">The word.</param>
        public void FeedLoneWord(string word)
        {
            if (word == null)
                throw new ArgumentNullException(nameof(word));

            var state = GetOrCreatePair(word, null);
            _chain.AddLink(_beginMarker, state, 1);
            _chain.AddLink(state, _endMarker, 1);
        }

        public void FeedLine(string line)
        {
            if (line == null)
                throw new ArgumentNullException(nameof(line));

            var words = line.Split(' ');
            string prevWord2 = null, prevWord1 = null;
            foreach (var word in words.Where(word => !string.IsNullOrWhiteSpace(word)))
            {
                if (prevWord2 == null && prevWord1 != null)
                    FeedStartPair(prevWord1, word);
                else if (prevWord2 != null)
                    FeedWordTriplet(prevWord2, prevWord1, word);
                prevWord2 = prevWord1;
                prevWord1 = word;
            }
            if (prevWord2 != null)
                FeedEndPair(prevWord2, prevWord1);
            else if (prevWord1 != null)
                FeedLoneWord(prevWord1);
        }

        public IEnumerable<string> GenerateSentence(Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return GenerateSentenceFromState(_beginMarker, random);
        }

        /// <summary>
        /// Generates a random sentence which starts with a particular word pair.
        /// If the generator does not know about the pair, then a random pair will be chosen.
        /// </summary>
        /// <param name="startPair">The pair to start with.</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>The words in the sentence.</returns>
        public IEnumerable<string> GenerateSentenceFrom(WordPair startPair, Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            var state = _chain.FindState(StateType.Value, startPair);
            return GenerateSentenceFromState(state >= 0 ? state : _beginMarker, random);
        }

        private IEnumerable<string> GenerateSentenceFromState(int startState, Random random)
        {
            var currentState = startState;
            string lastWord = null;
            while (currentState != _endMarker)
            {
                if (currentState != _beginMarker)
                {
                    var val = _chain.GetValue(currentState);
                    lastWord = val.Second;
                    yield return val.First;
                }
                if (!_chain.GetRandomNextState(currentState, random, out currentState))
                    break;
            }
            if (lastWord != null)
                yield return lastWord;
        }

        private int GetOrCreatePair(string word1, string word2)
        {
            return _chain.FindOrCreateState(StateType.Value, new WordPair(word1, word2));
        }
    }
}
