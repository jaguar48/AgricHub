using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AgricHub.Shared.DTO_s.Response
{

    public class PublicConsultantDto
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string? BusinessName { get; set; }
        public string? CountryId { get; set; }
        public string? StateId { get; set; }
        public string? AvatarUrl { get; set; }
        public bool IsVerified { get; set; }
        public double AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public int CompletedConsultations { get; set; }
    }

    public class PublicConsultantDetailDto : PublicConsultantDto
    {
        public List<PublicBusinessDto> Businesses { get; set; } = new();
    }

    public class PublicBusinessDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? ImagePath { get; set; }
        public List<PublicServiceDto> Services { get; set; } = new();
    }

    public class PublicServiceDto
    {
        public int Id { get; set; }
        public string ServiceName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? CategoryName { get; set; }
        public List<PublicPackageDto> Packages { get; set; } = new();
    }

    public class PublicPackageDto
    {
        public int Id { get; set; }
        public string PackageName { get; set; }
        public decimal Price { get; set; }
        public int DurationMinutes { get; set; }
        public string? Description { get; set; }
        public bool IncludesOnsiteVisit { get; set; }
    }
}