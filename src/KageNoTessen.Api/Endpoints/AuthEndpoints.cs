using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FastEndpoints;
using MediatR;
using KageNoTessen.Application.Auth;
using KageNoTessen.Application.Interfaces;
using KageNoTessen.Contracts.Auth;

namespace KageNoTessen.Api.Endpoints;

public class RegisterEndpoint : Endpoint<RegisterRequest, AuthResponse>
{
    public override void Configure()
    {
        Post("auth/register");
        AllowAnonymous();
        Description(d => d
            .WithName("Registrar")
            .WithSummary("Registra um novo usuário e retorna o token JWT")
            .Accepts<RegisterRequest>("application/json"));
    }

    public override async Task HandleAsync(RegisterRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
        {
            AddError(r => r.Email, "Email invalido.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }
        if (string.IsNullOrWhiteSpace(req.Name) || req.Name.Length < 2)
        {
            AddError(r => r.Name, "Nome deve ter pelo menos 2 caracteres.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }
        if (string.IsNullOrWhiteSpace(req.Password) || req.Password.Length < 4)
        {
            AddError(r => r.Password, "Senha deve ter pelo menos 4 caracteres.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cmd = new RegisterCommand(req.Email, req.Name, req.Password);
        var result = await Resolve<IMediator>().Send(cmd, ct);
        await SendOkAsync(result, ct);
    }
}

public class LoginEndpoint : Endpoint<LoginRequest, AuthResponse>
{
    public override void Configure()
    {
        Post("auth/login");
        AllowAnonymous();
        Description(d => d
            .WithName("Login")
            .WithSummary("Autentica o usuario e retorna o token JWT + refresh token")
            .Accepts<LoginRequest>("application/json"));
    }

    public override async Task HandleAsync(LoginRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || !req.Email.Contains('@'))
        {
            AddError(r => r.Email, "Email invalido.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }
        if (string.IsNullOrWhiteSpace(req.Password))
        {
            AddError(r => r.Password, "Senha obrigatoria.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var device = HttpContext.Request.Headers.UserAgent.ToString();
        var cmd = new LoginCommand(req.Email, req.Password, ip, device);
        var result = await Resolve<IMediator>().Send(cmd, ct);
        await SendOkAsync(result, ct);
    }
}

public class RefreshEndpoint : Endpoint<RefreshRequest, AuthResponse>
{
    public override void Configure()
    {
        Post("auth/refresh");
        AllowAnonymous();
        Description(d => d
            .WithName("RefreshToken")
            .WithSummary("Renova o token JWT usando o refresh token")
            .Accepts<RefreshRequest>("application/json"));
    }

    public override async Task HandleAsync(RefreshRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.AccessToken) || string.IsNullOrWhiteSpace(req.RefreshToken))
        {
            AddError("tokens", "AccessToken e RefreshToken sao obrigatorios.");
            await SendErrorsAsync(cancellation: ct);
            return;
        }

        var cmd = new RefreshCommand(req.AccessToken, req.RefreshToken);
        var result = await Resolve<IMediator>().Send(cmd, ct);
        await SendOkAsync(result, ct);
    }
}

public class MeEndpoint : EndpointWithoutRequest<UserDto>
{
    public override void Configure()
    {
        Get("me");
        Description(d => d
            .WithName("MeuPerfil")
            .WithSummary("Retorna o perfil do usuario logado"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userId = Guid.Parse(HttpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)!);
        var repo = Resolve<KageNoTessen.Application.Interfaces.IUserRepository>();
        var user = await repo.GetByIdAsync(userId, ct);
        if (user is null) { await SendNotFoundAsync(ct); return; }

        var role = HttpContext.User.FindFirstValue(ClaimTypes.Role)!;
        await SendOkAsync(new UserDto(user.Id, user.Email, user.Name, role, user.CreatedAt, user.LastLoginAt), ct);
    }
}
