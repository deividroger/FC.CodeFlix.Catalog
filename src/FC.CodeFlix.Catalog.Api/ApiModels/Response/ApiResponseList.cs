using FC.CodeFlix.Catalog.Application.Common;

namespace FC.CodeFlix.Catalog.Api.ApiModels.Response
{
    public class ApiResponseList<TItemData> : ApiResponse<IReadOnlyList<TItemData>>
    {
        public ApiResponseList(PaginatedListOutput<TItemData> paginatedListOutput) 
            : base(paginatedListOutput.Items)
        {
            Meta = new(paginatedListOutput.Page, paginatedListOutput.PerPage, paginatedListOutput.Total);
        }

        public ApiResponseListMeta Meta { get; private set; }
    }
}