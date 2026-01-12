using System.Threading.Tasks;

namespace IntelliVerseX.[Module]
{
    /// <summary>
    /// Defines the contract for [feature description] providers.
    /// </summary>
    /// <remarks>
    /// Implement this interface to create custom [feature] providers.
    /// Register implementations with the appropriate manager.
    /// </remarks>
    /// <example>
    /// <code>
    /// public class MyCustomProvider : IIVX[Feature]Provider
    /// {
    ///     public string ProviderId => "my-custom-provider";
    ///     public bool IsAvailable => true;
    ///     
    ///     public async Task&lt;IVX[Feature]Result&gt; ExecuteAsync()
    ///     {
    ///         // Custom implementation
    ///         return IVX[Feature]Result.Success();
    ///     }
    /// }
    /// </code>
    /// </example>
    public interface IIVX[Feature]Provider
    {
        /// <summary>
        /// Gets the unique identifier for this provider.
        /// </summary>
        /// <value>A string that uniquely identifies this provider.</value>
        string ProviderId { get; }

        /// <summary>
        /// Gets whether this provider is available on the current platform.
        /// </summary>
        /// <value>True if the provider can be used, false otherwise.</value>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets the display name for this provider.
        /// </summary>
        /// <value>A human-readable name for UI display.</value>
        string DisplayName { get; }

        /// <summary>
        /// Executes the provider's main operation.
        /// </summary>
        /// <returns>The result of the operation.</returns>
        /// <exception cref="IVX[Feature]Exception">Thrown if the operation fails.</exception>
        Task<IVX[Feature]Result> ExecuteAsync();

        /// <summary>
        /// Cancels any ongoing operation.
        /// </summary>
        void Cancel();

        /// <summary>
        /// Releases resources used by the provider.
        /// </summary>
        void Dispose();
    }
}
