using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs; // Assuming you have this namespace
using agroApp.Domain.Entities;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using agroApp.Infra.Data.Repositories; //You might not need this here.
using Microsoft.Extensions.Logging; //You might not need this here.
using System.Linq;


namespace agroApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NewsController : ControllerBase
    {
        private readonly INewsService _newsService;

        public NewsController(INewsService newsService) => _newsService = newsService;

        [HttpGet]
        public async Task<IActionResult> GetAllNews()
        {
            var news = await _newsService.GetAllNewsAsync();
            return Ok(news);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetNewsById(Guid id) 
        {
            {
                var news = await _newsService.GetNewsByIdAsync(id);
                if (news == null) return NotFound();
                return Ok(news);
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateNews([FromBody] NewsCreateDto newsDto)
        {
            Guid userId = GetUserId();

            var news = new News
            {
                Title = newsDto.Title,
                Text = newsDto.Text,
                ImageUrl = newsDto.ImageUrl,
                IconUrl = newsDto.IconUrl,
                Summary = newsDto.Summary,
                SourceUrl = newsDto.SourceUrl,
                IsPublished = newsDto.IsPublished,
                AuthorId = userId,
                CreatedDate = DateTime.UtcNow,
                UpdatedDate = DateTime.UtcNow
            };

            await _newsService.CreateNewsAsync(news, userId);
            return CreatedAtAction(nameof(GetNewsById), new { id = news.Id }, news);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateNews(Guid id, [FromBody] NewsUpdateDto newsDto)
        {
            if (id != newsDto.Id)
            {
                return BadRequest("Invalid news ID or ID mismatch");
            }

            var newsToUpdate = new News
            {
                Id = newsDto.Id,
                Title = newsDto.Title,
                Text = newsDto.Text,
                ImageUrl = newsDto.ImageUrl,
                IconUrl = newsDto.IconUrl,
                Summary = newsDto.Summary,
                SourceUrl = newsDto.SourceUrl,
                IsPublished = newsDto.IsPublished,
                UpdatedDate = DateTime.UtcNow
            };

            await _newsService.UpdateNewsAsync(newsToUpdate);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteNews(Guid id)
        {
            {
                await _newsService.DeleteNewsAsync(id);
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