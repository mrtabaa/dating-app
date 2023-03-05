namespace api.Interfaces;
public interface IUserRepository
{
    public Task<UserDto?> GetUser(string UserId);
    public Task<UpdateResult?> UpdateUser(string userId, UserRegisterDto userIn);
    public Task<DeleteResult> DeleteUser(string userId);
}