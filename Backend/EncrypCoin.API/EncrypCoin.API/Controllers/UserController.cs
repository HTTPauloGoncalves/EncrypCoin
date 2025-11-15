using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Dtos.Application.User.Response;
using EncrypCoin.API.Services.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EncrypCoin.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
            => _userService = userService;

        // ----------------------------
        // GET
        // ----------------------------
        [Authorize]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(UserResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var userDto = await _userService.GetByIdAsync(id);
            return userDto is null ? NotFound() : Ok(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), 200)]
        public async Task<IActionResult> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("by-username/{username}")]
        public async Task<IActionResult> GetUserByUsernameAsync(string username)
        {
            var userDto = await _userService.GetByUsernameAsync(username);
            return userDto is null ? NotFound() : Ok(userDto);
        }

        [Authorize] // 🔒 segurança
        [HttpGet("by-email/{email}")]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var userDto = await _userService.GetByEmailAsync(email);
            return userDto is null ? NotFound() : Ok(userDto);
        }

        [Authorize]
        [HttpGet("exists")]
        public async Task<IActionResult> ExistsAsync([FromQuery] string username, [FromQuery] string email)
        {
            if (!string.IsNullOrWhiteSpace(username))
                return Ok(await _userService.UsernameExistsAsync(username));

            if (!string.IsNullOrWhiteSpace(email))
                return Ok(await _userService.EmailExistsAsync(email));

            return BadRequest("Informe username ou email.");
        }

        // ----------------------------
        // POST
        // ----------------------------
        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateUserAsync([FromBody] UserLoginRequestDto dto)
        {
            if (dto is null)
                return BadRequest("Dados inválidos.");

            var authenticatedUser = await _userService.AuthenticateAsync(dto);
            return authenticatedUser is null ? Unauthorized() : Ok(authenticatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUserAsync([FromBody] UserRegisterRequestDto dto)
        {
            if (dto is null)
                return BadRequest("Dados inválidos.");

            var createdUser = await _userService.RegisterAsync(dto);
            return Ok(createdUser);
        }

        // ----------------------------
        // PUT / PATCH
        // ----------------------------
        [Authorize]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateUserAsync(Guid id, [FromBody] UserUpdateRequestDto dto)
        {
            var updated = await _userService.UpdateAsync(id, dto);
            return updated is null ? NotFound() : Ok(updated);
        }

        [Authorize(Roles = "Admin")]
        [HttpPatch("{id:guid}/deactivate")]
        public async Task<IActionResult> DeactivateUserAsync(Guid id)
        {
            bool result = await _userService.DeactivateUserAsync(id);
            return result ? Ok("Usuário desativado.") : NotFound("Usuário não encontrado.");
        }

        // ----------------------------
        // DELETE
        // ----------------------------
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteUserAsync(Guid id)
        {
            bool result = await _userService.DeleteUserAsync(id);
            return result ? Ok("Usuário removido.") : NotFound("Usuário não encontrado.");
        }
    }
}
