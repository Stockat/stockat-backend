using AutoMapper;
using Stockat.Core.DTOs.UserDTOs;
using Stockat.Core.Entities;
using Stockat.Core.Enums;
using Stockat.EF.Configurations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Service.MappingProfiles.UserMappingProfiles;

public class UserMappingProfile: Profile
{
    public UserMappingProfile()
    {
        CreateMap<User, UserReadDto>()
            .ForMember(dest => dest.IsApproved, opt => opt.MapFrom(src => src.IsApproved))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
            .AfterMap((src, dest) =>
            {
                var punishments = src.Punishments ?? new List<UserPunishment>();
                dest.CurrentPunishment = punishments
                    .Where(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                                (p.EndDate == null || p.EndDate > DateTime.UtcNow))
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PunishmentInfoDto
                    {
                        Type = p.Type.ToString(),
                        Reason = p.Reason,
                        EndDate = p.EndDate
                    })
                    .FirstOrDefault();

                dest.PunishmentHistory = punishments
                    .OrderByDescending(p => p.CreatedAt)
                    .Select(p => new PunishmentHistoryDto
                    {
                        Type = p.Type.ToString(),
                        Reason = p.Reason,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        IsActive = (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                                   (p.EndDate == null || p.EndDate > DateTime.UtcNow)
                    }).ToList();

                dest.Statistics = new UserStatisticsDto
                {
                    TotalProducts = src.Products?.Count ?? 0,
                    TotalServices = src.Services?.Count ?? 0,
                    TotalAuctions = src.CreatedAuctions?.Count ?? 0,
                    TotalPunishments = punishments.Count,
                    ActivePunishments = punishments.Count(p => (p.Type == PunishmentType.TemporaryBan || p.Type == PunishmentType.PermanentBan) &&
                                                           (p.EndDate == null || p.EndDate > DateTime.UtcNow))
                };
            });

        CreateMap<UserPunishment, PunishmentInfoDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate));

        CreateMap<UserPunishment, PunishmentHistoryDto>()
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()))
            .ForMember(dest => dest.Reason, opt => opt.MapFrom(src => src.Reason))
            .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => src.StartDate))
            .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => src.EndDate))
            .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src =>
                (src.Type == PunishmentType.TemporaryBan || src.Type == PunishmentType.PermanentBan) &&
                (src.EndDate == null || src.EndDate > DateTime.UtcNow)
            ));
    }
}
