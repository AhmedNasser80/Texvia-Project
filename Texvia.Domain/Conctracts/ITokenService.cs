using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Texvia.Domain.Models;
using Texvia.Shared.Dtos;

namespace Texvia.Domain.Conctracts
{
    public interface ITokenService
    {
        Task<TokenResponse> GenerateTokensAsync(ApplicationUser user);
        Task<TokenResponse> RefreshTokenAsync(string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);


    }
}
