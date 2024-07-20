using Autorisation.Interfaces;
using Autorisation.Models;
using Autorisation.Repositories;

namespace Autorisation.Services
{
    public class UsersService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUsersRepository _usersRepository;
        private readonly IJwtProvider _jwtProvider;

        public UsersService(
            IUsersRepository usersRepository,
            IPasswordHasher passwordHasher,
            IJwtProvider jwtProvider)
        {
            _passwordHasher = passwordHasher;
            _usersRepository = usersRepository;
            _jwtProvider = jwtProvider;
        }

        public async Task Register(string userName, string email, string password)
        {
            if (await _usersRepository.EmailExists(email))
            {
                throw new Exception("Данный пользователь уже существует!");
            }

            var hashedPassword = _passwordHasher.Generate(password);
            var user = User.Create(Guid.NewGuid(), userName, hashedPassword, email);
            await _usersRepository.Add(user);
        }

        public async Task<string> Login(string email, string password)
        {
            var user = await _usersRepository.GetByEmail(email)
                       ?? throw new Exception("Пользователь не найден!");

            var result = _passwordHasher.Verify(password, user.Password);

            if (!result)
            {
                throw new Exception("Ошибка входа!");
            }

            var token = _jwtProvider.GenerateToken(user);
            return token;
        }
    }
}
