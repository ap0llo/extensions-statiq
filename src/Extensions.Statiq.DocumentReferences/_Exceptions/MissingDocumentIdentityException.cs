using System;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    public sealed class MissingDocumentIdentityException : Exception
    {
        public MissingDocumentIdentityException(string? message) : base(message)
        { }
    }
}
