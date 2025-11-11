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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var userDto = await _userService.GetByIdAsync(id);
            return Ok(userDto);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> GetAllAsync()
        {
            var users = await _userService.GetAllAsync();
            return Ok(users);
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsernameAsync(string username)
        {
            var userDto = await _userService.GetByUsernameAsync(username);
            return Ok(userDto);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var userDto = await _userService.GetByEmailAsync(email);
            return Ok(userDto);
        }

        [HttpGet("exists/username/{username}")]
        public async Task<IActionResult> UsernameExistsAsync(string username)
        {
            bool exists = await _userService.UsernameExistsAsync(username);
            return Ok(exists);
        }

        [HttpGet("exists/email/{email}")]
        public async Task<IActionResult> EmailExistsAsync(string email)
        {
            bool exists = await _userService.EmailExistsAsync(email);
            return Ok(exists);
        }

        // ----------------------------
        // POST
        // ----------------------------

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateUserAsync([FromBody] UserLoginRequestDto user)
        {
            if (user == null)
                return BadRequest("Dados inválidos.");

            var authenticatedUser = await _userService.AuthenticateAsync(user);
            return Ok(authenticatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUserAsync([FromBody] UserRegisterRequestDto user)
        {
            if (user == null)
                return BadRequest("Dados inválidos.");

            var createdUser = await _userService.RegisterAsync(user);
            return Ok(createdUser);
        }

        // ----------------------------
        // PUT / PATCH
        // ----------------------------

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateUserAsync([FromBody] UserUpdateRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            var updatedUser = await _userService.UpdateAsync(dto);
            return Ok(updatedUser);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update/admin")]
        public async Task<IActionResult> UpdateUserAdminAsync([FromBody] UserUpdateAdminRequestDto dto)
        {
            if (dto == null)
                return BadRequest("Dados inválidos.");

            var updatedUser = await _userService.UpdateUserAdminAsync(dto);
            return Ok(updatedUser);
        }

        [Authorize]
        [HttpPut("update/name")]
        public async Task<IActionResult> UpdateNameAsync([FromBody] UserUpdateRequestDto dto)
        {
            var updated = await _userService.UpdateNameAsync(dto);
            return Ok(updated);
        }

        [Authorize]
        [HttpPut("update/email")]
        public async Task<IActionResult> UpdateEmailAsync([FromBody] UserUpdateRequestDto dto)
        {
            var updated = await _userService.UpdateEmailAsync(dto);
            return Ok(updated);
        }

        [Authorize]
        [HttpPut("update/password")]
        public async Task<IActionResult> UpdatePasswordAsync([FromBody] UserUpdateRequestDto dto)
        {
            var updated = await _userService.UpdatePasswordAsync(dto);
            return Ok(updated);
        }

        // ----------------------------
        // PATCH (para status)
        // ----------------------------

        [Authorize(Roles = "Admin")]
        [HttpPatch("deactivate/{userId}")]
        public async Task<IActionResult> DeactivateUserAsync(Guid userId)
        {
            bool result = await _userService.DeactivateUserAsync(userId);
            return result ? Ok("Usuário desativado com sucesso.") : BadRequest("Falha ao desativar o usuário.");
        }

        // ----------------------------
        // DELETE
        // ----------------------------

        [Authorize(Roles = "Admin")]
        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUserAsync(Guid userId)
        {
            bool result = await _userService.DeleteUserAsync(userId);
            return result ? Ok("Usuário deletado com sucesso.") : BadRequest("Falha ao deletar o usuário.");
        }
    }
}
