using AgricHub.DAL.Entities;
using AgricHub.DAL.Entities.Models;
using AgricHub.Shared.DTO_s.Request;
using AgricHub.Shared.DTO_s.Response;
using AutoMapper;
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

            CreateMap<ServicePackageRequest, ServicePackage>();
            CreateMap<ServicePackage, ServicePackageResponse>();

            // ✅ Business
            CreateMap<CreateBusinessRequest, Business>();
            CreateMap<Business, CreateBusinessRequest>();

            // ✅ Category
            CreateMap<CreateCategoryRequest, Category>();
            CreateMap<Category, CreateCategoryRequest>();

            // ✅ Consultation → ConsultationResponse (UPDATED TO USE GUIDs)
            CreateMap<Consultation, ConsultationResponse>()
                .ForMember(dest => dest.CustomerUserId, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.UserId : null))  // ✅ Map to UserId (GUID)
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : null))
                .ForMember(dest => dest.ConsultantUserId, opt => opt.MapFrom(src =>
                    src.Consultant != null ? src.Consultant.UserId : null))  // ✅ Map to UserId (GUID)
                .ForMember(dest => dest.ConsultantName, opt => opt.MapFrom(src =>
                    src.Consultant != null ? $"{src.Consultant.FirstName} {src.Consultant.LastName}" : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.ServiceName : null))
                .ForMember(dest => dest.PackageName, opt => opt.MapFrom(src =>
                    src.ServicePackage != null ? src.ServicePackage.PackageName : null))
                .ForMember(dest => dest.PendingAmount, opt => opt.Ignore());  // Set manually in service

            // ✅ ConsultationBookingRequest → Consultation
            CreateMap<ConsultationBookingRequest, Consultation>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerId, opt => opt.Ignore())
                .ForMember(dest => dest.ConsultantId, opt => opt.Ignore())
                .ForMember(dest => dest.SendbirdChannelUrl, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.EndAt, opt => opt.Ignore())
                .ForMember(dest => dest.IsCustomOffer, opt => opt.Ignore())
                .ForMember(dest => dest.CustomPrice, opt => opt.Ignore())
                .ForMember(dest => dest.CustomDurationMinutes, opt => opt.Ignore())
                .ForMember(dest => dest.DeliverablesPath, opt => opt.Ignore())
                .ForMember(dest => dest.CompletedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ConsultantNoShowReported, opt => opt.Ignore())
                .ForMember(dest => dest.CustomerNoShowReported, opt => opt.Ignore())
                .ForMember(dest => dest.Customer, opt => opt.Ignore())
                .ForMember(dest => dest.Consultant, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore())
                .ForMember(dest => dest.ServicePackage, opt => opt.Ignore());

            // ✅ ChatSession → ChatSessionResponse
            CreateMap<ChatSession, ChatSessionResponse>()
                .ForMember(dest => dest.CustomerUserId, opt => opt.MapFrom(src =>
                    src.Customer != null ? src.Customer.UserId : null))
                .ForMember(dest => dest.ConsultantUserId, opt => opt.MapFrom(src =>
                    src.Consultant != null ? src.Consultant.UserId : null))
                .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src =>
                    src.Customer != null ? $"{src.Customer.FirstName} {src.Customer.LastName}" : null))
                .ForMember(dest => dest.ConsultantName, opt => opt.MapFrom(src =>
                    src.Consultant != null ? $"{src.Consultant.FirstName} {src.Consultant.LastName}" : null))
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.ServiceName : null));

            // ✅ CustomOffer mappings
            CreateMap<CustomOfferRequest, CustomOffer>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore())
                .ForMember(dest => dest.CreatedAt, opt => opt.Ignore())
                .ForMember(dest => dest.AcceptedAt, opt => opt.Ignore())
                .ForMember(dest => dest.ChatSession, opt => opt.Ignore())
                .ForMember(dest => dest.Service, opt => opt.Ignore());

            CreateMap<CustomOffer, CustomOfferResponse>()
                .ForMember(dest => dest.ServiceName, opt => opt.MapFrom(src =>
                    src.Service != null ? src.Service.ServiceName : null));

            // ✅ WALLET MAPPINGS (NEW)
            // Wallet → WalletResponse (Customer)
            CreateMap<Wallet, WalletResponse>()
                .ForMember(dest => dest.UserId, opt => opt.MapFrom(src =>
                    src.CustomerId.HasValue && src.Customer != null
                        ? src.Customer.UserId
                        : src.Consultant != null ? src.Consultant.UserId : null))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src =>
                    src.CustomerId.HasValue && src.Customer != null
                        ? $"{src.Customer.FirstName} {src.Customer.LastName}"
                        : src.Consultant != null ? $"{src.Consultant.FirstName} {src.Consultant.LastName}" : null))
                .ForMember(dest => dest.UserType, opt => opt.MapFrom(src =>
                    src.CustomerId.HasValue ? "Customer" : "Consultant"));

          
        }
    }
}