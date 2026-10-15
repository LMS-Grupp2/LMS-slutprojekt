namespace Domain.Models.Exceptions;

/// <summary>
/// Thrown when a document does not exist, or when the user is not allowed to see it.
/// Both cases return 404 so a student cannot find out which documents exist in other courses.
/// </summary>
public class DocumentNotFoundException : NotFoundException
{
    public DocumentNotFoundException(Guid id) :
        base($"Document with id '{id}' was not found.", "Document not found.")
    {

    }
}