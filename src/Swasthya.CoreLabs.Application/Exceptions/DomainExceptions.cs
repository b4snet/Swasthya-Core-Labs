namespace Swasthya.CoreLabs.Application.Exceptions;

public sealed class NotFoundException : Exception
{
    public NotFoundException(string message)
        : base(message)
    {
    }
}

public sealed class PermissionDeniedException : Exception
{
    public PermissionDeniedException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// Input failed validation. Mapped to HTTP 400 with ProblemDetails.
/// </summary>
public sealed class ValidationException : Exception
{
    public ValidationException(string message)
        : base(message)
    {
    }
}

/// <summary>
/// The request conflicts with current state (duplicate, illegal state
/// transition, or concurrent modification). Mapped to HTTP 409.
/// </summary>
public sealed class ConflictException : Exception
{
    public ConflictException(string message)
        : base(message)
    {
    }
}
