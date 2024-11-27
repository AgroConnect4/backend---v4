using agroApp.API.Services;
using Microsoft.AspNetCore.Mvc;
using agroApp.API.DTOs;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System;
using agroApp.Domain.Entities;
using System.Threading.Tasks;
using agroApp.Infra.Data.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;

namespace agroApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PostsController : ControllerBase
    {
        private readonly IPostService _postService;
        private readonly ILogger<PostsController> _logger;
        private readonly IPostCommentService _postCommentService;

        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PostsController(IConfiguration configuration,
        IHttpContextAccessor httpContextAccessor,
        IPostService postService,
        IPostCommentService postCommentService,
        ILogger<PostsController> logger) // Inject ILogger
        {
            _postService = postService;
            _postCommentService = postCommentService;
            _httpContextAccessor = httpContextAccessor;
            _configuration = configuration;
            _logger = logger; // Assign the injected logger
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


        [HttpPost("post")]
        public async Task<IActionResult> CreatePost([FromBody] CreatePostDto request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var post = await _postService.CreatePostAsync(request, userId);
                return CreatedAtAction(nameof(GetPostById), new { id = post.Id }, post); // Return 201 Created with the created Post
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message }); //Improved error response
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating post.");
                return StatusCode(500, new { message = "An unexpected error occurred while creating the post." });
            }
        }
        
        [HttpPut("post/{id}")]
        public async Task<IActionResult> UpdatePost(Guid id, [FromBody] UpdatePostDto request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var updatedPost = await _postService.UpdatePostAsync(id, request, userId);
                if (updatedPost == null)
                {
                    return Unauthorized(); // Or NotFound(), depending on your error handling strategy
                }
                return Ok(updatedPost);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating post.");
                return StatusCode(500, "An unexpected error occurred while updating the post.");
            }
        }

        [HttpDelete("post/{id}")]
        public async Task<IActionResult> DeletePost(Guid id)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _postService.DeletePostAsync(id, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting post.");
                return StatusCode(500, "An unexpected error occurred while deleting the post.");
            }
        }

        [HttpPost("{postId}comment")]
        public async Task<IActionResult> CreateComment(Guid postId, [FromBody] CreatePostCommentDto createCommentDto)
        {
            try
            {
                var userId = GetUserIdFromToken();

                var comment = await _postCommentService.CreateCommentAsync(postId, createCommentDto, userId);
                return CreatedAtAction(nameof(GetCommentById), new { id = comment.Id }, comment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating comment.");
                return StatusCode(500, "An unexpected error occurred while creating the comment.");
            }
        }

        [HttpGet("comments/{id}")] 
        public async Task<IActionResult> GetCommentById(Guid id)
        {
            var comment = await _postCommentService.GetCommentByIdAsync(id);
            if (comment == null) return NotFound();
            return Ok(comment);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPostById(Guid id)
        {
            var post = await _postService.GetPostByIdAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            return Ok(post);
        }

        [HttpGet("{postId}/comments")]
        public async Task<IActionResult> GetAllCommentsByPostId(Guid postId)
        {
            try
            {
                var comments = await _postCommentService.GetAllCommentsByPostIdAsync(postId); // This method needs to be implemented in your service
                return Ok(comments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting comments for post ID {postId}", postId);
                return StatusCode(500, "An unexpected error occurred while retrieving comments.");
            }
        }

        [HttpPut("comments/{commentId}")]
        public async Task<IActionResult> UpdateComment(Guid commentId, [FromBody] UpdatePostCommentDto request)
        {
            try
            {
                var userId = GetUserIdFromToken();
                var updatedComment = await _postCommentService.UpdateCommentAsync(commentId, request, userId);
                if (updatedComment == null)
                {
                    return NotFound(); // Or Unauthorized(), depending on error handling
                }
                return Ok(updatedComment);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating comment.");
                return StatusCode(500, "An unexpected error occurred while updating the comment.");
            }
        }

        [HttpDelete("comments/{commentId}")]
        public async Task<IActionResult> DeleteComment(Guid commentId)
        {
            try
            {
                var userId = GetUserIdFromToken();
                await _postCommentService.DeleteCommentAsync(commentId, userId);
                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting comment.");
                return StatusCode(500, "An unexpected error occurred while deleting the comment.");
            }
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllPostsByUserId(Guid userId)
        {
            var posts = await _postService.GetAllPostsByUserIdAsync(userId);
            return Ok(posts);
        }

        [HttpGet("category/{categoryName}")] // Note que o parâmetro agora é categoryName (string)
        public async Task<IActionResult> GetAllPostsByCategoryName(string categoryName)
        {
            var posts = await _postService.GetAllPostsByCategoryNameAsync(categoryName);
            return Ok(posts);
        }

        [HttpPost("{postId}/report")]
        public async Task<IActionResult> ReportPost(Guid postId, [FromBody] ReportPostDto reportDto)
        {
            Guid userId = GetUserIdFromToken(); //Implement GetUserIdFromToken method
            var post = await _postService.ReportPostAsync(postId, userId, reportDto.Reason);
            if (post == null)
            {
                return NotFound();
            }
            return Ok(post);
        }

        [HttpPut("{postId}/activate")]
        public async Task<IActionResult> TogglePostActivation(Guid postId)
        {
            try
            {
                Guid userId = GetUserIdFromToken(); 
                await _postService.TogglePostActivationAsync(postId, userId);
                return NoContent(); // 204 No Content is appropriate for successful updates that don't return data.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling post activation for post ID {postId}", postId);
                return StatusCode(500, "An unexpected error occurred while toggling post activation."); // More informative error message
            }
        }

        [HttpGet("{postId}/reported")]
        public async Task<IActionResult> HasBeenReported(Guid postId)
        {
            bool reported = await _postService.HasBeenReportedAsync(postId);
            return Ok(reported);
        }

        [HttpGet("{postId}/reports")]
        public async Task<IActionResult> GetPostReports(Guid postId)
        {
            var reports = await _postService.GetPostReportsAsync(postId);
            return Ok(reports);
        }

        [HttpPost("{postId}/react")]
        public async Task<IActionResult> CreateReaction(Guid postId, [FromBody] CreateReactionDto reactionDto)
        {
            try
            {
                Guid userId = GetUserIdFromToken();
                var updatedPost = await _postService.CreateReactionAsync(postId, userId, reactionDto.ReactionType);
                return Ok(updatedPost);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Error creating reaction for post {postId}", postId);
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating reaction.");
                return StatusCode(500, "An unexpected error occurred.");
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            try
            {
                var posts = await _postService.GetAllPostsAsync();
                return Ok(posts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all posts.");
                return StatusCode(500, "An unexpected error occurred while retrieving posts.");
            }
        }
    }
}