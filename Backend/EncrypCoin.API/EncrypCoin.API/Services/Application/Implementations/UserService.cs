using AutoMapper;
using EncrypCoin.API.Dtos.Application.Auth.Response;
using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Dtos.Application.User.Response;
using EncrypCoin.API.Exceptions;
using EncrypCoin.API.Models;
using EncrypCoin.API.Repository.Interfaces;
using EncrypCoin.API.Services.Application.Interfaces;

namespace EncrypCoin.API.Services.Application.Implementations
{
    public class UserService : IUserService
    {
        private readonly ILogger<UserService> _logger;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        private readonly ICacheService _cache;

        public UserService(
            ILogger<UserService> logger,
            IUserRepository userRepository,
            ITokenService tokenService,
            IMapper mapper,
            ICacheService cache)
        {
            _logger = logger;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
            _cache = cache;
        }

        public async Task<AuthResponseDto> AuthenticateAsync(UserLoginRequestDto userLoginRequestDto)
        {
            if (string.IsNullOrWhiteSpace(userLoginRequestDto.Email))
                throw new ArgumentException("Email não pode ser vazio.");

            if (string.IsNullOrWhiteSpace(userLoginRequestDto.Password))
                throw new ArgumentException("Senha não pode ser vazia.");

            _logger.LogInformation("Autenticando usuário com email: {Email}", userLoginRequestDto.Email);

            string passwordHash = PasswordHasher.HashPassword(userLoginRequestDto.Password);
            var user = await _userRepository.AuthenticateAsync(userLoginRequestDto.Email, passwordHash);

            if (user == null)
            {
                _logger.LogWarning("Falha na autenticação para o email: {Email}", userLoginRequestDto.Email);
                throw new AuthenticationException("Email ou senha inválidos.");
            }

            var accessToken = _tokenService.GenerateToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await _userRepository.UpdateAsync(user);

            _logger.LogInformation("Usuário autenticado: {Email}", userLoginRequestDto.Email);

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Expiration = DateTime.UtcNow.AddMinutes(Convert.ToInt32(_tokenService.GetExpirationMinutes())),
                Username = user.Name,
                Email = user.Email
            };
        }

        public async Task<bool> EmailExistsAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio.");

            return await _userRepository.EmailExistsAsync(email);

        }

        public async Task<UserResponseDto?> GetByEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email não pode ser vazio.");

            _logger.LogInformation("Obtendo usuário pelo email: {Email}", email);

            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                _logger.LogWarning("Usuário com email {Email} não encontrado.", email);
                throw new NotFoundException($"Usuário com email {email} não encontrado.");
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> GetByIdAsync(Guid id)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");

            _logger.LogInformation("Obtendo usuário pelo ID: {UserId}", id);

            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado.", id);
                throw new NotFoundException($"Usuário com ID {id} não encontrado.");
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto?> GetByUsernameAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nome de usuário não pode ser vazio.");

            _logger.LogInformation("Obtendo usuário pelo nome de usuário: {Username}", username);

            var user = await _userRepository.GetByUsernameAsync(username);

            if (user == null)
            {
                _logger.LogWarning("Usuário com nome de usuário {Username} não encontrado.", username);
                throw new NotFoundException($"Usuário com nome de usuário {username} não encontrado.");
            }

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> RegisterAsync(UserRegisterRequestDto userRegisterRequestDto)
        {
            bool emailExists = await _userRepository.EmailExistsAsync(userRegisterRequestDto.Email);
            bool usernameExists = await _userRepository.UsernameExistsAsync(userRegisterRequestDto.Username);

            if (string.IsNullOrWhiteSpace(userRegisterRequestDto.Email))
                throw new ArgumentException("Email não pode ser vazio.");

            if (emailExists)
                throw new ValidationException($"Email {userRegisterRequestDto.Email} já está em uso.");

            if (string.IsNullOrWhiteSpace(userRegisterRequestDto.Username))
                throw new ArgumentException("Nome de usuário não pode ser vazio.");

            if (usernameExists)
                throw new ValidationException($"Nome de usuário {userRegisterRequestDto.Username} já está em uso.");

            if (string.IsNullOrWhiteSpace(userRegisterRequestDto.Password))
                throw new ArgumentException("Senha não pode ser vazia.");

            _logger.LogInformation("Registrando novo usuário: {Username}", userRegisterRequestDto.Username);

            var newUser = _mapper.Map<User>(userRegisterRequestDto);
            newUser.PasswordHash = PasswordHasher.HashPassword(userRegisterRequestDto.Password);

            _logger.LogInformation("Salvando novo usuário no repositório: {Username}", userRegisterRequestDto.Username);
            await _userRepository.AddAsync(newUser);

            _logger.LogInformation("Usuário registrado com sucesso: {Username}", userRegisterRequestDto.Username);

            return _mapper.Map<UserResponseDto>(newUser);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentException("Nome de usuário não pode ser vazio.");

            return await _userRepository.UsernameExistsAsync(username);
        }

        public async Task<List<UserResponseDto>> GetAllAsync()
        {
            const string cacheKey = "users:all";

            var cachedUsers = await _cache.GetAsync<List<UserResponseDto>>(cacheKey);
            if (cachedUsers != null)
            {
                _logger.LogInformation("Retornando lista de usuários do cache (Redis).");
                return cachedUsers;
            }

            _logger.LogInformation("Buscando todos os usuários no banco de dados...");
            var users = await _userRepository.GetAllAsync();

            if (users == null || !users.Any())
            {
                _logger.LogWarning("⚠️ Nenhum usuário encontrado.");
                return new List<UserResponseDto>();
            }

            var mappedUsers = _mapper.Map<List<UserResponseDto>>(users);

            await _cache.SetAsync(cacheKey, mappedUsers, TimeSpan.FromMinutes(5));
            _logger.LogInformation("✅ Usuários armazenados no cache Redis por 5 minutos.");

            return mappedUsers;
        }


        public async Task<UserResponseDto> UpdateUserAdminAsync(UserUpdateAdminRequestDto userUpdateAdminRequestDto)
        {
            if (userUpdateAdminRequestDto.Id == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");

            _logger.LogInformation("Atualizando usuário com ID: {UserId}", userUpdateAdminRequestDto.Id);

            var user = await _userRepository.GetByIdAsync(userUpdateAdminRequestDto.Id);
            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado para atualização.", userUpdateAdminRequestDto.Id);
                throw new NotFoundException($"Usuário com ID {userUpdateAdminRequestDto.Id} não encontrado.");
            }

            user.Name = userUpdateAdminRequestDto.Name ?? user.Name;
            user.Email = userUpdateAdminRequestDto.Email ?? user.Email;
            user.Role = userUpdateAdminRequestDto.Role?.ToString() ?? user.Role;
            user.IsActive = userUpdateAdminRequestDto.IsActive ?? user.IsActive;

            if (!string.IsNullOrWhiteSpace(userUpdateAdminRequestDto.Password))
                user.PasswordHash = PasswordHasher.HashPassword(userUpdateAdminRequestDto.Password);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Usuário com ID {UserId} atualizado com sucesso.", userUpdateAdminRequestDto.Id);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdateNameAsync(UserUpdateRequestDto userUpdateRequestDto)
        {
            if (userUpdateRequestDto.Id == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(userUpdateRequestDto.Name))
                throw new ArgumentException("Nome não pode ser vazio.");

            _logger.LogInformation("Atualizando nome do usuário com ID: {UserId}", userUpdateRequestDto.Id);

            var user = await _userRepository.GetByIdAsync(userUpdateRequestDto.Id);
            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado para atualização.", userUpdateRequestDto.Id);
                throw new NotFoundException($"Usuário com ID {userUpdateRequestDto.Id} não encontrado.");
            }

            user.Name = userUpdateRequestDto.Name;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Nome do usuário com ID {UserId} atualizado com sucesso.", userUpdateRequestDto.Id);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdateEmailAsync(UserUpdateRequestDto userUpdateRequestDto)
        {
            if (userUpdateRequestDto.Id == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(userUpdateRequestDto.Email))
                throw new ArgumentException("Email não pode ser vazio.");

            _logger.LogInformation("Atualizando email do usuário com ID: {UserId}", userUpdateRequestDto.Id);

            var user = await _userRepository.GetByIdAsync(userUpdateRequestDto.Id);
            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado para atualização.", userUpdateRequestDto.Id);
                throw new NotFoundException($"Usuário com ID {userUpdateRequestDto.Id} não encontrado.");
            }

            if (await _userRepository.EmailExistsAsync(userUpdateRequestDto.Email))
                throw new ValidationException($"Email {userUpdateRequestDto.Email} já está em uso.");

            user.Email = userUpdateRequestDto.Email;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Email do usuário com ID {UserId} atualizado com sucesso.", userUpdateRequestDto.Id);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<UserResponseDto> UpdatePasswordAsync(UserUpdateRequestDto userUpdateRequestDto)
        {
            if (userUpdateRequestDto.Id == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");
            if (string.IsNullOrWhiteSpace(userUpdateRequestDto.Password))
                throw new ArgumentException("Senha não pode ser vazia.");

            _logger.LogInformation("Atualizando senha do usuário com ID: {UserId}", userUpdateRequestDto.Id);

            var user = await _userRepository.GetByIdAsync(userUpdateRequestDto.Id);
            if (user == null)
            {
                _logger.LogWarning("Usuário com ID {UserId} não encontrado para atualização.", userUpdateRequestDto.Id);
                throw new NotFoundException($"Usuário com ID {userUpdateRequestDto.Id} não encontrado.");
            }

            user.PasswordHash = PasswordHasher.HashPassword(userUpdateRequestDto.Password);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Senha do usuário com ID {UserId} atualizada com sucesso.", userUpdateRequestDto.Id);

            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> DeactivateUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException($"Usuário com ID {userId} não encontrado.");

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            _logger.LogInformation("Usuário com ID {UserId} desativado com sucesso.", userId);
            return true;
        }

        public async Task<bool> DeleteUserAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("ID do usuário não pode ser vazio.");

            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException($"Usuário com ID {userId} não encontrado.");

            await _userRepository.DeleteByIdAsync(userId);

            _logger.LogInformation("Usuário com ID {UserId} deletado com sucesso.", userId);
            return true;
        }

        public async Task<UserResponseDto> UpdateAsync(Guid id, UserUpdateRequestDto dto)
        {
            var user = await _userRepository.GetByIdAsync(id)
                ?? throw new NotFoundException($"Usuário com ID {id} não encontrado.");

            if (!string.IsNullOrWhiteSpace(dto.Name))
                user.Name = dto.Name;

            if (!string.IsNullOrWhiteSpace(dto.Email))
                user.Email = dto.Email;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.PasswordHash = PasswordHasher.HashPassword(dto.Password);

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            return _mapper.Map<UserResponseDto>(user);
        }

    }
}