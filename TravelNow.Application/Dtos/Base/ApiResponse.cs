namespace TravelNow.Application.Dtos.Base;

public class ApiResponse<T>
{
    private ApiResponse(bool succeeded, T result, IEnumerable<string> errors)
    {
        Succeeded = succeeded;
        Result = result;
        Errors = errors;
    }

    public bool Succeeded { get; set; }

    public T Result { get; set; }

    public IEnumerable<string> Errors { get; set; }

    public static ApiResponse<T> Success(T result)
    {
        return new ApiResponse<T>(true, result, new List<string>());
    }

    public static ApiResponse<T> Failure(IEnumerable<string> errors)
    {
        return new ApiResponse<T>(false, default!, errors);
    }
}
