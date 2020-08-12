using System;

namespace Grynwald.Extensions.Statiq.DocumentReferences
{
    public class MissingDocumentIdentityException : Exception
    {
        public MissingDocumentIdentityException(string? message) : base(message)
        { }
    }
}
