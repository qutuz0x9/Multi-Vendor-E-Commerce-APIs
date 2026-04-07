using MultiVendorECommerce.Shared.Enums;

namespace MultiVendorECommerce.Shared.Helpers;

public class Error
{

    public string ErrorMessage { get; }
    public ErrorType Type { get; }

    private Error(string errorMessage, ErrorType type)
    {
        ErrorMessage = errorMessage;
        Type = type;
    }

    public static Error Failure(string errorMessage = "There was an error processing your request.") =>
        new(errorMessage, ErrorType.Failure);
    public static Error Validation(string errorMessage = "One or more validation errors occurred.") =>
        new(errorMessage, ErrorType.Validation);
    public static Error NotFound(string errorMessage = "The requested resource was not found.") =>
        new(errorMessage, ErrorType.NotFound);
    public static Error Unauthorized(string errorMessage = "You are not authorized to perform this action.") =>
        new(errorMessage, ErrorType.Unauthorized);
    public static Error Forbidden(string errorMessage = "You do not have permission to access this resource.") =>
        new(errorMessage, ErrorType.Forbidden);
    public static Error InvalidCredentials(string errorMessage = "The provided credentials are invalid.") =>
        new(errorMessage, ErrorType.InvalidCredentials);

}
