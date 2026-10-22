

namespace Domain.Models.Exceptions;

public class SubmissionNotFoundException : NotFoundException
{
    public SubmissionNotFoundException(Guid id) :
        base($"Submission with id '{id}' was not found.", "Submission not found.")
    {

    }
}
