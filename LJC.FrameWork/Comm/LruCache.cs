using System;
using System.Collections.Generic;

namespace LJC.FrameWork.Comm
{
    // Simple thread-safe LRU cache implementation
    public class LruCache<TKey, TValue>
    {
        private readonly int capacityCount; // if >0, use count-based eviction
        private readonly long capacityBytes; // if >0, use bytes-based eviction
        private long currentBytes;
        private readonly Dictionary<TKey, LinkedListNode<CacheItem>> map;
        private readonly LinkedList<CacheItem> list;
        private readonly object sync = new object();

        private class CacheItem
        {
            public TKey Key;
            public TValue Value;
            public long Size;
        }

        // count-based constructor
        public LruCache(int capacity)
        {
            this.capacityCount = Math.Max(1, capacity);
            this.capacityBytes = 0;
            map = new Dictionary<TKey, LinkedListNode<CacheItem>>();
            list = new LinkedList<CacheItem>();
        }

        // bytes-based constructor
        public LruCache(long capacityBytes)
        {
            this.capacityCount = 0;
            this.capacityBytes = Math.Max(1, capacityBytes);
            map = new Dictionary<TKey, LinkedListNode<CacheItem>>();
            list = new LinkedList<CacheItem>();
            currentBytes = 0;
        }

        public bool TryGet(TKey key, out TValue value)
        {
            lock (sync)
            {
                if (map.TryGetValue(key, out var node))
                {
                    // move to front
                    list.Remove(node);
                    list.AddFirst(node);
                    value = node.Value.Value;
                    return true;
                }
            }
            value = default(TValue);
            return false;
        }

        // estimatedSize: approximate size in bytes for this entry (used when bytes-based eviction enabled)
        public void Add(TKey key, TValue value, long estimatedSize = 0)
        {
            lock (sync)
            {
                if (map.TryGetValue(key, out var node))
                {
                    // update and move to front
                    list.Remove(node);
                    node.Value.Value = value;
                    // adjust size accounting
                    if (capacityBytes > 0)
                    {
                        currentBytes -= node.Value.Size;
                        node.Value.Size = estimatedSize;
                        currentBytes += node.Value.Size;
                    }
                    list.AddFirst(node);
                    map[key] = node;
                }
                else
                {
                    var item = new CacheItem { Key = key, Value = value, Size = estimatedSize };
                    var newNode = list.AddFirst(item);
                    map[key] = newNode;
                    if (capacityBytes > 0)
                    {
                        currentBytes += estimatedSize;
                        // evict until under capacity
                        while (currentBytes > capacityBytes && list.Last != null)
                        {
                            var last = list.Last;
                            map.Remove(last.Value.Key);
                            currentBytes -= last.Value.Size;
                            list.RemoveLast();
                        }
                    }
                    else
                    {
                        if (map.Count > capacityCount)
                        {
                            var last = list.Last;
                            if (last != null)
                            {
                                map.Remove(last.Value.Key);
                                list.RemoveLast();
                            }
                        }
                    }
                }
            }
        }
    }
}
