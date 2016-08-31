using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovLib
{
    /// <summary>
    /// Generates sentences using an order-1 Markov chain of words.
    /// </summary>
    public class Order1SentenceGenerator: ISentenceGenerator
    {
        private readonly IMarkovChain<string> _chain;
        private readonly int _beginMarker;
        private readonly int _endMarker;

        private const string BeginMarkerName = "^";
        private const string EndMarkerName = "$";

        /// <summary>
        /// Creates a sentence generator which is backed by a Markov chain.
        /// </summary>
        /// <param name="chain">The chain to use. It may be mutated by the generator.</param>
        public Order1SentenceGenerator(IMarkovChain<string> chain)
        {
            _chain = chain;

            // Create marker states which represent the beginning and end of a sentence
            _beginMarker = _chain.FindOrCreateState(StateType.Marker, BeginMarkerName);
            _endMarker = _chain.FindOrCreateState(StateType.Marker, EndMarkerName);
        }

        /// <summary>
        /// Indicates to the generator that two words have appeared next to each other in a sentence.
        /// </summary>
        /// <param name="fromWord">The first word.</param>
        /// <param name="toWord">The second word.</param>
        public void FeedWordPair(string fromWord, string toWord)
        {
            var fromIndex = FindOrCreateWord(fromWord);
            var toIndex = FindOrCreateWord(toWord);
            _chain.AddLink(fromIndex, toIndex, 1);
        }

        /// <summary>
        /// Indicates to the generator that a word has appeared at the beginning of a sentence.
        /// </summary>
        /// <param name="word">The word.</param>
        public void FeedStartWord(string word)
        {
            var toIndex = FindOrCreateWord(word);
            _chain.AddLink(_beginMarker, toIndex, 1);
        }

        /// <summary>
        /// Indicates to the generator that a word has appeared at the end of a sentence.
        /// </summary>
        /// <param name="word">The word.</param>
        public void FeedEndWord(string word)
        {
            var fromIndex = FindOrCreateWord(word);
            _chain.AddLink(fromIndex, _endMarker, 1);
        }

        public void FeedLine(string line)
        {
            string lastWord = null;
            var words = line.Split(' ');
            foreach (var word in words.Where(word => !string.IsNullOrWhiteSpace(word)))
            {
                if (lastWord != null)
                    FeedWordPair(lastWord, word);
                else
                    FeedStartWord(word);
                lastWord = word;
            }
            if (lastWord != null)
                FeedEndWord(lastWord);
        }

        public IEnumerable<string> GenerateSentence(Random random)
        {
            return GenerateSentenceFromState(_beginMarker, random);
        }

        /// <summary>
        /// Generates a random sentence which starts with a particular word.
        /// If the generator does not know about the word, then a random word will be chosen.
        /// </summary>
        /// <param name="startWord">The word to start with.</param>
        /// <param name="random">The random number generator.</param>
        /// <returns>The words in the sentence.</returns>
        public IEnumerable<string> GenerateSentenceFrom(string startWord, Random random)
        {
            var state = _chain.FindState(StateType.Value, startWord);
            return GenerateSentenceFromState(state >= 0 ? state : _beginMarker, random);
        } 

        private IEnumerable<string> GenerateSentenceFromState(int startState, Random random)
        {
            var currentState = startState;
            while (currentState != _endMarker)
            {
                if (currentState != _beginMarker)
                    yield return _chain.GetValue(currentState);
                if (!_chain.GetRandomNextState(currentState, random, out currentState))
                    break;
            }
        }

        private int FindOrCreateWord(string word)
        {
            return _chain.FindOrCreateState(StateType.Value, word);
        }
    }
}
