using Blake3;

namespace AuraFlow.Core.Extensions;

public static class HashExtensions
{
    public static Guid ToGuid(this Hash hash)
    {
        return new Guid(hash.AsSpan()[..16]);
    }
}
