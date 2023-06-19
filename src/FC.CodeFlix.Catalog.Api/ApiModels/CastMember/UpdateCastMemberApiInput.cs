using FC.CodeFlix.Catalog.Domain.Enum;

namespace FC.CodeFlix.Catalog.Api.ApiModels.CastMember
{
    public class UpdateCastMemberApiInput
    {
        public string Name { get; set; }

        public CastMemberType Type { get; set; }

        public UpdateCastMemberApiInput( string name, CastMemberType type)
        {
            Name = name;
            Type = type;
        }
    }
}
