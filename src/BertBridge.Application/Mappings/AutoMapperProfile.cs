using AutoMapper;
using BertBridge.Application.Dtos;
using BertBridge.Domain.Device;
using BertBridge.Domain.ErrorDetection;

namespace BertBridge.Application.Mappings;

/// <summary>
/// AutoMapper 映射配置。定义 DTO ↔ Domain 之间的转换规则。
/// </summary>
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Device → DeviceDto
        CreateMap<Domain.Device.Device, DeviceDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id))
            .ForMember(d => d.Model, o => o.MapFrom(s => s.Info != null ? s.Info.Model : null))
            .ForMember(d => d.SerialNumber, o => o.MapFrom(s => s.Info != null ? s.Info.SerialNumber : null))
            .ForMember(d => d.FirmwareVersion, o => o.MapFrom(s => s.Info != null ? s.Info.FirmwareVersion : null))
            .ForMember(d => d.ConnectionString, o => o.MapFrom(s => s.Connection != null ? s.Connection.Value : null))
            .ForMember(d => d.ConnectionState, o => o.MapFrom(s => s.State.ToString()))
            .ForMember(d => d.LaneCount, o => o.MapFrom(s => s.Lanes.Count));

        // Lane → LaneDto
        CreateMap<Lane, LaneDto>()
            .ForMember(d => d.Id, o => o.MapFrom(s => s.Id));

        // EdResult → EdResultDto
        CreateMap<EdResult, EdResultDto>()
            .ForMember(d => d.Ber, o => o.MapFrom(s => s.Ber.Mantissa * Math.Pow(10, s.Ber.Exponent)))
            .ForMember(d => d.SnrDb, o => o.MapFrom(s => s.Snr != null ? s.Snr.Decibels : (double?)null))
            .ForMember(d => d.SignalDetected, o => o.MapFrom(s => s.LinkState.SignalDetected))
            .ForMember(d => d.CdrLocked, o => o.MapFrom(s => s.LinkState.CdrLocked))
            .ForMember(d => d.PllLocked, o => o.MapFrom(s => s.LinkState.PllLocked))
            .ForMember(d => d.DspReady, o => o.MapFrom(s => s.LinkState.DspReady))
            .ForMember(d => d.FecLocked, o => o.MapFrom(s => s.LinkState.FecLocked))
            .ForMember(d => d.AlignmentLocked, o => o.MapFrom(s => s.LinkState.AlignmentLocked));
    }
}
