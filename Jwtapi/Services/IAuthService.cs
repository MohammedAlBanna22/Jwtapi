using System.Threading.Tasks;
using Jwtapi.Models;
using Jwtapi.Models.DTO;
namespace Jwtapi.Services
{
    public interface IAuthService
    {
        Task<AuthModel> RegisterAsync(RegisterModel model);
        Task<AuthModel> GetTokenAsync(TokenRequestModel model);
        Task<string> AddRoleAsync(AddRoleModel model);
     


    }
}
