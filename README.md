# Portfolio Web API — WPFW opdracht 4

Deze ASP.NET Core Minimal API beheert **projecten** en **blogposts** voor mijn persoonlijke portfoliosite. Voor WPFW opdracht 4 is de bestaande API uitgebreid met ASP.NET Core Identity, cookie-authenticatie, rolgebaseerde autorisatie en xUnit-integratietests.

**Repository:** https://github.com/leogordijn1-max/WebApplication1

## 1. Technologie en structuur

- C# en ASP.NET Core Minimal API (.NET 10)
- Entity Framework Core en SQL Server Express
- ASP.NET Core Identity met de rollen `Gast`, `Editor` en `Admin`
- xUnit en `Microsoft.AspNetCore.Mvc.Testing` voor HTTP-integratietests
- SQLite in-memory voor een afzonderlijke testdatabase
- Vertical Slice Architecture met endpoints, handlers en DTO's per bewerking

Belangrijke bestanden en mappen:

| Bestand of map | Functie |
|---|---|
| `WebApplication1/Program.cs` | Registreert services, middleware, policies en endpoints |
| `WebApplication1/data/AppDbContext.cs` | EF Core-context voor projecten, blogposts en Identity |
| `WebApplication1/data/Seed/RoleSeeder.cs` | Maakt ontbrekende rollen aan bij het starten |
| `WebApplication1/Features/Authentication/` | Registratie en login |
| `WebApplication1/Features/Projects/` | Projecten opvragen, toevoegen, wijzigen en verwijderen |
| `WebApplication1/Features/Blogposts/` | Blogposts opvragen, toevoegen, wijzigen en verwijderen |
| `WebApplication1.Tests/Infrastructure/CustomWebApplicationFactory.cs` | Start een testapplicatie met SQLite |
| `WebApplication1.Tests/Infrastructure/TestUsers.cs` | Maakt testgebruikers en logt deze in |

## 2. Benodigdheden

1. **.NET 10 SDK**.
2. **SQL Server Express**, met een bereikbare SQL Server-instantie.
3. **EF Core CLI**, indien nog niet geïnstalleerd:

   ```powershell
   dotnet tool install --global dotnet-ef --version "10.0.*"
   ```

4. Visual Studio of een andere geschikte .NET-ontwikkelomgeving.

De huidige `WebApplication1/appsettings.json` gebruikt een Windows-authenticatieverbinding naar `PortfolioDb` op `localhost\SQLEXPRESS`. Pas deze lokaal aan als de naam van de SQL Server-instantie anders is.

> **Let op:** zet geen wachtwoorden, API-sleutels of sessiecookies in de repository. `TrustServerCertificate=True` is hier uitsluitend bedoeld voor lokale ontwikkeling.

## 3. Database voorbereiden

Open een terminal in de API-projectmap (`WebApplication1/`, waar het API-`.csproj`-bestand staat) en voer uit:

```powershell
dotnet restore
dotnet ef database update
```

Met `dotnet ef database update` worden de bestaande EF Core-migraties toegepast, inclusief de Identity-tabellen. Bij het starten van de applicatie maakt `RoleSeeder` de rollen `Gast`, `Editor` en `Admin` aan als deze nog niet bestaan.

De tests gebruiken een andere aanpak: in de `Testing`-omgeving wordt een tijdelijke SQLite-database aangemaakt met `EnsureCreatedAsync()`.

## 4. API starten

Ga vanuit de solution-hoofdmap naar het API-project en start de applicatie:

```powershell
cd WebApplication1
dotnet run
```

Open vervolgens het lokale adres dat in de terminal wordt getoond, met `/swagger` erachter. Swagger is beschikbaar wanneer de applicatie in de omgeving `Development` draait. Voor HTTPS kan een lokaal ontwikkelcertificaat nodig zijn.

## 5. Authenticatie en rollen

### Registreren

`POST /auth/register` gebruikt `UserManager<User>` om een gebruiker aan te maken. Identity beheert de wachtwoordvalidatie en bewaart wachtwoorden als hashes, niet als platte tekst.

Iedere nieuwe gebruiker krijgt uitsluitend de rol `Gast`.

Voorbeeld van een **lokaal testverzoek**:

```json
{
  "email": "gast-voorbeeld@example.com",
  "password": "KiesZelfEenSterkTestWachtwoord!1"
}
```

### Inloggen

`POST /auth/login` controleert de inloggegevens met `SignInManager<User>`. Bij een geslaagde login ontvangt de browser een authenticatiecookie. Na een rolwijziging moet de gebruiker opnieuw inloggen zodat de nieuwe rol in de authenticatiecontext wordt opgenomen.

### Rechten

| Rol | Projecten en blogposts lezen | Aanmaken, wijzigen, verwijderen |
|---|---|---|
| `Gast` | Ja | Nee |
| `Editor` | Ja | Ja |
| `Admin` | Ja | Ja |
| Niet ingelogd | Ja | Nee |

In `Program.cs` vereist de policy `CanEditPortfolio` de rol `Editor` of `Admin`. Alle zes schrijf-endpoints gebruiken `.RequireAuthorization("CanEditPortfolio")`. De GET-endpoints zijn openbaar.

- `401 Unauthorized`: geen geldige aanmelding bij een schrijfverzoek.
- `403 Forbidden`: wel aangemeld, maar niet als Editor of Admin.

Voor de cookie-authenticatie zijn redirects naar login- of toegang-geweigerdpagina's vervangen door deze HTTP-statuscodes.

### Lokaal een Editor of Admin aanmaken

Een nieuwe gebruiker krijgt altijd eerst de rol `Gast`. Voor deze opdracht kan de rol `Editor` of `Admin` vervolgens **handmatig in de lokale database** worden toegekend. Dit is geen publiek API-endpoint.

1. Registreer eerst een testaccount via `POST /auth/register`.
2. Open in Visual Studio **SQL Server Object Explorer**, of gebruik SQL Server Management Studio.
3. Selecteer de database `PortfolioDb`.
4. Voer onderstaande query uit, met het juiste e-mailadres en de gewenste rol.

```sql
DECLARE @Email nvarchar(256) = N'editor-voorbeeld@example.com';
DECLARE @Rol nvarchar(256) = N'Editor'; -- of N'Admin'

IF @Rol NOT IN (N'Editor', N'Admin')
    THROW 50001, 'Alleen Editor of Admin is hier toegestaan.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Email = @Email)
    THROW 50002, 'Gebruiker niet gevonden. Registreer eerst.', 1;

IF NOT EXISTS (SELECT 1 FROM dbo.AspNetRoles WHERE Name = @Rol)
    THROW 50003, 'Rol niet gevonden. Start de API voor RoleSeeder.', 1;

INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
SELECT u.Id, r.Id
FROM dbo.AspNetUsers u
CROSS JOIN dbo.AspNetRoles r
WHERE u.Email = @Email
  AND r.Name = @Rol
  AND NOT EXISTS (
      SELECT 1
      FROM dbo.AspNetUserRoles ur
      WHERE ur.UserId = u.Id
        AND ur.RoleId = r.Id
  );

SELECT u.Email, r.Name AS Rol
FROM dbo.AspNetUsers u
JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
WHERE u.Email = @Email;
```

De bestaande rol `Gast` blijft naast `Editor` of `Admin` staan. Dat is toegestaan: de autorisatiepolicy controleert of de gebruiker **minstens één** van de vereiste rollen heeft.

Log na de roltoewijzing opnieuw in. In de definitieve `RoleSeeder` staat geen automatische rolpromotie van specifieke accounts. Tijdens de ontwikkeling is `UserManager.AddToRoleAsync` gebruikt om lokale testaccounts tijdelijk een extra rol te geven; die promotiecode is daarna verwijderd.

## 6. API-routes

| Methode | Route | Toegang | Mogelijke resultaten |
|---|---|---|---|
| GET | `/projects` | Iedereen | `200` |
| GET | `/projects/{id}` | Iedereen | `200`, `404` |
| POST | `/projects` | Editor / Admin | `201`, `400`, `401`, `403` |
| PUT | `/projects/{id}` | Editor / Admin | `200`, `400`, `401`, `403`, `404` |
| DELETE | `/projects/{id}` | Editor / Admin | `204`, `401`, `403`, `404` |
| GET | `/blogposts` | Iedereen | `200` |
| GET | `/blogposts/{id}` | Iedereen | `200`, `404` |
| POST | `/blogposts` | Editor / Admin | `201`, `400`, `401`, `403` |
| PUT | `/blogposts/{id}` | Editor / Admin | `200`, `400`, `401`, `403`, `404` |
| DELETE | `/blogposts/{id}` | Editor / Admin | `204`, `401`, `403`, `404` |
| POST | `/auth/register` | Iedereen | `201`, `400` |
| POST | `/auth/login` | Iedereen | `200` + cookie, `401` |

Bij de schrijfroutes worden de rechten gecontroleerd **voordat** de handler wordt uitgevoerd. Daarom kan een niet-ingelogde gebruiker `401` krijgen, ook als het opgegeven ID niet bestaat.

### Invoervalidatie

Bij projecten zijn `Titel`, `Beschrijving` en `Categorie` verplicht. `GitHubUrl` moet een absolute HTTP(S)-URL zijn. Bij blogposts zijn `Titel`, `Inhoud` en een ingevulde `Publicatiedatum` verplicht. Ongeldige invoer bij POST/PUT geeft `400 Bad Request`.

## 7. Automatische tests

Het aparte project `WebApplication1.Tests` bevat xUnit-integratietests voor de CRUD-routes, registratie, login, toegangsrechten en foutgevallen.

Voer vanuit de **solution-hoofdmap** uit:

```powershell
dotnet restore
dotnet test
```

De repository bevat `global.json` met de keuze voor `Microsoft.Testing.Platform`, omdat dit project xUnit v3 op .NET 10 gebruikt.

Bij een eerdere lokale uitvoering zijn **40 van de 40 tests geslaagd**. Het actuele resultaat kan de beoordelaar zelf controleren door de tests uit te voeren.

Belangrijke testbestanden:

- `WebApplication1.Tests/Projects/ProjectEndpointTests.cs`
- `WebApplication1.Tests/Blogposts/BlogpostEndpointTests.cs`
- `WebApplication1.Tests/Authentication/AuthEndpointTests.cs`
- `WebApplication1.Tests/Infrastructure/TestUsers.cs`
- `WebApplication1.Tests/Infrastructure/CustomWebApplicationFactory.cs`

### Hoe werkt de testdatabase?

`CustomWebApplicationFactory` start de applicatie met de environment `Testing` en vervangt de SQL Server-configuratie van `AppDbContext` door SQLite in-memory (`Data Source=:memory:`). Testgegevens staan daardoor los van `PortfolioDb`.

Met `WebApplicationFactory<Program>` voeren de tests HTTP-verzoeken uit tegen de applicatie. De tests controleren statuscodes en responsegegevens. Waar nodig controleren ze ook of gegevens in de testdatabase zijn aangemaakt, gewijzigd of verwijderd.

Er zijn daarnaast tests voor:

- Ongeldige invoer (`400`)
- Niet-bestaande ID's (`404`)
- Niet-ingelogde gebruikers (`401`)
- Ingelogde gasten zonder schrijfrechten (`403`)
- Toegestane schrijfacties door Editor en Admin

**Beperking:** SQLite en SQL Server zijn niet volledig identiek. Deze integratietests controleren dus niet ieder SQL Server-specifiek detail.

## 8. Inlevering en verantwoording

Voor Brightspace hoort bij de repository een PDF met de route- en testmatrix, screenshots van de publieke GET-route, registratie, login, `401`, `403`, een toegestane schrijfactie als Editor en Admin, en de testuitvoer. Er mogen geen wachtwoorden, volledige cookies of andere geheimen zichtbaar zijn.

De procesverantwoording in Boulder behandelt de aanpak, gebruikte bronnen, ontvangen en verwerkte feedback, zelfstudie-uren voor LU3 en LU4 en het daadwerkelijke AI-gebruik.

**AIAS-niveau 2:** bij de uitwerking is ChatGPT gebruikt voor uitleg, planning en ook concrete code-, test- en documentatievoorstellen. Dat laatste kan verder gaan dan wat AIAS-niveau 2 toestaat. Dit gebruik moet transparant worden verantwoord en waar nodig met de docent worden besproken. Beschrijf per bewijsstuk welke AI-hulp is gebruikt en wat zelfstandig is gecontroleerd en aangepast.

Daarnaast vraagt de opdracht om minimaal zes commits verspreid over minstens drie weken. Dit kan worden gecontroleerd via `git log`.

## 9. Bronnen

- [ASP.NET Core Identity in API's — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity-api-authorization?view=aspnetcore-10.0)
- [Role-based authorization — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles?view=aspnetcore-10.0)
- [Integration tests in ASP.NET Core — Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-10.0)
- [Unit testing C# with xUnit — Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-csharp-with-xunit)
