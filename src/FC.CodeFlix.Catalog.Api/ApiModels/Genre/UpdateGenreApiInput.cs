namespace FC.CodeFlix.Catalog.Api.ApiModels.Genre
{
    public class UpdateGenreApiInput
    {
        public UpdateGenreApiInput(string name, bool? isActive = null, List<Guid>? categoriesId = null)
        {
            Name = name;
            IsActive = isActive;

            CategoriesId = categoriesId;
        }

        public string Name { get; set; }

        public bool? IsActive { get; set; }

        public List<Guid>? CategoriesId { get; set; }

    }
}
