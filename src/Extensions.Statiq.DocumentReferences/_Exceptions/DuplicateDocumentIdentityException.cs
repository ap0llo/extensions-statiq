using System;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    public class DuplicateDocumentIdentityException : Exception
    {
        public DuplicateDocumentIdentityException(string? message) : base(message)
        { }
    }
}
