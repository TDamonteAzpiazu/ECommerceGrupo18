using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Repository;

namespace Users.API.Services
{
    public class UserService
    {
        private readonly UserRepository _repository;

        public UserService(UserRepository repository)
        {
            _repository = repository;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(request.Nombre) ||
                string.IsNullOrWhiteSpace(request.Apellido) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos.");

            // Verificar que el email no esté registrado
            var existing = await _repository.GetByEmailAsync(request.Email);
            if (existing != null)
                throw new BusinessRuleException("USR-001", $"El email '{request.Email}' ya está registrado.");

            // Crear usuario
            var user = new User
            {
                Id = Guid.NewGuid(),
                Nombre = request.Nombre,
                Apellido = request.Apellido,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password)
            };

            var created = await _repository.CreateAsync(user);

            return new RegisterResponse
            {
                Id = created.Id,
                Nombre = created.Nombre,
                Apellido = created.Apellido,
                Email = created.Email,
                FechaRegistro = created.FechaRegistro,
                Activo = created.Activo
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // Validar campos requeridos
            if (string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
                throw new BusinessRuleException("USR-002", "Los datos del usuario son inválidos.");

            var user = await _repository.GetByEmailAsync(request.Email);

            // Email no existe → 401
            if (user == null)
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");

            // Usuario bloqueado por fraude
            if (!user.Activo && user.IntentosFallidos < 3)
                throw new BusinessRuleException("USR-005", "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.");

            // Usuario bloqueado por intentos fallidos
            if (!user.Activo && user.IntentosFallidos >= 3)
                throw new BusinessRuleException("USR-004", "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.");

            // Password incorrecta
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                user.IntentosFallidos++;
                if (user.IntentosFallidos >= 3)
                    user.Activo = false;
                await _repository.UpdateAsync(user);
                throw new BusinessRuleException("USR-003", "Credenciales incorrectas.");
            }

            // Login exitoso, resetear intentos
            user.IntentosFallidos = 0;
            await _repository.UpdateAsync(user);

            return new LoginResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email
            };
        }

        public async Task<RegisterResponse> GetByIdAsync(Guid id)
        {
            var user = await _repository.GetByIdAsync(id);
            if (user == null)
                throw new NotFoundException("USR-007", "Usuario no encontrado.");

            return new RegisterResponse
            {
                Id = user.Id,
                Nombre = user.Nombre,
                Apellido = user.Apellido,
                Email = user.Email,
                FechaRegistro = user.FechaRegistro,
                Activo = user.Activo
            };
        }
    }
}