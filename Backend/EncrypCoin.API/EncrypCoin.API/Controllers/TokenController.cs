using EncrypCoin.API.Dtos.Application.Auth.Request;
using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Services.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace EncrypCoin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TokenController : ControllerBase
    {
        private readonly IRefreshTokenService _refreshTokenService;

        public TokenController(IRefreshTokenService refreshTokenService)
        {
            _refreshTokenService = refreshTokenService;
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshAsync([FromBody] TokenRefreshRequestDto request)
        {
            try
            {
                var result = await _refreshTokenService.RefreshAsync(request);
                return Ok(result);
            }
            catch (SecurityTokenException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Erro interno ao renovar o token.", details = ex.Message });
            }
        }

    }
}
