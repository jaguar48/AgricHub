using AgricHub.DAL.Entities;
using AgricHub.Shared.DTO_s.Request;
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
            

            CreateMap<CreateBusinessRequest, Business>();
            CreateMap<Business, CreateBusinessRequest>();

        }
    }
}
