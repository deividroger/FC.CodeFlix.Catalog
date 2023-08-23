using FC.CodeFlix.Catalog.Api.Extensions;
using FC.CodeFlix.Catalog.Application.UseCases.Video.UploadMedias;
using Microsoft.AspNetCore.Mvc;

namespace FC.CodeFlix.Catalog.Api.ApiModels.Video;

public class UploadMediaApiInput
{
    private static class MediaType { 
        public const string Banner = "banner";
        public const string Thumb = "thumbnail";
        public const string ThumbHalf = "thumbnail_half";
        public const string Media = "video";
        public const string Trailer = "trailer";
    }

    [FromForm(Name = "media_file")]
    public IFormFile? Media { get; set; }

    public UploadMediasInput ToUploadMediasInput(Guid id, string type)
    => type?.ToLower() switch
    {
        MediaType.Banner => new UploadMediasInput(id,BannerFile: Media.ToFileInput()),    
        MediaType.Thumb => new UploadMediasInput(id,ThumbFile: Media.ToFileInput()),
        MediaType.ThumbHalf => new UploadMediasInput(id,ThumbHalfFile: Media.ToFileInput()),
        MediaType.Media => new UploadMediasInput(id,VideoFile: Media.ToFileInput()),
        MediaType.Trailer => new UploadMediasInput(id,TrailerFile: Media.ToFileInput()),
            _=> throw new ArgumentException($"'{type}' is not a valid media type.")

        };

}
