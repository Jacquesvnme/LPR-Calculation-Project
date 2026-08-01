namespace LprSolver.Models;

/// <summary>
/// Represents a general response with a message and success status.
/// This is used throughout the system as a means of propper error handling.
/// </summary>
public class GeneralResponse
{
    public string Message { get; set; } = string.Empty;
    public bool IsSuccess { get; set; } = false;

    public GeneralResponse(string message, bool isSuccess)
    {
        Message = message;
        IsSuccess = isSuccess;
    }
}
