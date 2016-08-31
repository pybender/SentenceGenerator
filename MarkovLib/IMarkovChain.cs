using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MarkovLib
{
    public interface IMarkovChain<TValue>
    {
        /// <summary>
        /// Finds a state in the chain.
        /// </summary>
        /// <param name="type">The state's type.</param>
        /// <param name="value">The value to associate with the state.</param>
        /// <returns>The state's index if found, or -1 otherwise.</returns>
        int FindState(StateType type, TValue value);

        /// <summary>
        /// Finds a state in the chain or creates a new one if it does not exist.
        /// </summary>
        /// <param name="type">The state's type.</param>
        /// <param name="value">The value to associate with the state.</param>
        /// <returns>The state's index.</returns>
        int FindOrCreateState(StateType type, TValue value);

        /// <summary>
        /// Gets the value associated with a state.
        /// </summary>
        /// <param name="state">The state's index.</param>
        /// <returns>The state's value.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is invalid.</exception>
        TValue GetValue(int state);

        /// <summary>
        /// Adds a link between two states.
        /// </summary>
        /// <param name="fromState">The index of the source state.</param>
        /// <param name="toState">To index of the state to link to.</param>
        /// <param name="weight">The weight of the link.</param>
        /// <exception cref="IndexOutOfRangeException">Thrown if an index is invalid.</exception>
        void AddLink(int fromState, int toState, long weight);

        /// <summary>
        /// Given a state, randomly determines what the next state will be.
        /// </summary>
        /// <param name="fromState">The state to start from.</param>
        /// <param name="random">The random number generator.</param>
        /// <param name="nextState">On output, this will hold the index of the next state if one is found.</param>
        /// <returns><c>true</c> if the next state was found.</returns>
        /// <exception cref="IndexOutOfRangeException">Thrown if the index is invalid.</exception>
        bool GetRandomNextState(int fromState, Random random, out int nextState);
    }

    /// <summary>
    /// The type of a state.
    /// </summary>
    /// <remarks>
    /// This only matters when trying to find a state based on its value.
    /// It can be useful to create marker states that have values for debugging purposes which shouldn't be able to be found normally.
    /// <see cref="Value"/> states can still link to <see cref="Marker"/> states and vice-versa.
    /// </remarks>
    public enum StateType
    {
        Value,
        Marker,
    }
}
