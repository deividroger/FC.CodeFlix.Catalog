using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;

public interface ICreateVideo: IRequestHandler<CreateVideoInput, CreateVideoOutput>
{
}
