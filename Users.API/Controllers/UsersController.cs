using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers
{
    /// <summary>
    /// API para gestión de usuarios del eCommerce
    /// </summary>
    [ApiController]
    [Route("api/users")]
    [Tags("Users")]
    public class UsersController : ControllerBase
    {
        private readonly UserService _service;

        public UsersController(UserService service)
        {
            _service = service;
        }

        /// <summary>
        /// Registrar un nuevo usuario
        /// </summary>
        /// <param name="request">Datos del usuario a registrar</param>
        /// <returns>Usuario registrado</returns>
        /// <response code="201">Usuario registrado correctamente</response>
        /// <response code="400">USR-002: Los datos del usuario son inválidos</response>
        /// <response code="409">USR-001: El email ya está registrado</response>
        /// <response code="500">USR-006: Error interno del servidor</response>
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            var result = await _service.RegisterAsync(request);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Autenticar un usuario con email y contraseña
        /// </summary>
        /// <param name="request">Credenciales del usuario</param>
        /// <returns>Datos del usuario autenticado</returns>
        /// <response code="200">Login exitoso</response>
        /// <response code="400">USR-002: Los datos del usuario son inválidos</response>
        /// <response code="401">USR-003: Credenciales incorrectas</response>
        /// <response code="403">USR-004: Usuario bloqueado por intentos fallidos / USR-005: Usuario bloqueado por fraude</response>
        /// <response code="500">USR-006: Error interno del servidor</response>
        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 401)]
        [ProducesResponseType(typeof(ErrorResponse), 403)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _service.LoginAsync(request);
            return Ok(result);
        }

        /// <summary>
        /// Obtener un usuario por su ID
        /// </summary>
        /// <remarks>Endpoint de uso interno. Lo llaman Orders.API y Notifications.API para validar que el usuario existe.</remarks>
        /// <param name="id">ID del usuario (GUID)</param>
        /// <returns>Datos del usuario</returns>
        /// <response code="200">Usuario encontrado correctamente</response>
        /// <response code="404">USR-007: Usuario no encontrado</response>
        /// <response code="500">USR-006: Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(RegisterResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }
    }
}