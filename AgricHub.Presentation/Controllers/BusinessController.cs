using AgricHub.BLL.Implementations.UserServices.UserServices;
using AgricHub.BLL.Interfaces.IAgrichub_Services;
using AgricHub.BLL.Interfaces.IUserServices;
using AgricHub.Shared.DTO_s.Request;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Presentation.Controllers
{
    [ApiController]
    [Route("/api/agrichub/business")]
    public class BusinessController:ControllerBase
    {
        private readonly IBusiness_ConsultServices _business_ConsultServices;
        

        public BusinessController(IBusiness_ConsultServices businessServices)
        {
            _business_ConsultServices = businessServices;
          

        }

        [HttpPost("createBusiness")]

        public async Task<IActionResult> Createbusiness([FromBody] CreateBusinessRequest businessRequest)
        {

            var result = await _business_ConsultServices.AddBusiness(businessRequest);
            return Ok(result);
        }
        [HttpPost("createCategory")]

        public async Task<IActionResult> Createcategory([FromBody] CreateCategoryRequest categoryRequest)
        {

            var result = await _business_ConsultServices.AddCategory(categoryRequest);
            return Ok(result);
        }
    }

}
