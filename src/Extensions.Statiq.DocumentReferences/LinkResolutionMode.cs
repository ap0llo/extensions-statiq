namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    /// <summary>
    /// Enumerates option how links are resolved by the <see cref="ResolveDocumentReferences"/> module.
    /// </summary>
    /// <seealso cref="ResolveDocumentReferences" />
    public enum LinkResolutionMode
    {
        /// <summary>
        /// Generate links between the documents' Destination paths.
        /// </summary>
        Destination = 0,

        /// <summary>
        /// Generate links between the documents' Source paths.
        /// </summary>
        Source = 1
    }
}
