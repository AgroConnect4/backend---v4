using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs;
using agroApp.Domain.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace agroApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BannersController : ControllerBase
    {
        private readonly IBannerService _bannerService;

        public BannersController(IBannerService bannerService) => _bannerService = bannerService;

        [HttpGet]
        public async Task<IActionResult> GetAllBanners()
        {
            var banners = await _bannerService.GetAllBannersAsync();
            return Ok(banners);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetBannerById(Guid id) // Changed to string
        {
            {
            var banner = await _bannerService.GetBannerByIdAsync(id);
            if (banner == null) return NotFound();
            return Ok(banner);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateBanner([FromBody] BannerCreateDto bannerDto)
        {
            Guid userId = GetUserId();

            var banner = new Banner
            {
                ImageUrl = bannerDto.ImageUrl,
                Title = bannerDto.Title,
                LinkUrl = bannerDto.LinkUrl,
                IsActive = bannerDto.IsActive,
                DisplayOrder = bannerDto.DisplayOrder,
                UserId = userId, // Set the UserId here
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _bannerService.CreateBannerAsync(banner, userId);
            return CreatedAtAction(nameof(GetBannerById), new { id = banner.Id }, banner);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBanner(Guid id, [FromBody] BannerUpdateDto bannerDto)
        {
            if (id != bannerDto.Id)
            {
                return BadRequest("Invalid banner ID or ID mismatch");
            }

            var bannerToUpdate = new Banner
            {
                Id = bannerDto.Id,
                ImageUrl = bannerDto.ImageUrl,
                Title = bannerDto.Title,
                LinkUrl = bannerDto.LinkUrl,
                IsActive = bannerDto.IsActive,
                DisplayOrder = bannerDto.DisplayOrder,
                UpdatedDate = DateTime.UtcNow
            };

            await _bannerService.UpdateBannerAsync(bannerToUpdate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBanner(Guid id) // Changed to string
        {
            {
                await _bannerService.DeleteBannerAsync(id);
                return NoContent();
            }
        }

        private Guid GetUserId()
        {
            string userIdString = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !Guid.TryParse(userIdString, out Guid userId))
            {
                throw new UnauthorizedAccessException("Invalid User ID"); // throw instead of returning unauthorized
            }
            return userId;
        }
    }
}