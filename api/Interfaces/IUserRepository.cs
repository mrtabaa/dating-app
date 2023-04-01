namespace api.Interfaces;
public interface IUserRepository
{
    public Task<MemberDto?> GetUserById(string userId);
    public Task<MemberDto?> GetUserByEmail(string email);
    public Task<UpdateResult?> UpdateUser(string userId, UserRegisterDto userIn);
    public Task<DeleteResult> DeleteUser(string userId);
}