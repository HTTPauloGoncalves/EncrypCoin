using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Dtos.Application.User.Response;
using EncrypCoin.API.Services.Application.Interfaces;
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

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserByIdAsync(Guid id)
        {
            var userDto = await _userService.GetByIdAsync(id);

            if (userDto == null)
                return NotFound();

            return Ok(userDto);
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllAsync()
        {
            List<UserResponseDto> users = await _userService.GetAllAsync();

            if (users == null || users.Count == 0)
                return NotFound();

            return Ok(users);
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsernameAsync(string username)
        {
            var userDto = await _userService.GetByUsernameAsync(username);

            if (userDto == null)
                return NotFound();

            return Ok(userDto);
        }

        [HttpGet("email/{email}")]
        public async Task<IActionResult> GetUserByEmailAsync(string email)
        {
            var userDto = await _userService.GetByEmailAsync(email);

            if (userDto == null)
                return NotFound();

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

        [HttpPost("authenticate")]
        public async Task<IActionResult> AuthenticateUserAsync([FromBody] UserLoginRequestDto user)
        {
            if (user == null)
                return BadRequest("Não pode ser nulo");

            if (user.Email == null || user.Password == null)
                return BadRequest("Campos obrigatórios não podem ser nulos");

            var authenticatedUser = await _userService.AuthenticateAsync(user);

            if (authenticatedUser == null)
                return Unauthorized("Credenciais inválidas");

            return Ok(authenticatedUser);
        }

        [HttpPost]
        public async Task<IActionResult> RegisterUserAsync([FromBody] UserRegisterRequestDto user)
        {
            if (user == null)
                return BadRequest("Não pode ser nulo");

            if (user.Username == null || user.Email == null || user.Password == null)
                return BadRequest("Campos obrigatórios não podem ser nulos");

            var createdUser = await _userService.RegisterAsync(user);

            return Ok(createdUser);
        }
    }
}