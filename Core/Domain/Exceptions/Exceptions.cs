namespace Core.Domain.Exceptions;

public abstract class BusinessException(string message) : Exception(message);

public class NotFoundException(string message) : BusinessException(message);

public class ConflictException(string message) : BusinessException(message);