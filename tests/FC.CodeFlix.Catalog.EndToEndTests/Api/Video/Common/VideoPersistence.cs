using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Infra.Data.EF;
using FC.CodeFlix.Catalog.Infra.Data.EF.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DomainEntity = FC.CodeFlix.Catalog.Domain.Entity;

namespace FC.CodeFlix.Catalog.EndToEndTests.Api.Video.Common;

public class VideoPersistence
{
    private readonly CodeFlixCatalogDbContext _context;

    public VideoPersistence(CodeFlixCatalogDbContext dbContext) 
        => _context = dbContext;

    public async Task<DomainEntity.Video?> GetById(Guid guid)
        => await _context
                    .Videos
        .AsNoTracking()
        .Where(video=> video.Id == guid).FirstOrDefaultAsync();

    public async Task< List<VideosCastMembers>? > GetVideosCastMembers(Guid id)
        => await _context.VideosCastMembers.AsNoTracking()
        .Where(videoCastMember => videoCastMember.VideoId == id).ToListAsync();

    public async Task<List<VideosGenres>?> GetVideosGenres(Guid id)
        => await _context.VideosGenres.AsNoTracking()
        .Where(videogenre => videogenre.VideoId == id).ToListAsync();

    public async Task<List<VideosCategories>?> GetVideosCategories(Guid id)
        => await _context.VideosCategories.AsNoTracking()
        .Where(videocategory => videocategory.VideoId == id).ToListAsync();

    public async Task<int> GetMediaCount()
        => await _context.Set<Media>().CountAsync();

    public async Task InsertList(List<DomainEntity.Video> videos)
    {
        await _context.Videos.AddRangeAsync(videos);

        foreach (var video in videos)
        {
            var videosCategories = video.Categories?
                .Select(category
                => new VideosCategories(category, video.Id)).ToList();

            if(videosCategories != null && videosCategories.Any())
                await _context.VideosCategories.AddRangeAsync(videosCategories);

            var videosGenres = video.Genres?
                .Select(genre => new VideosGenres(genre, video.Id)).ToList();

            if(videosGenres != null && videosGenres.Any())
                await _context.VideosGenres.AddRangeAsync(videosGenres);

            var videosCastMembers = video.CastMembers?
                .Select(castMember => new VideosCastMembers(castMember, video.Id)).ToList();

            if(videosCastMembers != null && videosCastMembers.Any())
                await _context.VideosCastMembers.AddRangeAsync(videosCastMembers);

        }

        await _context.SaveChangesAsync();
    }

}
