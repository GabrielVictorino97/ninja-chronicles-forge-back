# Kage no Tessen: Shinobi Legacy — Backend

API REST do jogo Kage no Tessen: Shinobi Legacy, MMORPG ninja baseado no universo de Naruto.

## Stack

- **.NET 8** / C# 12 / ASP.NET Core
- **FastEndpoints** (REPR pattern)
- **PostgreSQL** + Entity Framework Core 8
- **Redis** (StackExchange.Redis)
- **JWT** + Refresh Token
- **MediatR** (CQRS)
- **Swagger** (NSwag)
- **Serilog**

## Requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL + Redis (via Docker ou instalação local)

## Setup

```bash
# Subir PostgreSQL e Redis
docker compose up -d

# Rodar API (aplica migrations + seeds automaticamente)
cd src/KageNoTessen.Api
dotnet run
```

Swagger em `https://localhost:7259/swagger`.

### Credenciais de admin

- Email: `admin@kagenotessen.gg`
- Senha: `Admin123!`

> Para produção, altere o `Jwt:Secret`, `Admin:Email` e `Admin:Password` no `appsettings.json` ou via variáveis de ambiente.

## Estrutura

```
src/
├── KageNoTessen.Domain/         # Entidades, enums, value objects
├── KageNoTessen.Application/    # Commands, queries, handlers
├── KageNoTessen.Contracts/      # DTOs (Requests/Responses)
├── KageNoTessen.Infrastructure/ # EF Core, repositórios, serviços
└── KageNoTessen.Api/            # Endpoints FastEndpoints, Program.cs
```

## Endpoints

### Auth
| Método | Rota | Auth |
|--------|------|------|
| POST | `/api/auth/register` | Anônimo |
| POST | `/api/auth/login` | Anônimo |
| POST | `/api/auth/refresh` | Anônimo |
| GET | `/api/me` | Autenticado |

### Personagens
| Método | Rota | Auth |
|--------|------|------|
| POST | `/api/characters` | Autenticado |
| GET | `/api/characters/me` | Autenticado |
| GET | `/api/characters/{id}` | Anônimo |
| PUT | `/api/characters/me/attributes` | Autenticado |

### Vilas, Clãs, Elementos
| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/villages` | Anônimo |
| GET | `/api/bloodline-clans` | Anônimo |
| GET | `/api/elements` | Anônimo |
| POST | `/api/elements/{element}/learn` | Autenticado |

### Jutsus
| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/jutsus` | Anônimo |
| GET | `/api/jutsus/me` | Autenticado |
| POST | `/api/jutsus/{id}/learn` | Autenticado |
| POST | `/api/jutsus/{id}/equip` | Autenticado |

### Missões
| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/missions` | Anônimo |
| POST | `/api/missions/{id}/start` | Autenticado |
| POST | `/api/missions/{id}/complete` | Autenticado |

### Caçadas
| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/hunts/status` | Autenticado |
| POST | `/api/hunts/start` | Autenticado |
| POST | `/api/hunts/complete` | Autenticado |

### Loja e Inventário
| Método | Rota | Auth |
|--------|------|------|
| GET | `/api/shop/items` | Anônimo |
| POST | `/api/shop/buy/{id}` | Autenticado |
| GET | `/api/inventory` | Autenticado |

## Seeds

O seed inicial popula:
- Admin (SuperAdmin)
- 7 Vilas (Konoha, Suna, Kiri, Kumo, Iwa, Ame, Oto)
- 14 Clãs (Uchiha, Hyuga, Uzumaki, Senju, Nara, etc.)
- 46 Jutsus (básicos, elementais, kinjutsus, genjutsus, kuchiyoses)
- 25 Missões (Rank D a S, incluindo missões longas de horas)
- 10 Itens de loja
- 5 Localizações do mundo
- 4 Conquistas

## Migrations

```bash
# Criar
dotnet ef migrations add Nome \
  --project src/KageNoTessen.Infrastructure \
  --startup-project src/KageNoTessen.Api

# A API aplica automaticamente ao iniciar via `db.Database.MigrateAsync()`
```

## Integração com Frontend

Os DTOs em `KageNoTessen.Contracts` mantêm os mesmos formatos dos types TypeScript do frontend. JSON serializado em camelCase por padrão.
