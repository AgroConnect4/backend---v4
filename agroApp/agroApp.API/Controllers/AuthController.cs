using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using agroApp.Infra.Data.Repositories;

namespace agroApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly SignInManager<User> _signInManager;
        //private readonly UserManager<User> _userManager;
        private readonly IUserRepository _userRepository;

        public AuthController(IAuthService authService, SignInManager<User> signInManager, IUserRepository userRepository)
        {
            _authService = authService;
            //_userManager = userManager;
            _signInManager = signInManager;
            _userRepository = userRepository; 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            // Validação básica
            if (registerDto == null || string.IsNullOrWhiteSpace(registerDto.Username) ||
                string.IsNullOrWhiteSpace(registerDto.Email) || string.IsNullOrWhiteSpace(registerDto.Password))
            {
                return BadRequest("Todos os campos são obrigatórios.");
            }

            var result = await _authService.RegisterAsync(registerDto.Username, registerDto.Email, registerDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description).ToList();
                return BadRequest(new { Errors = errors });
            }
            return Ok("Usuário registrado com sucesso.");
        }

        // API.Controllers/AuthController.cs
        [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
            {
                // Validação básica
                if (string.IsNullOrWhiteSpace(loginDto.Email) || string.IsNullOrWhiteSpace(loginDto.Password))
                {
                    return BadRequest("Email e senha são obrigatórios.");
                }

                // Encontre o usuário pelo email
                var user = await _userRepository.GetByEmailAsync(loginDto.Email);

                // Se o usuário não existir, retorne Unauthorized
                if (user == null)
                {
                    return Unauthorized("Usuário não encontrado.");
                }

                // Use o SignInManager para verificar a senha (o ASP.NET Identity cuida da criptografia)
                var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

                // Se a senha estiver incorreta, retorne Unauthorized
                if (!result.Succeeded)
                {
                    return Unauthorized("Falha ao efetuar login. Verifique suas credenciais.");
                }

                // Se a senha estiver correta, gere o token JWT
                var token = await _authService.GenerateTokenAsync(user);
                return Ok(new { token });
            }
        
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto forgotPasswordDto)
        {
            // Validação básica
            if (string.IsNullOrWhiteSpace(forgotPasswordDto.Email))
            {
                return BadRequest("Email é obrigatório.");
            }

            try
            {
                await _authService.ForgotPasswordAsync(forgotPasswordDto.Email);
                return Ok("Instruções de recuperação de senha enviadas.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Método para validar o nome de usuário
        private bool IsValidUserName(string username)
        {
            return username.All(char.IsLetterOrDigit); // Verifica se todos os caracteres são letras ou números
        }
    }
}
