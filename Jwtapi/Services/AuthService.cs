using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Jwtapi.Helpers;
using Jwtapi.Models;
using Jwtapi.Models.DTO;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Jwtapi.Services
{
    public class AuthService:IAuthService
    {
        private readonly UserManager<ApplicationUser> _usermanger;
        //use <identity rule> not application user  in role manger 
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly JWT _jwt;

        public AuthService(UserManager<ApplicationUser> usermanger, RoleManager<IdentityRole> roleManager, IOptions<JWT> jwt)
        {
            _usermanger = usermanger;
            _roleManager = roleManager;
            _jwt = jwt.Value;
        }
        public async Task<AuthModel> RegisterAsync(RegisterModel model)
        {
            if (await _usermanger.FindByEmailAsync(model.Email) is not null)
                return new AuthModel { Message="email is already register"};
            if (await _usermanger.FindByNameAsync(model.Username) is not null)
                return new AuthModel { Message = "Username is already register" };
            //You can make username and last name not repeated as above.

            var user = new ApplicationUser
            {
                UserName = model.Username,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
            };
            var result = await _usermanger.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                var errors = string.Empty;
                foreach (var error in result.Errors)
                {
                    errors += $"{error.Description},";

                    return new AuthModel { Message = errors };
                }

            }

            await _usermanger.AddToRoleAsync(user, "User");

            var jwtSecurityToken = await CreateJwtToken(user);

            return new AuthModel
            {
                Email = user.Email,
                ExpiresOn = jwtSecurityToken.ValidTo,
                IsAuthenticated = true,
                Roles = new List<string> { "User" },
                Token = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken),
                Username = user.UserName
            };

        }



        private async Task<JwtSecurityToken> CreateJwtToken(ApplicationUser user)
        {
            var userClaims = await _usermanger.GetClaimsAsync(user);
            var roles = await _usermanger.GetRolesAsync(user);
            var roleClaims = new List<Claim>();

            foreach (var role in roles)
                roleClaims.Add(new Claim("roles", role));

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserName),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("uid", user.Id)
            }
            .Union(userClaims)
            .Union(roleClaims);

            var symmetricSecurityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Key));
            var signingCredentials = new SigningCredentials(symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _jwt.Issuer,
                audience: _jwt.Audience,
                claims: claims,
                expires: DateTime.Now.AddDays(_jwt.DuratioinInDays),
                signingCredentials: signingCredentials);

            return jwtSecurityToken;
        }
        //genertat token when login correct
        public async Task<AuthModel> GetTokenAsync(TokenRequestModel model)
        {
            var authModel = new AuthModel();
            var user=await _usermanger.FindByEmailAsync(model.Email);
            //check
            if (user is null || !await _usermanger.CheckPasswordAsync(user,model.Password))
            {
                authModel.Message = "Email or Password is incorrect";
                return authModel;
            }
            // if data is correct apply below
            //togenerate token 
            var jwtSecurityToken= await CreateJwtToken(user);
            //to know role of user
            var roleList = await _usermanger.GetRolesAsync(user);
            
            authModel.IsAuthenticated = true;
            //to get token in response
            authModel.Token=new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken); 
            authModel.Email=user.Email;
            authModel.Username = user.UserName;
            authModel.ExpiresOn = jwtSecurityToken.ValidTo;
            authModel.Roles=roleList.ToList();

            return authModel;

        }



        public async Task<string> AddRoleAsync(AddRoleModel model)
        {
            var user = await _usermanger.FindByIdAsync(model.UserId);
            //to chek the role and user is exist in databas
            if (user is null || !await _roleManager.RoleExistsAsync(model.Role))
                return "Invalid UserId or Role";
            //to not choose the same role
            if (await _usermanger.IsInRoleAsync(user,model.Role))
                return "user is already assign to this role";
            // add new role
            var result = await _usermanger.AddToRoleAsync(user, model.Role);

            return result.Succeeded ? String.Empty : "Something went rong ";
        }





    }
}
