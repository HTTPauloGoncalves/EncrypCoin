using AutoMapper;
using EncrypCoin.API.Dtos.Application.User.Request;
using EncrypCoin.API.Dtos.Application.User.Response;
using EncrypCoin.API.Enums.User;
using EncrypCoin.API.Exceptions;
using EncrypCoin.API.Models;
using EncrypCoin.API.Repository.Implementations;
using EncrypCoin.API.Repository.Interfaces;
using EncrypCoin.API.Services.Application.Implementations;
using EncrypCoin.API.Services.Application.Interfaces;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace EncrypCoin.UnitTests.EncrypCoin.UnitTests.Services.Application
{
    public class UserServiceTests
    {
        #region Mocks
        private readonly Mock<IUserRepository> _userRepository = new();
        private readonly Mock<ICacheService> _cacheService = new();
        private readonly Mock<IMapper> _mapper = new();
        private readonly Mock<ITokenService> _tokenService = new();
        private readonly Mock<ILogger<UserService>> _logger = new();

        private UserService CreateService()
        {
            return new UserService(
                _logger.Object,
                _userRepository.Object,
                _tokenService.Object,
                _mapper.Object,
                _cacheService.Object
            );
        }
        #endregion

        #region EmailExistsAsync
        [Fact]
        public async Task EmailExistsAsync_ReturnsTrue_WhenEmailExists()
        {
            // Arrange
            string email = "paulo@gmail.com";

            _userRepository.Setup(r => r.EmailExistsAsync(email))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var exits = await service.EmailExistsAsync(email);

            // Assert
            Assert.True(exits);
            _userRepository.Verify(r => r.EmailExistsAsync(email), Times.Once);
        }

        [Fact]
        public async Task EmailExistsAsync_ReturnsFalse_DoesNotExists()
        {
            // Arrange
            string email = "ana@gmail.com";

            _userRepository
                .Setup(x => x.EmailExistsAsync(email))
                .ReturnsAsync(false);

            var service = CreateService();

            //Act
            var exits = await service.EmailExistsAsync(email);

            // Assert
            Assert.False(exits);
            _userRepository.Verify(r => r.EmailExistsAsync(email), Times.Once);
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldThrowArgumentException_WhenUsernameIsBlank()
        {
            // Arrange
            string email = "   ";

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.EmailExistsAsync(email)
            );

            _userRepository.Verify(r => r.EmailExistsAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task EmailExistsAsync_ShouldThrowArgumentException_WhenEmailIsInvalid()
        {
            // Arrange
            string email = "invalid-email";
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.EmailExistsAsync(email)
            );
            _userRepository.Verify(r => r.EmailExistsAsync(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region GetByEmailAsync

        [Fact]
        public async Task GetByEmailAsync_ReturnsUserResponseDto_WhenUserExists()
        {
            // Arrange
            string email = "paulo@gmail.com";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                Name = "Paulo"
            };

            var dto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Name
            };

            _userRepository
                .Setup(x => x.GetByEmailAsync(email))
                .ReturnsAsync(user);

            _mapper
                .Setup(m => m.Map<UserResponseDto>(It.IsAny<User>()))
                .Returns(dto);

            var service = CreateService();

            // Act
            var result = await service.GetByEmailAsync(email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(dto, result);

            _userRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string email = "paulo@gmail.com";

            _userRepository
                .Setup(r => r.GetByEmailAsync(email))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await service.GetByEmailAsync(email);
            });

            _userRepository.Verify(r => r.GetByEmailAsync(email), Times.Once);
        }

        [Fact]
        public async Task GetByEmailAsync_ShouldThrowArgumentException_WhenUsernameIsBlank()
        {
            // Arrange
            string email = "   ";

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetByEmailAsync(email)
            );

            _userRepository.Verify(r => r.GetByEmailAsync(It.IsAny<string>()), Times.Never);

        }
        #endregion

        #region GetByUsernameAsync
        [Fact]
        public async Task GetByUsernameAsync_ReturnsUserResponseDto_WhenUserExists()
        {
            //Arrange
            string username = "Paulo";

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = "paulo@gmail.com",
                Name = "Paulo"
            };

            var expectedDto = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Name
            };

            _mapper
                .Setup(m => m.Map<UserResponseDto>(It.IsAny<User>()))
                .Returns(expectedDto);

            _userRepository
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.GetByUsernameAsync(username);

            // Assert
            Assert.Equal(expectedDto, result);
            Assert.NotNull(result);

            _userRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
            _mapper.Verify(m => m.Map<UserResponseDto>(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            string username = "NonExistentUser";

            _userRepository
                .Setup(r => r.GetByUsernameAsync(username))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(async () =>
            {
                await service.GetByUsernameAsync(username);
            });

            _userRepository.Verify(r => r.GetByUsernameAsync(username), Times.Once);
        }

        [Fact]
        public async Task GetByUsernameAsync_ShouldThrowArgumentException_WhenUsernameIsBlank()
        {
            // Arrange
            string username = "   ";
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetByUsernameAsync(username)
            );

            _userRepository.Verify(r => r.GetByUsernameAsync(It.IsAny<string>()), Times.Never);
        }
        #endregion

        #region AuthenticateAsync

        [Fact]
        public async Task AuthenticateAsync_ShouldReturnAuthResponse_WhenCredentialsAreValid()
        {
            // Arrange
            var loginDto = new UserLoginRequestDto
            {
                Email = "paulo@gmail.com",
                Password = "123456"
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = loginDto.Email,
                Name = "Paulo",
                RefreshToken = "",
                RefreshTokenExpiryTime = DateTime.UtcNow
            };

            _userRepository
                .Setup(r => r.AuthenticateAsync(
                    loginDto.Email,
                    It.IsAny<string>()))
                .ReturnsAsync(user);

            _tokenService.Setup(t => t.GenerateToken(user)).Returns("access-token");
            _tokenService.Setup(t => t.GenerateRefreshToken()).Returns("refresh-token");
            _tokenService.Setup(t => t.GetExpirationMinutes()).Returns(30);

            var service = CreateService();

            // Act
            var result = await service.AuthenticateAsync(loginDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("access-token", result.AccessToken);
            Assert.Equal("refresh-token", result.RefreshToken);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Name, result.Username);

            _userRepository.Verify(r =>
                r.AuthenticateAsync(loginDto.Email, It.IsAny<string>()),
                Times.Once);

            _userRepository.Verify(r =>
                r.UpdateAsync(It.IsAny<User>()),
                Times.Once);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var dto = new UserLoginRequestDto
            {
                Email = "  ",
                Password = "senha"
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AuthenticateAsync(dto));

            _userRepository.Verify(r => r.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var dto = new UserLoginRequestDto
            {
                Email = "paulo@gmail.com",
                Password = " "
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.AuthenticateAsync(dto));

            _userRepository.Verify(r => r.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task AuthenticateAsync_ShouldThrowAuthenticationException_WhenUserIsNull()
        {
            // Arrange
            var loginDto = new UserLoginRequestDto
            {
                Email = "paulo@gmail.com",
                Password = "123456"
            };

            _userRepository
                .Setup(r => r.AuthenticateAsync(It.IsAny<string>(), It.IsAny<string>()))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<AuthenticationException>(() =>
                service.AuthenticateAsync(loginDto));

            _userRepository.Verify(r =>
                r.AuthenticateAsync(loginDto.Email, It.IsAny<string>()),
                Times.Once);
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_ReturnsUserResponseDto_WhenUserExists()
        {
            // Arrange
            var userId = Guid.NewGuid();

            var user = new User
            {
                Id = userId,
                Email = "paulo@gmail.com",
                Name = "Paulo"
            };

            var expectedDto = new UserResponseDto
            {
                Id = userId,
                Email = user.Email,
                Username = user.Name
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(userId))
                .ReturnsAsync(user);

            _mapper
                .Setup(m => m.Map<UserResponseDto>(user))
                .Returns(expectedDto);

            var service = CreateService();

            // Act
            var result = await service.GetByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);

            _userRepository.Verify(r => r.GetByIdAsync(userId), Times.Once);
            _mapper.Verify(m => m.Map<UserResponseDto>(user), Times.Once);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            Guid id = Guid.Empty;

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.GetByIdAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            Guid id = Guid.NewGuid();

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.GetByIdAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        #endregion

        #region RegisterAsync

        [Fact]
        public async Task RegisterAsync_ShouldRegisterUser_WhenDataIsValid()
        {
            // Arrange
            var registerDto = new UserRegisterRequestDto
            {
                Email = "paulo@gmail.com",
                Username = "Paulo",
                Password = "123456"
            };

            var mappedUser = new User
            {
                Id = Guid.NewGuid(),
                Email = registerDto.Email,
                Name = registerDto.Username,
                PasswordHash = ""
            };

            var expectedDto = new UserResponseDto
            {
                Id = mappedUser.Id,
                Email = mappedUser.Email,
                Username = mappedUser.Name
            };

            _userRepository.Setup(r => r.EmailExistsAsync(registerDto.Email)).ReturnsAsync(false);
            _userRepository.Setup(r => r.UsernameExistsAsync(registerDto.Username)).ReturnsAsync(false);

            _mapper.Setup(m => m.Map<User>(registerDto)).Returns(mappedUser);
            _mapper.Setup(m => m.Map<UserResponseDto>(mappedUser)).Returns(expectedDto);

            var service = CreateService();

            // Act
            var result = await service.RegisterAsync(registerDto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expectedDto, result);

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Once);
            _mapper.Verify(m => m.Map<User>(registerDto), Times.Once);
            _mapper.Verify(m => m.Map<UserResponseDto>(mappedUser), Times.Once);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var dto = new UserRegisterRequestDto
            {
                Email = "  ",
                Username = "Paulo",
                Password = "123456"
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(dto));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowValidationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new UserRegisterRequestDto
            {
                Email = "paulo@gmail.com",
                Username = "Paulo",
                Password = "123456"
            };

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(true);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(dto));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenUsernameIsEmpty()
        {
            // Arrange
            var dto = new UserRegisterRequestDto
            {
                Email = "paulo@gmail.com",
                Username = " ",
                Password = "123456"
            };

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(dto));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowValidationException_WhenUsernameAlreadyExists()
        {
            // Arrange
            var dto = new UserRegisterRequestDto
            {
                Email = "paulo@gmail.com",
                Username = "Paulo",
                Password = "123456"
            };

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
            _userRepository.Setup(r => r.UsernameExistsAsync(dto.Username)).ReturnsAsync(true);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() => service.RegisterAsync(dto));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        [Fact]
        public async Task RegisterAsync_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var dto = new UserRegisterRequestDto
            {
                Email = "paulo@gmail.com",
                Username = "Paulo",
                Password = " "
            };

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email)).ReturnsAsync(false);
            _userRepository.Setup(r => r.UsernameExistsAsync(dto.Username)).ReturnsAsync(false);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => service.RegisterAsync(dto));

            _userRepository.Verify(r => r.AddAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region UsernameExistsAsync

        [Fact]
        public async Task UsernameExistsAsync_ReturnsTrue_WhenUsernameExists()
        {
            // Arrange
            string username = "Paulo";

            _userRepository
                .Setup(r => r.UsernameExistsAsync(username))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act
            var exists = await service.UsernameExistsAsync(username);

            // Assert
            Assert.True(exists);
            _userRepository.Verify(r => r.UsernameExistsAsync(username), Times.Once);
        }

        [Fact]
        public async Task UsernameExistsAsync_ReturnsFalse_WhenUsernameDoesNotExist()
        {
            // Arrange
            string username = "Ana";

            _userRepository
                .Setup(r => r.UsernameExistsAsync(username))
                .ReturnsAsync(false);

            var service = CreateService();

            // Act
            var exists = await service.UsernameExistsAsync(username);

            // Assert
            Assert.False(exists);
            _userRepository.Verify(r => r.UsernameExistsAsync(username), Times.Once);
        }

        [Fact]
        public async Task UsernameExistsAsync_ShouldThrowArgumentException_WhenUsernameIsBlank()
        {
            // Arrange
            string username = "   ";

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UsernameExistsAsync(username));

            _userRepository.Verify(r => r.UsernameExistsAsync(It.IsAny<string>()), Times.Never);
        }

        #endregion

        #region GetAllAsync

        [Fact]
        public async Task GetAllAsync_ShouldReturnCachedUsers_WhenCacheHasData()
        {
            // Arrange
            var cacheKey = "users:all";

            var cachedList = new List<UserResponseDto>
            {
                new UserResponseDto { Id = Guid.NewGuid(), Email = "cached@mail.com", Username = "CachedUser" }
            };

            _cacheService
                .Setup(c => c.GetAsync<List<UserResponseDto>>(cacheKey))
                .ReturnsAsync(cachedList);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("cached@mail.com", result[0].Email);

            _cacheService.Verify(c => c.GetAsync<List<UserResponseDto>>(cacheKey), Times.Once);
            _userRepository.Verify(r => r.GetAllAsync(), Times.Never); // Não deve chamar banco
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
        {
            // Arrange
            _cacheService
                .Setup(c => c.GetAsync<List<UserResponseDto>>("users:all"))
                .ReturnsAsync((List<UserResponseDto>?)null);

            _userRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(new List<User>());

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);

            _userRepository.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnMappedUsers_AndStoreInCache()
        {
            // Arrange
            var users = new List<User>
            {
                new User { Id = Guid.NewGuid(), Email = "paulo@gmail.com", Name = "Paulo" },
                new User { Id = Guid.NewGuid(), Email = "ana@gmail.com", Name = "Ana" }
            };

            var mapped = new List<UserResponseDto>
            {
                new UserResponseDto { Id = users[0].Id, Email = users[0].Email, Username = users[0].Name },
                new UserResponseDto { Id = users[1].Id, Email = users[1].Email, Username = users[1].Name }
            };

            _cacheService
                .Setup(c => c.GetAsync<List<UserResponseDto>>("users:all"))
                .ReturnsAsync((List<UserResponseDto>?)null);

            _userRepository
                .Setup(r => r.GetAllAsync())
                .ReturnsAsync(users);

            _mapper
                .Setup(m => m.Map<List<UserResponseDto>>(users))
                .Returns(mapped);

            var service = CreateService();

            // Act
            var result = await service.GetAllAsync();

            // Assert
            Assert.Equal(mapped.Count, result.Count);

            _cacheService.Verify(c =>
                c.SetAsync("users:all", mapped, It.IsAny<TimeSpan>()),
                Times.Once);

            _mapper.Verify(m => m.Map<List<UserResponseDto>>(users), Times.Once);
        }

        #endregion

        #region UpdateUserAdminAsync

        [Fact]
        public async Task UpdateUserAdminAsync_ShouldUpdateUser_WhenDataIsValid()
        {
            // Arrange
            var dto = new UserUpdateAdminRequestDto
            {
                Id = Guid.NewGuid(),
                Name = "Novo Nome",
                Email = "novoemail@gmail.com",
                Role = UserRole.Admin,
                IsActive = true,
                Password = "novaSenha123"
            };

            var user = new User
            {
                Id = dto.Id,
                Name = "Antigo Nome",
                Email = "old@mail.com",
                Role = "User",
                IsActive = false
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            var expected = new UserResponseDto
            {
                Id = user.Id,
                Email = dto.Email,
                Username = dto.Name
            };

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(expected);

            var service = CreateService();

            // Act
            var result = await service.UpdateUserAdminAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            Assert.Equal(dto.Name, user.Name);
            Assert.Equal(dto.Email, user.Email);
            Assert.Equal(dto.Role.ToString(), user.Role);
            Assert.True(user.IsActive);

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAdminAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateAdminRequestDto
            {
                Id = Guid.Empty
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateUserAdminAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateUserAdminAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new UserUpdateAdminRequestDto
            {
                Id = Guid.NewGuid()
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateUserAdminAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateUserAdminAsync_ShouldHashNewPassword_WhenPasswordProvided()
        {
            // Arrange
            var dto = new UserUpdateAdminRequestDto
            {
                Id = Guid.NewGuid(),
                Password = "novaSenha"
            };

            var user = new User
            {
                Id = dto.Id,
                PasswordHash = "oldHash"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(new UserResponseDto());

            var service = CreateService();

            // Act
            var result = await service.UpdateUserAdminAsync(dto);

            // Assert
            Assert.NotEqual("oldHash", user.PasswordHash); // senha deve mudar

            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        #endregion

        #region UpdateNameAsync

        [Fact]
        public async Task UpdateNameAsync_ShouldUpdateName_WhenValid()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Name = "Novo Nome"
            };

            var user = new User
            {
                Id = dto.Id,
                Name = "Nome Antigo"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            var expected = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = dto.Name
            };

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(expected);

            var service = CreateService();

            // Act
            var result = await service.UpdateNameAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.Equal("Novo Nome", user.Name);

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateNameAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.Empty,
                Name = "Nome"
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateNameAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNameAsync_ShouldThrowArgumentException_WhenNameIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Name = " "
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateNameAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateNameAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Name = "Novo Nome"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateNameAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
        }

        #endregion

        #region SetUserActiveStatusAsync

        [Fact]
        public async Task SetUserActiveStatusAsync_ShouldReturnTrue_WhenUserExists()
        {
            // Arrange
            var id = Guid.NewGuid();
            bool newStatus = true;

            var user = new User
            {
                Id = id,
                Name = "Paulo",
                Email = "paulo@gmail.com",
                IsActive = false
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.SetUserActiveStatusAsync(id, newStatus);

            // Assert
            Assert.True(result);
            Assert.True(user.IsActive);

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task SetUserActiveStatusAsync_ShouldReturnFalse_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            bool newStatus = false;

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act
            var result = await service.SetUserActiveStatusAsync(id, newStatus);

            // Assert
            Assert.False(result);
            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region UpdateEmailAsync

        [Fact]
        public async Task UpdateEmailAsync_ShouldUpdateEmail_WhenValid()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Email = "novoemail@gmail.com"
            };

            var user = new User
            {
                Id = dto.Id,
                Email = "old@mail.com"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email))
                .ReturnsAsync(false);

            var expected = new UserResponseDto
            {
                Id = user.Id,
                Email = dto.Email,
                Username = user.Name
            };

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                .Returns(expected);

            var service = CreateService();

            // Act
            var result = await service.UpdateEmailAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.Equal(dto.Email, user.Email);

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _userRepository.Verify(r => r.EmailExistsAsync(dto.Email), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.Empty,
                Email = "teste@mail.com"
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateEmailAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateEmailAsync_ShouldThrowArgumentException_WhenEmailIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Email = "   "
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdateEmailAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdateEmailAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Email = "novo@mail.com"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateEmailAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
        }

        [Fact]
        public async Task UpdateEmailAsync_ShouldThrowValidationException_WhenEmailAlreadyExists()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Email = "novo@mail.com"
            };

            var user = new User
            {
                Id = dto.Id,
                Email = "old@mail.com"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            _userRepository.Setup(r => r.EmailExistsAsync(dto.Email))
                .ReturnsAsync(true);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ValidationException>(() =>
                service.UpdateEmailAsync(dto));

            _userRepository.Verify(r => r.EmailExistsAsync(dto.Email), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region UpdatePasswordAsync

        [Fact]
        public async Task UpdatePasswordAsync_ShouldUpdatePassword_WhenValid()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Password = "novaSenha123"
            };

            var user = new User
            {
                Id = dto.Id,
                PasswordHash = "oldHash"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync(user);

            var expected = new UserResponseDto
            {
                Id = user.Id,
                Email = user.Email,
                Username = user.Name
            };

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                .Returns(expected);

            var service = CreateService();

            // Act
            var result = await service.UpdatePasswordAsync(dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);
            Assert.NotEqual("oldHash", user.PasswordHash);

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.Empty,
                Password = "senha"
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdatePasswordAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowArgumentException_WhenPasswordIsEmpty()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Password = " "
            };

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.UpdatePasswordAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task UpdatePasswordAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var dto = new UserUpdateRequestDto
            {
                Id = Guid.NewGuid(),
                Password = "novaSenha"
            };

            _userRepository.Setup(r => r.GetByIdAsync(dto.Id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdatePasswordAsync(dto));

            _userRepository.Verify(r => r.GetByIdAsync(dto.Id), Times.Once);
        }

        #endregion

        #region DeactivateUserAsync

        [Fact]
        public async Task DeactivateUserAsync_ShouldDeactivateUser_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();

            var user = new User
            {
                Id = id,
                IsActive = true
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.DeactivateUserAsync(id);

            // Assert
            Assert.True(result);
            Assert.False(user.IsActive);

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            var id = Guid.Empty;
            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.DeactivateUserAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeactivateUserAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeactivateUserAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region DeleteUserAsync

        [Fact]
        public async Task DeleteUserAsync_ShouldDeleteUser_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();

            var user = new User
            {
                Id = id,
                Email = "paulo@gmail.com",
                Name = "Paulo"
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            var result = await service.DeleteUserAsync(id);

            // Assert
            Assert.True(result);

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.DeleteByIdAsync(id), Times.Once);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowArgumentException_WhenIdIsEmpty()
        {
            // Arrange
            Guid id = Guid.Empty;

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() =>
                service.DeleteUserAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Fact]
        public async Task DeleteUserAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.DeleteUserAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.DeleteByIdAsync(It.IsAny<Guid>()), Times.Never);
        }

        #endregion

        #region UpdateAsync

        [Fact]
        public async Task UpdateAsync_ShouldUpdateAllFields_WhenValid()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UserUpdateRequestDto
            {
                Name = "Novo Nome",
                Email = "novo@mail.com",
                Password = "novaSenha"
            };

            var user = new User
            {
                Id = id,
                Name = "Antigo",
                Email = "old@mail.com",
                PasswordHash = "oldHash"
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var expected = new UserResponseDto
            {
                Id = id,
                Email = dto.Email,
                Username = dto.Name
            };

            _mapper
                .Setup(m => m.Map<UserResponseDto>(user))
                .Returns(expected);

            var service = CreateService();

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(expected, result);

            Assert.Equal(dto.Name, user.Name);
            Assert.Equal(dto.Email, user.Email);
            Assert.NotEqual("oldHash", user.PasswordHash);

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyName_WhenOnlyNameProvided()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UserUpdateRequestDto
            {
                Name = "Novo Nome"
            };

            var user = new User
            {
                Id = id,
                Name = "Antigo Nome",
                Email = "old@mail.com"
            };

            _userRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(new UserResponseDto());

            var service = CreateService();

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Novo Nome", user.Name);
            Assert.Equal("old@mail.com", user.Email); // unchanged

            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyEmail_WhenOnlyEmailProvided()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UserUpdateRequestDto
            {
                Email = "novo@mail.com"
            };

            var user = new User
            {
                Id = id,
                Name = "Paulo",
                Email = "old@mail.com"
            };

            _userRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(new UserResponseDto());

            var service = CreateService();

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("novo@mail.com", user.Email);

            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldUpdateOnlyPassword_WhenOnlyPasswordProvided()
        {
            // Arrange
            var id = Guid.NewGuid();

            var dto = new UserUpdateRequestDto
            {
                Password = "newPass"
            };

            var user = new User
            {
                Id = id,
                PasswordHash = "oldHash"
            };

            _userRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(user);

            _mapper.Setup(m => m.Map<UserResponseDto>(user))
                   .Returns(new UserResponseDto());

            var service = CreateService();

            // Act
            var result = await service.UpdateAsync(id, dto);

            // Assert
            Assert.NotNull(result);
            Assert.NotEqual("oldHash", user.PasswordHash);

            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();
            var dto = new UserUpdateRequestDto
            {
                Name = "Novo Nome"
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.UpdateAsync(id, dto));

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

        #region LogoutAsync

        [Fact]
        public async Task LogoutAsync_ShouldClearRefreshToken_WhenUserExists()
        {
            // Arrange
            var id = Guid.NewGuid();

            var user = new User
            {
                Id = id,
                RefreshToken = "oldRefreshToken",
                RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(5)
            };

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync(user);

            var service = CreateService();

            // Act
            await service.LogoutAsync(id);

            // Assert
            Assert.Equal("", user.RefreshToken);
            Assert.True(user.RefreshTokenExpiryTime <= DateTime.UtcNow);

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(user), Times.Once);
        }

        [Fact]
        public async Task LogoutAsync_ShouldThrowNotFound_WhenUserDoesNotExist()
        {
            // Arrange
            var id = Guid.NewGuid();

            _userRepository
                .Setup(r => r.GetByIdAsync(id))
                .ReturnsAsync((User?)null);

            var service = CreateService();

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() =>
                service.LogoutAsync(id));

            _userRepository.Verify(r => r.GetByIdAsync(id), Times.Once);
            _userRepository.Verify(r => r.UpdateAsync(It.IsAny<User>()), Times.Never);
        }

        #endregion

    }
}
