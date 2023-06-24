using FC.CodeFlix.Catalog.Domain.Entity;
using FC.CodeFlix.Catalog.Domain.Validation;

namespace FC.CodeFlix.Catalog.Domain.Validator;

public class VideoValidator : Validation.Validator
{
    private readonly Video _video;
    private const int _titleMaxLength = 255;
    private const int _descriptionMaxLength = 4000;

    public VideoValidator(Video video, ValidationHandler handler) : base(handler)
        => _video = video;

    public override void Validate()
    {
        ValidateTitle();
        ValidateDescription();
    }

    private void ValidateDescription()
    {
        if (string.IsNullOrWhiteSpace(_video.Description))
        {
            _handler.HandleError($"'{nameof(_video.Description)}' is required");
        }

        if (_video.Description.Length > _descriptionMaxLength)
        {
            _handler.HandleError($"'{nameof(_video.Description)}' should be less or equal {_descriptionMaxLength} characters long");
        }
    }

    private void ValidateTitle()
    {
        if (string.IsNullOrWhiteSpace(_video.Title))
        {
            _handler.HandleError($"'{nameof(_video.Title)}' is required");
        }

        if (_video.Title.Length > _titleMaxLength)
        {
            _handler.HandleError($"'{nameof(_video.Title)}' should be less or equal {_titleMaxLength} characters long");
        }
    }
}