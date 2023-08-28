using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Repository;
using Microsoft.Extensions.Logging;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateMediaStatus;

public class UpdateMediaStatus : IUpdateMediaStatus
{
    private readonly IVideoRepository _videoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateMediaStatus> _logger;

    public UpdateMediaStatus(IVideoRepository videoRepository, IUnitOfWork unitOfWork, ILogger<UpdateMediaStatus> logger)
    {
        _videoRepository = videoRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VideoModelOutput> Handle(UpdateMediaStatusInput request, CancellationToken cancellationToken)
    {
        var video = await _videoRepository.Get(request.VideoId, cancellationToken);

        switch (request.Status)
        {
            case Domain.Enum.MediaStatus.Completed:
                video.UpdateAsEncoded(request.EncodedPath!);
                break;
            case Domain.Enum.MediaStatus.Error:
                _logger.LogError("There was an error encoding the video {videoId}: {error}", 
                    video.Id, request.ErrorMessage);
                video.UpdateAsEncodingError();
                break;
            default:
                throw new EntityValidationException("Invalid media status");
        }

        await _videoRepository.Update(video, cancellationToken);
        await _unitOfWork.Commit(cancellationToken);


        return VideoModelOutput.FromVideo(video);
    }
}
