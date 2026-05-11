using KageNoTessen.Application.Interfaces;

namespace KageNoTessen.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
