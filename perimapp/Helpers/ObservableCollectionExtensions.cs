using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace perimapp.Helpers
{
    public static class ObservableCollectionExtensions
    {
        /// <summary>
        /// Updates an ObservableCollection efficiently by comparing existing items with new items
        /// This avoids the performance hit of Clear() + AddRange()
        /// </summary>
        public static void UpdateWith<T>(this ObservableCollection<T> collection, 
            IEnumerable<T> newItems, 
            Func<T, T, bool> comparer)
        {
            var newList = newItems.ToList();
            
            // Remove items that are no longer in the new list
            for (int i = collection.Count - 1; i >= 0; i--)
            {
                if (!newList.Any(newItem => comparer(collection[i], newItem)))
                {
                    collection.RemoveAt(i);
                }
            }
            
            // Add or update items
            foreach (var newItem in newList)
            {
                var existingItem = collection.FirstOrDefault(existing => comparer(existing, newItem));
                if (existingItem != null)
                {
                    // Update existing item if needed
                    var index = collection.IndexOf(existingItem);
                    collection[index] = newItem;
                }
                else
                {
                    // Add new item
                    collection.Add(newItem);
                }
            }
        }
        
        /// <summary>
        /// Replaces collection content efficiently, maintaining sort order
        /// </summary>
        public static void ReplaceWith<T>(this ObservableCollection<T> collection, 
            IEnumerable<T> newItems)
        {
            var newList = newItems.ToList();
            
            // Only update if the collections are different
            if (collection.Count != newList.Count || 
                !collection.SequenceEqual(newList))
            {
                collection.Clear();
                foreach (var item in newList)
                {
                    collection.Add(item);
                }
            }
        }
    }
}