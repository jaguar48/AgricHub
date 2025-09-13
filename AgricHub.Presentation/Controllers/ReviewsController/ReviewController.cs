using AgricHub.BLL.Interfaces.IRatingServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("api/agrichub/reviews")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewService _reviewService;

        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

       
        [HttpPost("add")]
        [Authorize] 
        public async Task<IActionResult> AddReview([FromBody] CreateReviewRequest request)
        {
            var review = await _reviewService.AddReviewAsync(request);
            return Ok(new
            {
                success = true,
                message = "Review added successfully.",
                data = review
            });
        }

        
        [HttpGet("consultant/{consultantId}")]
        public async Task<IActionResult> GetConsultantReviews(int consultantId)
        {
            var reviews = await _reviewService.GetReviewsForConsultantAsync(consultantId);
            return Ok(reviews);
        }

       
        [HttpGet("consultant/{consultantId}/rating")]
        public async Task<IActionResult> GetAverageRating(int consultantId)
        {
            var avg = await _reviewService.GetAverageRatingForConsultantAsync(consultantId);
            return Ok(new { consultantId, averageRating = avg });
        }
    }
}
