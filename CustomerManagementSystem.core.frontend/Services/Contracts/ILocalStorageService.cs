namespace CustomerManagementSystem.core.frontend.Services.Contracts
{
    /// <summary>
    /// Service for local storage operations.
    /// </summary>
    public interface ILocalStorageService
    {
        /// <summary>
        /// Sets an item in local storage.
        /// </summary>
        /// <typeparam name="T">The type of the item to set.</typeparam>
        /// <param name="key">The key to set the item under.</param>
        /// <param name="value">The item to set.</param>
        /// <returns>A ValueTask that represents the asynchronous operation.</returns>
        ValueTask SetItemAsync<T>(string key, T value);

        /// <summary>
        /// Gets an item from local storage.
        /// </summary>
        /// <typeparam name="T">The type of the item to get.</typeparam>
        /// <param name="key">The key of the item to get.</param>
        /// <returns>A ValueTask that represents the asynchronous operation. The result contains the item if found, otherwise null.</returns>
        ValueTask<T?> GetItemAsync<T>(string key);

        /// <summary>
        /// Removes an item from local storage.
        /// </summary>
        /// <param name="key">The key of the item to remove.</param>
        /// <returns>A ValueTask that represents the asynchronous operation.</returns>
        ValueTask RemoveItemAsync(string key);
    }
}