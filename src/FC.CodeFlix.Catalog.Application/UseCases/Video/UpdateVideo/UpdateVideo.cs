using FC.CodeFlix.Catalog.Application.Common;
using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Application.Interfaces;
using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using FC.CodeFlix.Catalog.Application.UseCases.Video.CreateVideo;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.Validation;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.UpdateVideo;

public class UpdateVideo : IUpdateVideo
{
    private readonly IVideoRepository _videoRepository;
    private readonly IGenreRepository _genreRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ICastMemberRepository _castMemberRepository;
    private readonly IStorageService _storageService;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateVideo(IVideoRepository videoRepository, 
                       IGenreRepository genreRepository, 
                       ICategoryRepository categoryRepository,
                       ICastMemberRepository castMemberRepository,
                       IStorageService  storageService,
                       IUnitOfWork unitOfWork)
    {
        _videoRepository = videoRepository;
        _genreRepository = genreRepository;
        _categoryRepository = categoryRepository;
        _castMemberRepository = castMemberRepository;
        _storageService = storageService;
        _unitOfWork = unitOfWork;
    }

    public async Task<VideoModelOutput> Handle(UpdateVideoInput input, CancellationToken cancellationToken)
    {
        var video = await _videoRepository.Get(input.VideoId, cancellationToken);

        video.Update(input.Title,
                     input.Description,
                     input.YearLaunched,
                     input.Opened,
                     input.Published,
                     input.Duration,
                     input.Rating);

        var validationHandler = new NotificationValidationHandler();

        video.Validate(validationHandler);

        if(validationHandler.HasErrors())
        {
            throw new EntityValidationException("There are validation errors", 
                                                validationHandler.Errors);
        }

        await ValidateAndAddRelations(input, video, cancellationToken);

        await UploadImagesMedia(input, video, cancellationToken);

        await _videoRepository.Update(video, cancellationToken);

        await _unitOfWork.Commit(cancellationToken);

        return VideoModelOutput
                .FromVideo(video);
    }


    private async Task UploadImagesMedia(UpdateVideoInput input, DomainEntity.Video video, CancellationToken cancellationToken)
    {
        if (input.Banner is not null)
        {
            var fileName = StorageFileName.Create(video.Id, nameof(video.Banner), input.Banner.Extension);

            var bannerUrl = await _storageService.Upload(
                   fileName,
                   input.Banner.FileStream,
                   input.Banner.ContentType,
                   cancellationToken);

            video.UpdateBanner(bannerUrl);
        }

        if (input.Thumb is not null)
        {
            var fileName = StorageFileName.Create(video.Id, nameof(video.Thumb), input.Thumb.Extension);

            var thumbUrl = await _storageService.Upload(
                   fileName,
                   input.Thumb.FileStream,
                   input.Thumb.ContentType,
                   cancellationToken);

            video.UpdateThumb(thumbUrl);
        }


        if (input.ThumbHalf is not null)
        {
            var fileName = StorageFileName.Create(video.Id, nameof(video.ThumbHalf), input.ThumbHalf.Extension);

            var thumbHalfUrl = await _storageService.Upload(
                   fileName,
                   input.ThumbHalf.FileStream,
                   input.ThumbHalf.ContentType,
                   cancellationToken);

            video.UpdateThumbHalf(thumbHalfUrl);
        }
    }

    private async Task ValidateAndAddRelations(UpdateVideoInput input, DomainEntity.Video video, CancellationToken cancellationToken)
    {
        if (input.GenresIds is not null)
        {
            video.RemoveAllGenre();

            if (input.GenresIds.Count > 0) { 
                await ValidateGenresIds(input, cancellationToken);
                input.GenresIds!.ToList().ForEach(video.AddGenre);
            }
        }

        if (input.CategoriesIds is not null)
        {

            video.RemoveAllCategories();

            if (input.CategoriesIds.Count > 0) {
                await ValidateCategoriesIds(input, cancellationToken);
                input.CategoriesIds!.ToList().ForEach(video.AddCategory);
            }

        }

        if ( input.CastMembersIds is not null)
        {
            video.RemoveAllCastMembers();

            if(input.CastMembersIds.Count > 0)
            {
                await ValidateCastMembersIds(input, cancellationToken);
                input.CastMembersIds!.ToList().ForEach(video.AddCastMember);
            }

        }
    }

    private async Task ValidateCastMembersIds(UpdateVideoInput input, CancellationToken cancellationToken)
    {
        var persistenceIds = await _castMemberRepository
            .GetIdsListByIds(input.CastMembersIds!.ToList(), cancellationToken);

        if (persistenceIds.Count < input.CastMembersIds!.Count)
        {
            var notFoundIds = input.CastMembersIds!.ToList()
                .FindAll(castMemberId => !persistenceIds.Contains(castMemberId));

            throw new RelatedAggregateException(
                $"Related castMember id (or ids) not found: {string.Join(',', notFoundIds)}.");
        }
    }

    private async Task ValidateCategoriesIds(UpdateVideoInput input, CancellationToken cancellationToken)
    {
        var persistenceIds = await _categoryRepository
            .GetIdsListByIds(input.CategoriesIds!.ToList(), cancellationToken);

        if (persistenceIds.Count < input.CategoriesIds!.Count)
        {
            var notFoundIds = input.CategoriesIds!.ToList()
                .FindAll(categoryId => !persistenceIds.Contains(categoryId));

            throw new RelatedAggregateException(
                $"Related category id (or ids) not found: {string.Join(',', notFoundIds)}.");
        }
    }

    private async Task ValidateGenresIds(UpdateVideoInput input, CancellationToken cancellationToken)
    {
        var persistenceIds = await _genreRepository
            .GetIdsListByIds(input.GenresIds!.ToList(), cancellationToken);

        if (persistenceIds.Count < input.GenresIds!.Count)
        {

            var notFoundIds = input.GenresIds!.ToList()
                .FindAll(genreId => !persistenceIds.Contains(genreId));

            throw new RelatedAggregateException(
                $"Related genre id (or ids) not found: {string.Join(',', notFoundIds)}.");
        }
    }

}
