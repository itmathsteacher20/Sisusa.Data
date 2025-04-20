namespace Sisusa.Data.EFCore.Exceptions
{
    ///<summary>
    /// Represents an exception that is thrown when an entity cannot be found in the data store based on the specified key.
    ///</summary>
    ///<exception cref="System.Collections.Generic.KeyNotFoundException">
    /// The underlying exception that is thrown when the entity is not found.
    ///</exception>
    public sealed class EntityNotFoundException : KeyNotFoundException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="EntityNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public EntityNotFoundException(string message = "") : base(
            string.IsNullOrWhiteSpace(message) ?
                "No entity matching the specified key could be found in the data store." :
                message)
        {
        }
    }
}