using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using agroApp.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using agroApp.Infra.Data.Repositories;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using Microsoft.AspNetCore.Identity;
using agroApp.Domain.Entities;
using agroApp.Infra.Data.Context;
using agroApp.Infra.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace agroApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserProfileController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<User> _userManager;
        private readonly IProfileService _profileService;
        private readonly ILogger<UserProfileController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor; // Inject HttpContextAccessor

        public UserProfileController(
            IConfiguration configuration,
            UserManager<User> userManager,
            IProfileService profileService, 
            IUserRepository userRepository,
            AppDbContext context,
            IHttpContextAccessor httpContextAccessor,
            ILogger<UserProfileController> logger)
        {
            _profileService = profileService;
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _configuration = configuration;
            _userRepository = userRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        private Guid GetUserIdFromToken()
        {
            var token = _httpContextAccessor.HttpContext?.Request?.Headers["Authorization"].FirstOrDefault()?.Split(" ").LastOrDefault();

            if (string.IsNullOrEmpty(token))
            {
                _logger.LogError("Token JWT não encontrado no cabeçalho de autorização.");
                throw new UnauthorizedAccessException("Token JWT ausente.");
            }

            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidAudience = _configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"])),
                    ClockSkew = TimeSpan.Zero
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out validatedToken);

                var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier); // Usar NameIdentifier
                if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId)) 
                {
                    _logger.LogError("Claim de ID de usuário inválida ou ausente no token JWT.");
                    throw new UnauthorizedAccessException("ID de usuário inválida no token.");
                }

                return userId; // Agora retorna Guid
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Erro ao validar o token JWT.");
                throw new UnauthorizedAccessException("Token JWT inválido.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro inesperado ao recuperar o ID do usuário do token JWT.");
                throw;
            }
        }

       [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUserProfile(Guid userId, [FromBody] UpdateProfileDto updateProfileDto)
        {
            //Console.WriteLine($"Dados recebidos: {JsonConvert.SerializeObject(updateProfileDto)}");
            
            if (!ModelState.IsValid) return BadRequest(ModelState);

            // Retrieve the user profile by ID
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null) return NotFound("Perfil não encontrado.");

            Guid userIdFromToken = GetUserIdFromToken();
            if(userIdFromToken != userId){
                return Unauthorized("Você não tem permissão para editar este perfil.");
            }

            // Update profile properties (only include properties you want to update)
            profile.Name = updateProfileDto.Name ?? profile.Name;
            profile.Bio = updateProfileDto.Bio ?? profile.Bio;
            profile.ProfilePicture = updateProfileDto.ProfilePicture ?? profile.ProfilePicture;
            profile.CoverPicture = updateProfileDto.CoverPicture ?? profile.CoverPicture;
            profile.Description = updateProfileDto.Description ?? profile.Description;
            profile.PhoneNumber = updateProfileDto.PhoneNumber ?? profile.PhoneNumber;
            profile.Website = updateProfileDto.Website ?? profile.Website;
            profile.Certifications = updateProfileDto.Certifications ?? profile.Certifications;
            profile.ProductsOffered = updateProfileDto.ProductsOffered ?? profile.ProductsOffered;


            await _profileService.UpdateProfileAsync(profile);
            return Ok();
        }

        [HttpGet("myprofile")]
        public async Task<IActionResult> GetCurrentUserProfile()
        {
            try
            {
                Guid userId = GetUserIdFromToken(); // Get userId from token

                var profile = await _profileService.GetProfileAsync(userId);
                if (profile == null)
                {
                    return NotFound("Profile not found.");
                }

                var profileDto = new UserProfileDto
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    Bio = profile.Bio,
                    ProfilePicture = profile.ProfilePicture,
                    CoverPicture = profile.CoverPicture,
                    Description = profile.Description,
                    PhoneNumber = profile.PhoneNumber,
                    Website = profile.Website,
                    AverageRating = profile.AverageRating,
                    Certifications = profile.Certifications,
                    ProductsOffered = profile.ProductsOffered,
                    ContactMethods = profile.ContactMethods.Select(cm => new ContactMethodDto
                    {
                        Type = cm.Type,
                        UrlOrNumber = cm.UrlOrNumber
                    }).ToList(),
                    Farms = profile.Farms.Select(f => new FarmDto
                    {
                        Name = f.Name,
                        Location = f.Location
                    }).ToList(),
                    Specializations = profile.Specializations.Select(s => new SpecializationDto
                    {
                        Name = s.Name,
                        Description = s.Description
                    }).ToList(),
                    Portfolio = profile.Portfolio.Select(p => new PortfolioItemDto
                    {
                        Title = p.Title,
                        Description = p.Description,
                        ImageUrl = p.ImageUrl,
                        VideoUrl = p.VideoUrl
                    }).ToList()
                };

                return Ok(profileDto);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogError(ex, "Unauthorized access attempt.");
                return Unauthorized(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        [HttpGet("{userId}")]
        public async Task<IActionResult> GetUserProfile(Guid userId)
        {
            try
            {
                var profile = await _profileService.GetProfileAsync(userId);
                if (profile == null) return NotFound("Profile not found.");

                var profileDto = new UserProfileDto
                {
                    Id = profile.Id,
                    Name = profile.Name,
                    Bio = profile.Bio,
                    ProfilePicture = profile.ProfilePicture,
                    CoverPicture = profile.CoverPicture,
                    Description = profile.Description,
                    PhoneNumber = profile.PhoneNumber,
                    Website = profile.Website,
                    AverageRating = profile.AverageRating,
                    Certifications = profile.Certifications, // Map lists as well
                    ProductsOffered = profile.ProductsOffered,
                    ContactMethods = profile.ContactMethods.Select(cm => new ContactMethodDto
                    {
                        Type = cm.Type,
                        UrlOrNumber = cm.UrlOrNumber
                    }).ToList(), // Example mapping for ContactMethods - adjust as needed
                    Farms = profile.Farms.Select(f => new FarmDto 
                    {
                        Name = f.Name,
                        Location = f.Location
                    }).ToList(), // Example mapping for Farms - adjust as needed
                    Specializations = profile.Specializations.Select(s => new SpecializationDto
                    {
                        Name = s.Name,
                        Description = s.Description
                    }).ToList(), // Example mapping for Specializations - adjust as needed
                    Portfolio = profile.Portfolio.Select(p => new PortfolioItemDto
                    {
                        Title = p.Title,
                        Description = p.Description,
                        ImageUrl = p.ImageUrl,
                        VideoUrl = p.VideoUrl
                    }).ToList() // Example mapping for PortfolioItems - adjust as needed

                };

                return Ok(profileDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user profile.");
                return StatusCode(500, new { message = "Internal Server Error" });
            }
        }

        // Endpoint for managing Farms
        [HttpPost("farms")]
        public async Task<IActionResult> AddFarm([FromBody] FarmDto farmDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _profileService.GetProfileAsync(userId);
            if (profile == null) return NotFound("Profile not found.");

            var farm = new Farm
            {
                Name = farmDto.Name,
                Location = farmDto.Location,
                Profile = profile 
            };

            _context.Farms.Add(farm);
            await _context.SaveChangesAsync();
            return Ok(farm);
        }


        [HttpPut("farms/{farmId}")]
        public async Task<IActionResult> UpdateFarm(Guid farmId, [FromBody] FarmDto farmDto)
        {
            Guid userId = GetUserIdFromToken();

            var farm = await _context.Farms.Include(f => f.Profile).FirstOrDefaultAsync(f => f.Id == farmId);
            if (farm == null) return NotFound("Farm not found.");
            if (farm.Profile.UserId != userId) return Unauthorized("You are not authorized to update this farm.");

            farm.Name = farmDto.Name;
            farm.Location = farmDto.Location;
            await _context.SaveChangesAsync();
            return Ok(farm);
        }

        [HttpDelete("farms/{farmId}")]
        public async Task<IActionResult> DeleteFarm(Guid farmId)
        {
            Guid userId = GetUserIdFromToken();
            var farm = await _context.Farms.Include(f => f.Profile).FirstOrDefaultAsync(f => f.Id == farmId);
            if (farm == null) return NotFound("Farm not found.");
            if (farm.Profile.UserId != userId) return Unauthorized("You are not authorized to delete this farm.");

            _context.Farms.Remove(farm);
            await _context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("profile/contactmethods")]
        public async Task<IActionResult> AddContactMethodToProfile([FromBody] ContactMethodDto contactMethodDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.User).FirstOrDefaultAsync(p => p.User.Id == userId);
            if (profile == null) return NotFound("Profile not found.");

            var contactMethod = new ContactMethod
            {
                Type = contactMethodDto.Type,
                UrlOrNumber = contactMethodDto.UrlOrNumber,
                Profile = profile
            };

            _context.ContactMethods.Add(contactMethod);
            await _context.SaveChangesAsync();
            return Ok(contactMethod);
        }

        [HttpPut("profile/contactmethods/{contactMethodId}")]
        public async Task<IActionResult> UpdateContactMethodInProfile(Guid contactMethodId, [FromBody] ContactMethodDto contactMethodDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.ContactMethods).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var contactMethod = profile.ContactMethods.FirstOrDefault(cm => cm.Id == contactMethodId);
            if (contactMethod == null) return NotFound("Contact method not found.");

            contactMethod.Type = contactMethodDto.Type;
            contactMethod.UrlOrNumber = contactMethodDto.UrlOrNumber;
            await _context.SaveChangesAsync();
            return Ok(contactMethod);
        }

        [HttpDelete("profile/contactmethods/{contactMethodId}")]
        public async Task<IActionResult> DeleteContactMethodFromProfile(Guid contactMethodId)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.ContactMethods).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var contactMethod = profile.ContactMethods.FirstOrDefault(cm => cm.Id == contactMethodId);
            if (contactMethod == null) return NotFound("Contact method not found.");

            _context.ContactMethods.Remove(contactMethod);
            await _context.SaveChangesAsync();
            return Ok();
        }


        // Specializations

        [HttpPost("profile/specializations")]
        public async Task<IActionResult> AddSpecializationToProfile([FromBody] SpecializationDto specializationDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.User).FirstOrDefaultAsync(p => p.User.Id == userId);
            if (profile == null) return NotFound("Profile not found.");

            var specialization = new Specialization
            {
                Name = specializationDto.Name,
                Description = specializationDto.Description,
                Profile = profile
            };

            _context.Specializations.Add(specialization);
            await _context.SaveChangesAsync();
            return Ok(specialization);
        }


        [HttpPut("profile/specializations/{specializationId}")]
        public async Task<IActionResult> UpdateSpecializationInProfile(Guid specializationId, [FromBody] SpecializationDto specializationDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.Specializations).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var specialization = profile.Specializations.FirstOrDefault(s => s.Id == specializationId);
            if (specialization == null) return NotFound("Specialization not found.");

            specialization.Name = specializationDto.Name;
            specialization.Description = specializationDto.Description;
            await _context.SaveChangesAsync();
            return Ok(specialization);
        }

        [HttpDelete("profile/specializations/{specializationId}")]
        public async Task<IActionResult> DeleteSpecializationFromProfile(Guid specializationId)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.Specializations).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var specialization = profile.Specializations.FirstOrDefault(s => s.Id == specializationId);
            if (specialization == null) return NotFound("Specialization not found.");

            _context.Specializations.Remove(specialization);
            await _context.SaveChangesAsync();
            return Ok();
        }



        // Portfolio Items

        [HttpPost("profile/portfolio")]
        public async Task<IActionResult> AddPortfolioItemToProfile([FromBody] PortfolioItemDto portfolioItemDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.User).FirstOrDefaultAsync(p => p.User.Id == userId);
            if (profile == null) return NotFound("Profile not found.");

            var portfolioItem = new PortfolioItem
            {
                Title = portfolioItemDto.Title,
                Description = portfolioItemDto.Description,
                ImageUrl = portfolioItemDto.ImageUrl,
                VideoUrl = portfolioItemDto.VideoUrl,
                Profile = profile
            };

            _context.PortfolioItems.Add(portfolioItem);
            await _context.SaveChangesAsync();
            return Ok(portfolioItem);
        }


        [HttpPut("profile/portfolio/{portfolioItemId}")]
        public async Task<IActionResult> UpdatePortfolioItemInProfile(Guid portfolioItemId, [FromBody] PortfolioItemDto portfolioItemDto)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.Portfolio).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var portfolioItem = profile.Portfolio.FirstOrDefault(p => p.Id == portfolioItemId);
            if (portfolioItem == null) return NotFound("Portfolio item not found.");

            portfolioItem.Title = portfolioItemDto.Title;
            portfolioItem.Description = portfolioItemDto.Description;
            portfolioItem.ImageUrl = portfolioItemDto.ImageUrl;
            portfolioItem.VideoUrl = portfolioItemDto.VideoUrl;
            await _context.SaveChangesAsync();
            return Ok(portfolioItem);
        }

        [HttpDelete("profile/portfolio/{portfolioItemId}")]
        public async Task<IActionResult> DeletePortfolioItemFromProfile(Guid portfolioItemId)
        {
            Guid userId = GetUserIdFromToken();
            var profile = await _context.Profiles.Include(p => p.Portfolio).FirstOrDefaultAsync(p => p.UserId == userId);
            if (profile == null) return NotFound("Profile not found.");

            var portfolioItem = profile.Portfolio.FirstOrDefault(p => p.Id == portfolioItemId);
            if (portfolioItem == null) return NotFound("Portfolio item not found.");

            _context.PortfolioItems.Remove(portfolioItem);
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}