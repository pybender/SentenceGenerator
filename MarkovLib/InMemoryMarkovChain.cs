using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovLib
{
    /// <summary>
    /// A Markov chain which resides completely in memory.
    /// </summary>
    /// <typeparam name="TValue">The type of value to store.</typeparam>
    /// <seealso cref="MarkovLib.IMarkovChain{TValue}" />
    public class InMemoryMarkovChain<TValue> : IMarkovChain<TValue>
    {
        private readonly List<InMemoryMarkovState<TValue>> _states = new List<InMemoryMarkovState<TValue>>();
        private readonly Dictionary<StateType, Dictionary<TValue, int>> _stateIndexes = new Dictionary<StateType, Dictionary<TValue, int>>();

        /// <summary>
        /// Constructs a new in-memory Markov chain using the default equality comparer.
        /// </summary>
        public InMemoryMarkovChain() : this(EqualityComparer<TValue>.Default)
        {
        }

        /// <summary>
        /// Constructs a new in-memory Markov chain using a custom equality comparer.
        /// </summary>
        /// <param name="comparer">The equality comparer to use to search for states.</param>
        public InMemoryMarkovChain(IEqualityComparer<TValue> comparer)
        {
            if (comparer == null)
                throw new ArgumentNullException(nameof(comparer));

            _stateIndexes[StateType.Value] = new Dictionary<TValue, int>(comparer);
            _stateIndexes[StateType.Marker] = new Dictionary<TValue, int>(comparer);
        }

        public int FindState(StateType type, TValue value)
        {
            int index;
            if (!_stateIndexes[type].TryGetValue(value, out index))
                return -1;
            return index;
        }

        public int FindOrCreateState(StateType type, TValue value)
        {
            int index;
            var indexes = _stateIndexes[type];
            if (!indexes.TryGetValue(value, out index))
            {
                var state = new InMemoryMarkovState<TValue>(value);
                index = _states.Count;
                _states.Add(state);
                indexes[value] = index;
            }
            return index;
        }

        public TValue GetValue(int state)
        {
            ThrowIfStateIsInvalid(nameof(state), state);
            return _states[state].Value;
        }

        public void AddLink(int fromState, int toState, long weight)
        {
            ThrowIfStateIsInvalid(nameof(fromState), fromState);
            ThrowIfStateIsInvalid(nameof(toState), toState);
            _states[fromState].AddLink(toState, weight);
        }

        public bool GetRandomNextState(int fromState, Random random, out int nextState)
        {
            ThrowIfStateIsInvalid(nameof(fromState), fromState);
            return _states[fromState].GetRandomNextState(random, out nextState);
        }

        private void ThrowIfStateIsInvalid(string name, int index)
        {
            if (index < 0 || index >= _states.Count)
                throw new IndexOutOfRangeException($"Invalid {name} index: {index}");
        }
    }
}
