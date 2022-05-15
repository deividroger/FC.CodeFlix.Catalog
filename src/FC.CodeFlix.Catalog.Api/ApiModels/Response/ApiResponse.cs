namespace FC.CodeFlix.Catalog.Api.ApiModels.Response;

public class ApiResponse<TData>
{
    public ApiResponse(TData data)
        => Data = data;

    public TData Data { get; set; }
}
