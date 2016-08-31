using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MarkovLib.Common;

namespace MarkovLib
{
    internal class InMemoryMarkovState<TValue>
    {
        private readonly List<Link> _links = new List<Link>(); // Sorted in order of descending weight starting from _firstSortedLink
        private readonly SortedList<int, int> _linksByStateIndex = new SortedList<int, int>(); // SortedList to keep memory usage lower

        private int _firstSortedLink;

        public InMemoryMarkovState(TValue val)
        {
            Value = val;
        }

        public TValue Value { get; }

        public void AddLink(int toIndex, long weight)
        {
            int index;
            if (!_linksByStateIndex.TryGetValue(toIndex, out index))
            {
                var link = new Link(toIndex);
                index = _links.Count;
                _links.Add(link);
                _linksByStateIndex[toIndex] = index;
            }
            IncrementLink(index, weight);
        }

        public bool GetRandomNextState(Random random, out int nextState)
        {
            if (_links.Count == 0)
            {
                nextState = -1;
                return false;
            }

            // Get a weighted random value by using binary search on the cumulative weights
            UpdateLinks();
            var totalWeight = _links[0].CumulativeWeight + _links[0].Weight;
            var weight = random.NextLong(totalWeight);
            var index = BinarySearch.Search(_links, weight, (x, y) => Math.Sign(y - x), l => l.CumulativeWeight);
            if (index < 0)
                index = ~index;
            nextState = _links[index].ToIndex;
            return true;
        }

        private void IncrementLink(int index, long weight)
        {
            _links[index].Weight += Math.Max(0, weight);

            // Indicate that the sorting order is now broken starting with this link
            _firstSortedLink = Math.Max(_firstSortedLink, index + 1);
        }

        private void UpdateLinks()
        {
            if (_firstSortedLink == 0)
                return;

            // Sort the bad section of links in reverse order
            var reverseComparer = Comparer<Link>.Create((x, y) => Math.Sign(y.Weight - x.Weight));
            _links.Sort(0, _firstSortedLink, reverseComparer);

            // Now recompute their indexes and cumulative weights
            var cumulativeWeight = 0L;
            if (_firstSortedLink < _links.Count - 1)
            {
                var link = _links[_firstSortedLink];
                cumulativeWeight = link.CumulativeWeight + link.Weight;
            }
            for (var i = _firstSortedLink - 1; i >= 0; i--)
            {
                var link = _links[i];
                link.CumulativeWeight = cumulativeWeight;
                _linksByStateIndex[link.ToIndex] = i;
                cumulativeWeight += link.Weight;
            }
            _firstSortedLink = 0;
        }

        private class Link
        {
            public Link(int toIndex)
            {
                ToIndex = toIndex;
            }

            public int ToIndex { get; }

            public long Weight { get; set; }

            /// <summary>
            /// Gets or sets the sum of all <see cref="Weight"/> values of the links after this link in the sorted list.
            /// This is lazily updated and can only be relied upon after <see cref="InMemoryMarkovState{TValue}.UpdateLinks"/> is called.
            /// </summary>
            public long CumulativeWeight { get; set; }
        }
    }
}
