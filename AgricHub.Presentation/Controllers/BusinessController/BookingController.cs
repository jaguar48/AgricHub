using AgricHub.BLL.Implementations.BusinessServices;
using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.BLL.Interfaces.IBusinessServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers.BusinessController
{

    [ApiController]
    [Route("/api/agrichub/Booking")]
    public class BookingController : ControllerBase
    {
        private readonly IConsultationService _ConsultationService;

       

        public BookingController(IConsultationService consultationService)
        {
            _ConsultationService = consultationService;


        }




        [HttpPost("book")]
        [Authorize]
        public async Task<IActionResult> BookConsultation([FromBody] ConsultationBookingRequest dto)
        {
            var result = await _ConsultationService.BookConsultationAsync(dto);
            return Ok(result);
        }



    }
}
