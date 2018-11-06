using System;

namespace HearstWebService.Interfaces
{
    public interface ICache
    {
        bool RemoveItem(string key);
        TItem GetItem<TItem>(string key) where TItem : class;
        void SetOrUpdateItem<TItem>(string key, TimeSpan expiration, TItem value) where TItem : class;
    }
}
