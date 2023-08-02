using FC.CodeFlix.Catalog.Domain.Enum;
using FC.CodeFlix.Catalog.Domain.Exceptions;
using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Domain.Validation;
using FC.CodeFlix.Catalog.Domain.Validator;
using FC.CodeFlix.Catalog.Domain.ValueObject;

namespace FC.CodeFlix.Catalog.Domain.Entity;

public class Video : AggregateRoot
{


    public string Title { get; private set; }
    public string Description { get; private set; }
    public int YearLaunched { get; private set; }
    public bool Opened { get; private set; }
    public bool Published { get; private set; }
    public int Duration { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public Rating Rating { get; private set; }

    public Image? Thumb { get; private set; }
    public Image? ThumbHalf { get; private set; }
    public Image? Banner { get; private set; }

    public Media? Media { get; private set; }

    public Media? Trailer { get; private set; }


    private List<Guid> _categories;

    public IReadOnlyList<Guid> Categories
        => _categories;

    private List<Guid> _genres;

    public IReadOnlyList<Guid> Genres
        => _genres;

    private List<Guid> _castMembers;

    public IReadOnlyList<Guid> CastMembers
        => _castMembers;

    public Video(string title, string description, int yearLaunched, bool opened, bool published, int duration, Rating rating)
    {
        Title = title;
        Description = description;
        YearLaunched = yearLaunched;
        Opened = opened;
        Published = published;
        Duration = duration;
        Rating = rating;
        CreatedAt = DateTime.Now;
        _categories = new();
        _genres = new();
        _castMembers = new();   
    }

    public void Validate(ValidationHandler handler)
        => new VideoValidator(this, handler).Validate();

    public void Update(string title, 
                       string description,
                       int yearLaunched,
                       bool opened,
                       bool published,
                       int duration,
                       Rating? rating = null)
    {
        Title = title;
        Description = description;
        YearLaunched = yearLaunched;
        Opened = opened;
        Published = published;
        Duration = duration;
        Rating = rating ?? Rating;

    }

    public void UpdateThumb(string path)
        => Thumb = new Image(path);

    public void UpdateThumbHalf(string path)
        => ThumbHalf = new Image(path);

    public void UpdateBanner(string path)
        => Banner = new Image(path);

    public void UpdateMedia(string validPath)
        => Media = new Media(validPath);

    public void UpdateTrailer(string validPath)
        => Trailer = new Media(validPath);

    public void UpdateAsSentToEncode()
    {
        if (Media is null)
            throw new EntityValidationException("There is no media");
        Media!.UpdateAsSentToEncode();
    }

    public void UpdateAsEncoded(string validEncodedPath)
    {
        if (Media is null)
            throw new EntityValidationException("There is no media");
        Media!.UpdateAsEncoded(validEncodedPath);
    }

    public void AddCategory(Guid categoryId)
        => _categories.Add(categoryId);

    public void RemoveCategory(Guid categoryId)
        => _categories.Remove(categoryId);

    public void RemoveAllCategories()
        => _categories.Clear();

    public void AddGenre(Guid genreId)
        => _genres.Add(genreId);

    public void RemoveGenre(Guid genreId)
        => _genres.Remove(genreId);

    public void RemoveAllGenre()
       => _genres.Clear();

    public void AddCastMember(Guid castMemberId)
        => _castMembers.Add(castMemberId);

    public void RemoveCastMember(Guid castMemberId)
        => _castMembers.Remove(castMemberId);

    public void RemoveAllCastMembers()
        => _castMembers.Clear();
}
