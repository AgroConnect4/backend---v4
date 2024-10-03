using agroApp.Application.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs;

namespace agroApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;

        public PostsController(IPostService postService)
        {
            _postService = postService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto request)
        {
            var postId = await _postService.CreatePostAsync(request);
            return CreatedAtAction(nameof(GetPostById), new { id = postId }, null);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(int id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllPostsByUserId(int userId)
        {
            var posts = await _postService.GetAllPostsByUserIdAsync(userId);
            return Ok(posts);
        }

        [HttpGet("type/{postType}")]
        public async Task<IActionResult> GetAllPostsByPostType(string postType)
        {
            var posts = await _postService.GetAllPostsByPostTypeAsync(postType);
            return Ok(posts);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdatePostDto request)
        {
            var post = await _postService.UpdatePostAsync(id, request);
            return Ok(post);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePost(int id)
        {
            await _postService.DeletePostAsync(id);
            return NoContent();
        }
    }
}