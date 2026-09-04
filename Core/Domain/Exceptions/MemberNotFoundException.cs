namespace Core.Domain.Exceptions;

public sealed class MemberNotFoundException(int id) : NotFoundException($"Le membre avec l'ID {id} est introuvable.")
{

}
