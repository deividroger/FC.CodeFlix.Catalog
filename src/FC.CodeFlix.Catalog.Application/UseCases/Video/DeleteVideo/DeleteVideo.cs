using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Domain.Repository;
using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.DeleteVideo
{
    public class DeleteVideo : IDeleteVideo
    {
        private readonly IVideoRepository _repository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IStorageService _storageService;

        public DeleteVideo(IVideoRepository videoRepository, IUnitOfWork unitOfWork, IStorageService storageService)
        {
            _repository = videoRepository;
            _unitOfWork = unitOfWork;
            _storageService = storageService;
        }

        public async Task<Unit> Handle(DeleteVideoInput input, CancellationToken cancellationToken)
        {
            var video = await _repository.Get(input.VideoId, cancellationToken);

            var trailerFilePath = video.Trailer?.FilePath;
            var mediaFilePath = video.Media?.FilePath;
            var thumbFilePath = video.Thumb?.Path;
            var thumbHalfFilePath = video.ThumbHalf?.Path;
            var bannerFilePath = video.Banner?.Path;
            

            await _repository.Delete(video, cancellationToken);
            await _unitOfWork.Commit(cancellationToken);

            await ClearVideoMedias(mediaFilePath, trailerFilePath, cancellationToken);
            await ClearImageMedias(bannerFilePath, thumbFilePath, thumbHalfFilePath, cancellationToken);


            return Unit.Value;
        }

        private async Task ClearImageMedias(
            string? bannerFilePath, 
            string? thumbFilePath,
            string? thumbHalfFilePath, 
            CancellationToken cancellationToken)
        {
            if (bannerFilePath is not null)
            {
                await _storageService.Delete(bannerFilePath, cancellationToken);
            }
            if (thumbFilePath is not null)
            {
                await _storageService.Delete(thumbFilePath, cancellationToken);
            }

            if (thumbHalfFilePath is not null)
            {
                await _storageService.Delete(thumbHalfFilePath, cancellationToken);
            }
        }   

        private async Task ClearVideoMedias(
            string? mediaFilePath,
            string? trailerFilePath, 
            CancellationToken cancellationToken)
        {
            if (trailerFilePath is not null)
            {
                await _storageService.Delete(trailerFilePath, cancellationToken);
            }
            if (mediaFilePath is not null)
            {
                await _storageService.Delete(mediaFilePath, cancellationToken);
            }
        }   

    }
}
