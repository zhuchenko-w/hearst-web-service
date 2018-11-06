using HearstWebService.Interfaces;
using System;
using System.Runtime.Caching;

namespace HearstWebService.Common.Cache
{
    public class InMemoryCache : ICache
    {
        protected readonly MemoryCache _cache;

        public InMemoryCache()
        {
            _cache = MemoryCache.Default;
        }

        public bool RemoveItem(string key)
        {
            return _cache.Remove(key) != null;
        }

        public TItem GetItem<TItem>(string key) where TItem : class
        {
            return _cache[key] as TItem;
        }

        public void SetOrUpdateItem<TItem>(string key, TimeSpan expiration, TItem value) where TItem : class
        {
            _cache.Set(key, value, DateTime.Now.Add(expiration));
        }
    }
}
