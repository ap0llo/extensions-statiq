namespace Grynwald.Extensions.Statiq.DocsTemplate.Modules
{
    /// <summary>
    /// Enumerates option how links are resolved by the <see cref="MarkNavbarItemsAsActive"/> module.
    /// </summary>
    /// <seealso cref="ResolveDocumentReferences" />
    public enum LinkMode
    {
        /// <summary>
        /// Use the input documents' source path
        /// </summary>
        Destination = 0,

        /// <summary>
        /// Use the input documents' destination path
        /// </summary>
        Source = 1
    }
}
