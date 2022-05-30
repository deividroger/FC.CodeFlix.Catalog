using MediatR;

namespace FC.CodeFlix.Catalog.Application.UseCases.Genre.DeleteGenre
{
    public class DeleteGenreInput: IRequest
    {
        public DeleteGenreInput(Guid id) => Id = id;
        
        public Guid Id { get; set; }
    }
}
