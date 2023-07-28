using FC.CodeFlix.Catalog.Application.UseCases.Video.Common;
using DomainEntities = FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;

namespace FC.CodeFlix.Catalog.Application.UseCases.Video.ListVideos
{
    public class ListVideos : IListVideos
    {
        private readonly IVideoRepository _videoRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IGenreRepository _genreRepository;
        private readonly ICastMemberRepository _castMemberRepository;

        public ListVideos(IVideoRepository videoRepository,
                          ICategoryRepository categoryRepository,
                          IGenreRepository genreRepository,
                          ICastMemberRepository castMemberRepository)
        {
            _videoRepository = videoRepository;
            _categoryRepository = categoryRepository;
            _genreRepository = genreRepository;
            _castMemberRepository = castMemberRepository;
        }

        public async Task<ListVideosOutput> Handle(ListVideosInput input, CancellationToken cancellationToken)
        {

            var searchResult = await _videoRepository.Search(input.ToSearchInput(), cancellationToken);

            var relatedCategoriesIds = searchResult.Items
                    .SelectMany(video => video.Categories
                                .Select(categories => categories))
                    .Distinct().ToList();

            var relatedGenreIds = searchResult.Items
                    .SelectMany(video => video.Genres
                                .Select(genre => genre))
                    .Distinct().ToList();

            var relatedCastMembersIds = searchResult.Items
                    .SelectMany(video => video.CastMembers
                                .Select(castMember => castMember))
                    .Distinct().ToList();

            IReadOnlyList<DomainEntities.Category>? categories = null;
            IReadOnlyList<DomainEntities.Genre>? genres = null;
            IReadOnlyList<DomainEntities.CastMember>? castMembers = null;

            if (relatedCategoriesIds is not null && relatedCategoriesIds.Count > 0)
            {
                categories = await _categoryRepository.GetListByIds(relatedCategoriesIds, cancellationToken);
            }

            if (relatedGenreIds is not null && relatedGenreIds.Count > 0)
            {
                genres = await _genreRepository.GetListByIds(relatedGenreIds, cancellationToken);
            }

            if (relatedCastMembersIds is not null && relatedCastMembersIds.Count > 0)
            {
                castMembers = await _castMemberRepository.GetListByIds(relatedCastMembersIds, cancellationToken);
            }

            var output = new ListVideosOutput(searchResult.CurrentPage,
                                               searchResult.PerPage,
                                               searchResult.Total,
                                                searchResult
                                                        .Items
                                                        .Select(video => VideoModelOutput
                                                        .FromVideo(video, categories, genres, castMembers))
                                                        .ToList());
            return output;
        }
    }
}
