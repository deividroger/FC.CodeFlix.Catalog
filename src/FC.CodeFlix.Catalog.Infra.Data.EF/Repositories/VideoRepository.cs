using FC.CodeFlix.Catalog.Application.Exceptions;
using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Repository;
using FC.CodeFlix.Catalog.Domain.SeedWork.SearchableRepository;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;

namespace FC.CodeFlix.Catalog.Infra.Data.EF.Repositories;

public class VideoRepository : IVideoRepository
{
    private readonly CodeFlixCatalogDbContext _context;
    private DbSet<Video> _videos => _context.Videos;
    private DbSet<VideosCategories> _videoCategories => _context.VideosCategories;

    private DbSet<VideosGenres> _videoGenres => _context.VideosGenres;

    private DbSet<VideosCastMembers> _videoCastMembers => _context.VideosCastMembers;

    private DbSet<Media> _medias => _context.Set<Media>();

    public VideoRepository(CodeFlixCatalogDbContext context)
         => _context = context;


    public async Task Insert(Video video, CancellationToken cancellationToken)
    {
        await _context.Videos.AddAsync(video, cancellationToken);

        await AddRelations(video, cancellationToken);
    }

    public async Task Update(Video video, CancellationToken cancellationToken)
    {
        _context.Videos.Update(video);

        RemoveRelations(video);

        await AddRelations(video, cancellationToken);
        
        DeleteOrphanMedias(video);
    }

    private void DeleteOrphanMedias(Video video)
    {
        if (_context.Entry(video).Reference(v=> v.Trailer).IsModified)
        {
            var oldTrailerId = _context.Entry(video).OriginalValues.GetValue<Guid?>($"{nameof(video.Trailer)}Id");

            if (oldTrailerId != null && oldTrailerId != video.Trailer?.Id)
            {
                var oldTrailer = _medias.Find(oldTrailerId);
                _medias.Remove(oldTrailer!);
            }
        }

        if (_context.Entry(video).Reference(v => v.Media).IsModified)
        {
            var oldMediaId = _context.Entry(video).OriginalValues.GetValue<Guid?>($"{nameof(video.Media)}Id");

            if (oldMediaId != null && oldMediaId != video.Media?.Id)
            {
                var oldMedia = _medias.Find(oldMediaId);
                _medias.Remove(oldMedia!);
            }
        }
    }

    public Task Delete(Video video, CancellationToken cancellationToken)
    {
        _videos.Remove(video);

        if (video.Trailer is not null)
            _medias.Remove(video.Trailer);

        if (video.Media is not null)
            _medias.Remove(video.Media);

        RemoveRelations(video);

        return Task.CompletedTask;
    }

    public async Task<Video> Get(Guid id, CancellationToken cancellationToken)
    {
        var video = await _videos.FirstOrDefaultAsync(videoId => videoId.Id == id, cancellationToken: cancellationToken);

        NotFoundException.ThrowIfNull(video, $"Video '{id}' not found.");

        await ObtainRelations(video, cancellationToken);

        return video!;
    }

    public async Task<SearchOutput<Video>> Search(SearchInput input, CancellationToken cancellationToken)
    {
        var toSkip = (input.Page - 1) * input.PerPage;

        var query = _videos.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(input.Search))
        {
            query = query.Where(video => video.Title.Contains(input.Search));
        }

        query = InsertOrderBy(input, query);
        var count = query.Count();

        var items = await query.Skip(toSkip)
                          .Take(input.PerPage)
                          .ToListAsync(cancellationToken);

        var videosIds = items.Select(video => video.Id).ToList();

        await AddCategoriesToVideos(items, videosIds, cancellationToken);
        await AddGenresToVideos(items, videosIds, cancellationToken);
        await AddCastMembersToVideos(items, videosIds, cancellationToken);


        return new(input.Page, input.PerPage, count, items);

    }


    private async Task AddCastMembersToVideos(List<Video> items, List<Guid> videosIds, CancellationToken cancellationToken)
    {
        var genresRelations = await _videoCastMembers
                                            .Where(vc => videosIds.Contains(vc.VideoId))
                                            .ToListAsync(cancellationToken);
        var relationsWithCastMembersByVideoId =
            genresRelations.GroupBy(x => x.VideoId).ToList();

        relationsWithCastMembersByVideoId.ForEach(relationGroup =>
        {
            var video = items.Find(video => video.Id == relationGroup.Key);
            if (video is null) return;
            relationGroup.ToList().ForEach(relation => video.AddCastMember(relation.CastMemberId));

        });
    }


    private async Task AddGenresToVideos(List<Video> items, List<Guid> videosIds, CancellationToken cancellationToken)
    {
        var genresRelations = await _videoGenres
                                            .Where(vc => videosIds.Contains(vc.VideoId))
                                            .ToListAsync(cancellationToken);
        var relationsWithGenresByVideoId =
            genresRelations.GroupBy(x => x.VideoId).ToList();

        relationsWithGenresByVideoId.ForEach(relationGroup =>
        {
            var video = items.Find(video => video.Id == relationGroup.Key);
            if (video is null) return;
            relationGroup.ToList().ForEach(relation => video.AddGenre(relation.GenreId));

        });
    }

    private async Task AddCategoriesToVideos(List<Video> items, List<Guid> videosIds, CancellationToken cancellationToken)
    {
        var categoriesRelations = await _videoCategories
                                            .Where(vc => videosIds.Contains(vc.VideoId))
                                            .ToListAsync(cancellationToken);
        var relationsWithCategoriesByVideoId =
            categoriesRelations.GroupBy(x => x.VideoId).ToList();

        relationsWithCategoriesByVideoId.ForEach(relationGroup =>
        {
            var video = items.Find(video => video.Id == relationGroup.Key);
            if (video is null) return;
            relationGroup.ToList().ForEach(relation => video.AddCategory(relation.CategoryId));

        });
    }

    private async Task ObtainRelations(Video? video, CancellationToken cancellationToken)
    {
        var categoriesIds = await _videoCategories
                                        .Where(vc => vc.VideoId == video!.Id)
                                        .Select(vc => vc.CategoryId)
                                        .ToListAsync(cancellationToken);

        categoriesIds.ForEach(video!.AddCategory);

        var genresIds = await _videoGenres
                                .Where(vg => vg.VideoId == video.Id)
                                .Select(vg => vg.GenreId)
                                .ToListAsync(cancellationToken);
        genresIds.ForEach(video!.AddGenre);

        var castMembersIds = await _videoCastMembers
                                .Where(vcm => vcm.VideoId == video.Id)
                                .Select(vcm => vcm.CastMemberId)
                                .ToListAsync(cancellationToken);
        castMembersIds.ForEach(video!.AddCastMember);
    }

    private static IQueryable<Video> InsertOrderBy(SearchInput input, IQueryable<Video> query)
    {
        query = input switch
        {
            { OrderBy: "title", Order: SearchOrder.ASC } =>
                query.OrderBy(video => video.Title).ThenBy(x => x.Id),
            { OrderBy: "title", Order: SearchOrder.DESC } =>
                query.OrderByDescending(video => video.Title).ThenByDescending(x => x.Id),

            { OrderBy: "id", Order: SearchOrder.ASC } =>
            query.OrderBy(video => video.Id),
            { OrderBy: "id", Order: SearchOrder.DESC } =>
                query.OrderByDescending(video => video.Id),

            { OrderBy: "createdAt", Order: SearchOrder.ASC } =>
                query.OrderBy(video => video.CreatedAt),
            { OrderBy: "createdAt", Order: SearchOrder.DESC } =>
                query.OrderByDescending(video => video.CreatedAt),
            _ => query.OrderBy(video => video.Title).ThenBy(x => x.Id),
        };
        return query;
    }

    private void RemoveRelations(Video video)
    {
        _videoCategories.RemoveRange(_videoCategories.Where(vc => vc.VideoId == video.Id));
        _videoGenres.RemoveRange(_videoGenres.Where(vg => vg.VideoId == video.Id));
        _videoCastMembers.RemoveRange(_videoCastMembers.Where(vcm => vcm.VideoId == video.Id));
    }

    private async Task AddRelations(Video video, CancellationToken cancellationToken)
    {
        if (video.Categories.Count > 0)
        {
            var relations = video.Categories
                                .Select(categoryId => new VideosCategories(categoryId, video.Id));

            await _videoCategories.AddRangeAsync(relations, cancellationToken);
        }

        if (video.Genres.Count > 0)
        {
            var relations = video.Genres
                                .Select(genreId => new VideosGenres(genreId, video.Id));

            await _videoGenres.AddRangeAsync(relations, cancellationToken);
        }

        if (video.CastMembers.Count > 0)
        {
            var relations = video.CastMembers
                                .Select(castMemberId => new VideosCastMembers(castMemberId, video.Id));

            await _videoCastMembers.AddRangeAsync(relations, cancellationToken);
        }
    }
}
