using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
using GoogleApi.Entities.Search.Video.Common.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.BLL.MappingProfiles
{
   
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<CreateServiceRequest, Service>();
            CreateMap<Service, ViewServiceResponse>()
               .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.BusinessName));

            CreateMap<CreateBusinessRequest, Business>();
            CreateMap<Business, CreateBusinessRequest>();

            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Category, CreateCategoryRequest>();

          

        }
    }
  
}
