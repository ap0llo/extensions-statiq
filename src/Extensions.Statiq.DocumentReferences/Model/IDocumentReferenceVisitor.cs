namespace Grynwald.Extensions.Statiq.DocumentReferences.Model
{
    public interface IDocumentReferenceVisitor
    {
        void Visit(FullyQualifiedDocumentReference reference);

        void Visit(SameNameDocumentReference reference);

        void Visit(SelfDocumentReference reference);

        void Visit(SameVersionDocumentReference reference);
    }

    public interface IDocumentReferenceVisitor<TResult>
    {
        TResult Visit(FullyQualifiedDocumentReference reference);

        TResult Visit(SameNameDocumentReference reference);

        TResult Visit(SelfDocumentReference reference);

        TResult Visit(SameVersionDocumentReference reference);
    }

    public interface IDocumentReferenceVisitor<TResult, TParameter>
    {
        TResult Visit(FullyQualifiedDocumentReference reference, TParameter parameter);

        TResult Visit(SameNameDocumentReference reference, TParameter parameter);

        TResult Visit(SelfDocumentReference reference, TParameter parameter);

        TResult Visit(SameVersionDocumentReference reference, TParameter parameter);
    }
}


