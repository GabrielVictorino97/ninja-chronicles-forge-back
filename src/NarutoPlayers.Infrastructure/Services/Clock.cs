using NarutoPlayers.Application.Interfaces;

namespace NarutoPlayers.Infrastructure.Services;

public class SystemClock : IClock
{
    public DateTime UtcNow => DateTime.UtcNow;
}
