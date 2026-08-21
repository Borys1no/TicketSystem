namespace SupportFlow.Application.Queries.Users;

public class GetUserByIdQuery
{
    public Guid Id { get; }

    public GetUserByIdQuery(Guid id)
    {
        Id = id;
    }
}