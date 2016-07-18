using System.Collections.Generic;

namespace CommonHelper
{
    public static class CollectionExtensions
    {
        /// <summary>
		/// Splits colelction on partitions of specified size.
		/// </summary>
		public static IEnumerable<IEnumerable<T>> Partition<T>(this IEnumerable<T> collection, int partitionSize)
        {
            var nextPartition = new List<T>(partitionSize);
            foreach (T item in collection)
            {
                nextPartition.Add(item);
                if (nextPartition.Count == partitionSize)
                {
                    yield return nextPartition;
                    nextPartition = new List<T>(partitionSize);
                }
            }
            if (nextPartition.Count > 0)
                yield return nextPartition;
        }
    }
}
