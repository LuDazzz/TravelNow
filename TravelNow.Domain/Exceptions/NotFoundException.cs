namespace TravelNow.Domain.Exceptions;

/// <summary>
/// Represents an exception that is thrown when a requested resource or entity is not found.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    public NotFoundException(string message) : base(message) { }
    public NotFoundException(Type type) : base($"{type} is missing") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a default error message.
    /// </summary>
    public NotFoundException() : base("The requested resource was not found.") { }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class with a specified error message 
    /// and a reference to the inner exception that is the cause of this exception.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public NotFoundException(string message, Exception innerException) : base(message, innerException) { }
}
