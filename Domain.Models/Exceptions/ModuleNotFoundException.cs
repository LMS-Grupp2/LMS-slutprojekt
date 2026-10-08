namespace Domain.Models.Exceptions;

public class ModuleNotFoundException : NotFoundException
{
    public ModuleNotFoundException(Guid id)
        : base($"Module with id '{id}' was not found.", "Module not found")
    {
    }
}