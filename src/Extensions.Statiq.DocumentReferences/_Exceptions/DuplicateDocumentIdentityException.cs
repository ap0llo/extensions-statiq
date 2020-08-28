using System;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    public sealed class DuplicateDocumentIdentityException : Exception
    {
        public DuplicateDocumentIdentityException(string? message) : base(message)
        { }
    }
}
