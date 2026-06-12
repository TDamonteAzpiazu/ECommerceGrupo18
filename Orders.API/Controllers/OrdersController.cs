using Microsoft.AspNetCore.Mvc;
using Orders.API.DTOs;
using Orders.API.Services;

namespace Orders.API.Controllers
{
    /// <summary>
    /// API para gestión de órdenes del eCommerce
    /// </summary>
    [ApiController]
    [Route("api/orders")]
    [Tags("Orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _service;

        public OrdersController(OrderService service)
        {
            _service = service;
        }

        /// <summary>
        /// Listar todas las órdenes con filtro opcional por usuario
        /// </summary>
        /// <param name="usuarioId">Filtrar por ID de usuario (opcional)</param>
        /// <returns>Lista de órdenes</returns>
        /// <response code="200">Lista de órdenes obtenida correctamente</response>
        /// <response code="500">ORD-007: Error interno del servidor</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponse>), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetAll([FromQuery] Guid? usuarioId)
        {
            var result = await _service.GetAllAsync(usuarioId);
            return Ok(result);
        }

        /// <summary>
        /// Obtener el detalle de una orden por su ID
        /// </summary>
        /// <param name="id">ID de la orden (GUID)</param>
        /// <returns>Detalle de la orden</returns>
        /// <response code="200">Orden encontrada correctamente</response>
        /// <response code="404">ORD-001: Orden no encontrada</response>
        /// <response code="500">ORD-007: Error interno del servidor</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(OrderResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse) ,404)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>
        /// Crear una nueva orden
        /// </summary>
        /// <param name="request">Datos de la orden a crear</param>
        /// <returns>Orden creada</returns>
        /// <response code="201">Orden creada correctamente</response>
        /// <response code="400">ORD-002: Los datos de la orden son inválidos</response>
        /// <response code="404">ORD-003: Usuario no encontrado / ORD-004: Producto no encontrado</response>
        /// <response code="422">ORD-005: Stock insuficiente para uno o más productos</response>
        /// <response code="500">ORD-007: Error interno del servidor</response>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponse), 201)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 422)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequest request)
        {
            var result = await _service.CreateAsync(request);
            return StatusCode(201, result);
        }

        /// <summary>
        /// Actualizar el estado de una orden
        /// </summary>
        /// <param name="id">ID de la orden (GUID)</param>
        /// <param name="request">Nuevo estado de la orden</param>
        /// <returns>Orden con estado actualizado</returns>
        /// <response code="200">Estado actualizado correctamente</response>
        /// <response code="400">ORD-002: Los datos de la orden son inválidos</response>
        /// <response code="404">ORD-001: Orden no encontrada</response>
        /// <response code="409">ORD-006: El estado de la orden no puede ser modificado</response>
        /// <response code="500">ORD-007: Error interno del servidor</response>
        [HttpPut("{id}/status")]
        [ProducesResponseType(typeof(UpdateOrderStatusResponse), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 400)]
        [ProducesResponseType(typeof(ErrorResponse), 404)]
        [ProducesResponseType(typeof(ErrorResponse), 409)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusRequest request)
        {
            var result = await _service.UpdateStatusAsync(id, request);
            return Ok(result);
        }

        /// <summary>
        /// Verificar si un producto tiene órdenes activas
        /// </summary>
        /// <remarks>Endpoint de uso interno. Lo llama Products.API antes de eliminar un producto.</remarks>
        /// <param name="productoId">ID del producto (GUID)</param>
        /// <returns>true si tiene órdenes activas, false si no</returns>
        /// <response code="200">Verificación realizada correctamente</response>
        /// <response code="500">ORD-007: Error interno del servidor</response>
        [HttpGet("product/{productoId}/active")]
        [ProducesResponseType(typeof(bool), 200)]
        [ProducesResponseType(typeof(ErrorResponse), 500)]
        public async Task<IActionResult> HasActiveOrders(Guid productoId)
        {
            var result = await _service.HasActiveOrdersForProductAsync(productoId);
            return Ok(result);
        }
    }
}