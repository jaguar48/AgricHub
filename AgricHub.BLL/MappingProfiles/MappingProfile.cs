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
            // ✅ Service
            CreateMap<CreateServiceRequest, Service>();
            CreateMap<Service, ViewServiceResponse>()
                .ForMember(dest => dest.BusinessName, opt => opt.MapFrom(src => src.Business.BusinessName));

            // ✅ Business
            CreateMap<CreateBusinessRequest, Business>();
            CreateMap<Business, CreateBusinessRequest>();

            // ✅ Category
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Category, CreateCategoryRequest>();

            // ✅ Consultation → ConsultationResponse
            CreateMap<Consultation, ConsultationResponse>()
                .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Customer.UserId))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.FirstName + " " + src.Customer.LastName))
                .ForMember(dest => dest.ConsultantId, opt => opt.MapFrom(src => src.Consultant.UserId))
                .ForMember(dest => dest.ConsultantName, opt => opt.MapFrom(src => src.Consultant.FirstName + " " + src.Consultant.LastName))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src => src.Service != null ? src.Service.ServiceName : null));





            CreateMap<ConsultationBookingRequest, Consultation>()
    .ForMember(dest => dest.CustomerId, opt => opt.Ignore())     // set after DB lookup
    .ForMember(dest => dest.ConsultantId, opt => opt.Ignore())   // set after DB lookup
    .ForMember(dest => dest.Id, opt => opt.Ignore())             // generated
    .ForMember(dest => dest.SendbirdChannelUrl, opt => opt.Ignore()) // set later
    .ForMember(dest => dest.Status, opt => opt.Ignore());        // defaults


            CreateMap<ChatSession, ChatSessionResponse>()
     .ForMember(dest => dest.CustomerUserId,
         opt => opt.MapFrom(src => src.Customer.UserId))
     .ForMember(dest => dest.ConsultantUserId,
         opt => opt.MapFrom(src => src.Consultant.UserId))
     .ForMember(dest => dest.CustomerName,
         opt => opt.MapFrom(src => $"{src.Customer.FirstName} {src.Customer.LastName}"))
     .ForMember(dest => dest.ConsultantName,
         opt => opt.MapFrom(src => $"{src.Consultant.FirstName} {src.Consultant.LastName}"))
     .ForMember(dest => dest.ServiceName,
         opt => opt.MapFrom(src => src.Service != null ? src.Service.ServiceName : null));


        }


    }
  
}
