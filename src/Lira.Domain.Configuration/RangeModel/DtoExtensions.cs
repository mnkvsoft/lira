using Lira.Domain.Configuration.PrettyParsers;
using Lira.Domain.Configuration.RangeModel.Dto;

namespace Lira.Domain.Configuration.RangeModel;

static class DtoExtensions
{
    public static long GetCapacity(this DataOptionsDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Capacity))
            throw new Exception("Field 'capacity' is required if filled 'start' field");

        if (!PrettyNumberParser<long>.TryParse(dto.Capacity, out long capacity))
            throw new Exception($"Field 'capacity' has not int value '{dto.Capacity}'");
        
        return capacity;
    }
}