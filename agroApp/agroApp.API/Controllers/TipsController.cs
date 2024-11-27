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
    public class TipsController : ControllerBase
    {
        private readonly ITipService _tipService;

        public TipsController(ITipService tipService) => _tipService = tipService;

        [HttpGet]
        public async Task<IActionResult> GetAllTips()
        {
            var tips = await _tipService.GetAllTipsAsync();
            return Ok(tips);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTipById(Guid id) // Changed to string
        {
            {
            var tip = await _tipService.GetTipByIdAsync(id);
            if (tip == null) return NotFound();
            return Ok(tip);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateTip([FromBody] TipCreateDto tipDto)
        {
            Guid userId = GetUserId(); // Use your GetUserId method

            // Map the DTO to the Tip entity
            var tip = new Tip
            {
                Title = tipDto.Title,
                Description = tipDto.Description,
                IconUrl = tipDto.IconUrl,
                Category = tipDto.Category,
                IsPublished = tipDto.IsPublished,
                AuthorId = userId, // Set the AuthorId here
                CreatedDate = DateTime.UtcNow, // Set created date
                UpdatedDate = DateTime.UtcNow // Set updated date
            };

            await _tipService.CreateTipAsync(tip, userId);
            return CreatedAtAction(nameof(GetTipById), new { id = tip.Id }, tip);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateTip(Guid id, [FromBody] TipUpdateDto tipDto)
        {
             if (id != tipDto.Id)
            {
                return BadRequest("Invalid tip ID or ID mismatch");
            }

            //Map DTO to Tip entity for update. Note this assumes your service takes a Tip entity for update.  Adjust as needed.
            var tipToUpdate = new Tip
            {
                Id = tipDto.Id,
                Title = tipDto.Title,
                Description = tipDto.Description,
                IconUrl = tipDto.IconUrl,
                Category = tipDto.Category,
                IsPublished = tipDto.IsPublished,
                UpdatedDate = DateTime.UtcNow
            };

            await _tipService.UpdateTipAsync(tipToUpdate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteTip(Guid id) // Changed to string
        {
           {
            await _tipService.DeleteTipAsync(id);
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