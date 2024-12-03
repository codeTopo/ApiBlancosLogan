using ApiBlancosLogan.Models;
using ApiBlancosLogan.Request;
using ApiBlancosLogan.Tools;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ApiBlancosLogan.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly BlancosLoganContext _context;
        private readonly JwtService _jwtService;
        public UsersController(BlancosLoganContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        private List<string> ObtenerRolesUsuario(string email)
        {
            // Por defecto, todos los usuarios tienen el rol de "Usuario"
            List<string> roles = new List<string> { "Usuario" };
            // Definir listas de correos electrónicos para cada rol
            List<string> correosMaster = new List<string>
            {
                "codetopo.cc@gmail.com",
            };
            List<string> correosAdmin = new List<string>
            {
               "botlogan@gmail.com"
            };

            // Verificar si el correo pertenece a un "Master"
            if (correosMaster.Contains(email))
            {
                roles.Add("Master");
                roles.Add("Admin");
            }
            // Verificar si el correo pertenece a un "Admin"
            else if (correosAdmin.Contains(email))
            {
                roles.Add("Admin");
            }

            return roles;
        }

        // POST: Validar Login
        [HttpPost("validar")]
        public async Task<IActionResult> Post(LoginRequest model)
        {
            var respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al Conectar a la Base de Datos"
            };

            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .Select(x => new
                    {
                        Campo = x.Key,
                        Errores = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                respuestas.Mensaje = "Datos de entrada no válidos";
                respuestas.Data = errores;
                return BadRequest(respuestas);
            }

            // Estrategia de reintentos para la operación de base de datos
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Usuario? user = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == model.Email);
                        if (user == null)
                        {
                            respuestas.Mensaje = "Correo no encontrado";
                            return NotFound(respuestas);
                        }

                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password!));
                            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            if (hashedPassword != user.Password)
                            {
                                respuestas.Mensaje = "Contraseña incorrecta";
                                return Unauthorized(respuestas);
                            }
                        }

                        var roles = ObtenerRolesUsuario(model.Email!);
                        var token = _jwtService.GenerateToken(model.Email!, roles);
                        respuestas.Exito = 1;
                        respuestas.Mensaje = "Login exitoso";
                        respuestas.Data = new { token };

                        await transaction.CommitAsync();
                        return Ok(respuestas);
                    }
                    catch (DbUpdateException)
                    {
                        await transaction.RollbackAsync();
                        respuestas.Mensaje = "Error al conectar a la base de datos";
                        return StatusCode(StatusCodes.Status500InternalServerError, respuestas);
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        respuestas.Mensaje = "Ocurrió un error inesperado";
                        respuestas.Data = new { detalle = ex.Message };
                        return StatusCode(StatusCodes.Status500InternalServerError, respuestas);
                    }
                }
            });
        }

        // POST: Agregar Usuario
        [HttpPost("agregar")]
        public async Task<IActionResult> Agregar(AuthRequest model)
        {
            Respuestas respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al intentar agregar el usuario."
            };
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .Select(x => new
                    {
                        Campo = x.Key,
                        Errores = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                respuestas.Mensaje = "La solicitud contiene datos no válidos. Detalles:";
                respuestas.Data = errores;
                return BadRequest(respuestas);
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Usuario usuario = new Usuario
                        {
                            Email = model.Email!
                        };
                        using (SHA256 sha256 = SHA256.Create())
                        {
                            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password!));
                            string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                            usuario.Password = hashedPassword;
                        }
                        _context.Usuarios.Add(usuario);
                        await _context.SaveChangesAsync();
                        await transaction.CommitAsync();
                        var roles = new List<string> { "Usuario" };
                        var token = _jwtService.GenerateToken(model.Email!, roles);
                        respuestas.Exito = 1;
                        respuestas.Mensaje = "Usuario agregado correctamente.";
                        respuestas.Data = usuario.Email;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        respuestas.Mensaje = $"Error al intentar agregar el usuario. {ex.Message}";
                    }
                }
                return Ok(respuestas);
            });
        }

        // PUT: Editar Usuario
        [HttpPut("editar")]
        [Authorize(Roles = "Master,Admin,Usuario")]
        public async Task<IActionResult> Edit(AuthRequest model)
        {
            Respuestas respuestas = new Respuestas
            {
                Exito = 0,
                Mensaje = "Error al intentar editar el usuario."
            };
            if (!ModelState.IsValid)
            {
                var errores = ModelState
                    .Where(x => x.Value!.Errors.Any())
                    .Select(x => new
                    {
                        Campo = x.Key,
                        Errores = x.Value!.Errors.Select(e => e.ErrorMessage).ToList()
                    })
                    .ToList();

                respuestas.Mensaje = "La solicitud contiene datos no válidos. Detalles:";
                respuestas.Data = errores;
                return BadRequest(respuestas);
            }
            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using (var transaction = await _context.Database.BeginTransactionAsync())
                {
                    try
                    {
                        Usuario? userr = await _context.Usuarios.SingleOrDefaultAsync(u => u.Email == model.Email);

                        if (userr != null)
                        {
                            using (SHA256 sha256 = SHA256.Create())
                            {
                                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(model.Password!));
                                string hashedPassword = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                                userr.Password = hashedPassword;
                            }
                            await _context.SaveChangesAsync();
                            await transaction.CommitAsync();
                            respuestas.Exito = 1;
                            respuestas.Mensaje = "Usuario editado correctamente.";
                            respuestas.Data = userr;
                        }
                        else
                        {
                            respuestas.Mensaje = "Usuario no encontrado.";
                        }
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();
                        respuestas.Mensaje = $"Error al intentar editar el usuario. {ex.Message}";
                    }
                }

                return Ok(respuestas);
            });
        }

        // GET: Obtener Usuarios (Solo Master)
        [HttpGet]
        [Authorize(Roles = "Master")]
        public async Task<IActionResult> Get()
        {
            Respuestas respuestas = new()
            {
                Exito = 0,
                Mensaje = "Error en la conexión con la base de datos"
            };

            var strategy = _context.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                try
                {
                    var cliente = await _context.Usuarios.ToListAsync();
                    respuestas.Exito = 1;
                    respuestas.Mensaje = "Lista de Clientes";
                    respuestas.Data = cliente;
                }
                catch (Exception ex)
                {
                    respuestas.Exito = 0;
                    respuestas.Mensaje = ex.Message;
                    respuestas.Data = null;
                }

                return Ok(respuestas);
            });
        }


    }
}
