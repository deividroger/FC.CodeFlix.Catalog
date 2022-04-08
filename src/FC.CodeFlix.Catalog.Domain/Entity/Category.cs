using FC.CodeFlix.Catalog.Domain.SeedWork;
using FC.CodeFlix.Catalog.Domain.Validation;

namespace FC.CodeFlix.Catalog.Domain.Entity
{
    public class Category : AggregateRoot
    {
        public Category(string name, string description, bool isActive = true) : base()
        {

            Name = name;
            Description = description;

            IsActive = isActive;
            CreatedAt = DateTime.Now;

            Validate();
        }

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Name { get; private set; }

        public string Description { get; private set; }

        public void Update(string name, string? description = null)
        {
            Name = name;
            Description = description ?? Description;

            Validate();
        }

        public void Active()
        {
            IsActive = true;
            Validate();
        }

        public void Deativate()
        {
            IsActive = false;
            Validate();
        }

        private void Validate()
        {
            DomainValidation.NotNullOrEmpty(Name, nameof(Name));
            DomainValidation.MinLength(Name,3,nameof(Name));
            DomainValidation.MaxLength(Name,255,nameof(Name));
            DomainValidation.NotNull(Description, nameof(Description));
            DomainValidation.MaxLength(Description, 10000, nameof(Description));
                
        }
    }
}