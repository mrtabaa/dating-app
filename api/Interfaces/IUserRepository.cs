namespace api.Interfaces;
public interface IUserRepository
{
    public Task<List<MemberDto?>> GetUsers(CancellationToken cancellationToken);
    public Task<MemberDto?> GetUserById(string userId, CancellationToken cancellationToken);
    public Task<MemberDto?> GetUserByEmail(string email, CancellationToken cancellationToken);
    public Task<UpdateResult?> UpdateUser(MemberUpdateDto memberUpdateDto, string Id, CancellationToken cancellationToken);
    public Task<DeleteResult?> DeleteUser(string userId, CancellationToken cancellationToken);
}