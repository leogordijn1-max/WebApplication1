Portfolio Web API — WPFW opdracht 4
Doel
Deze ASP.NET Core Minimal API beheert projecten en blogposts voor een persoonlijke portfoliosite. Voor opdracht 4 is de API uitgebreid met ASP.NET Core Identity, cookie-authenticatie, rolgebaseerde autorisatie en xUnit-integratietests.

Repository: https://github.com/leogordijn1-max/WebApplication1
Technologie en indeling
- C# / .NET 10, ASP.NET Core Minimal API
- Entity Framework Core met SQL Server Express voor gewone opslag
- ASP.NET Core Identity met rollen Gast, Editor, Admin
- xUnit en Microsoft.AspNetCore.Mvc.Testing voor HTTP-integratietests
- SQLite in-memory als afzonderlijke testdatabase
- Vertical Slice Architecture met routes, DTO's en handlers per actie onder Features/
Belangrijke bestanden: WebApplication1/Program.cs, WebApplication1/data/AppDbContext.cs, WebApplication1/data/Seed/RoleSeeder.cs, WebApplication1/Features/Authentication/, WebApplication1.Tests/Infrastructure/CustomWebApplicationFactory.cs.
Benodigdhedencd
1. .NET 10 SDK
2. SQL Server Express met een bereikbare instantie (in de huidige lokale configuratie localhost\\SQLEXPRESS)
3. EF Core CLI: dotnet tool install --global dotnet-ef --version "10.0.*" als nog niet aanwezig
4. Een geschikte .NET-ontwikkelomgeving, zoals Visual Studio of een terminal
De huidige WebApplication1/appsettings.json gebruikt een lokale Windows-authenticatieverbinding naar PortfolioDb. Pas de connectionstring lokaal aan indien je SQL Server een andere naam heeft. Plaats geen echte wachtwoorden, API-sleutels of sessiecookies in Git. TrustServerCertificate=True is uitsluitend bedoeld voor lokale ontwikkeling.
Database voorbereiden
Ga in de terminal naar de map WebApplication1 waarin het API-`.csproj` staat. Voer uit:
dotnet restore
dotnet ef database update
database update past de aanwezige EF Core-migraties toe, inclusief de Identity-tabellen. Bij normaal opstarten maakt RoleSeeder de drie rollen aan als ze ontbreken. De testomgeving gebruikt geen SQL Server-migraties: daar worden de tabellen tijdelijk met SQLite en EnsureCreatedAsync aangemaakt.
API starten
cd WebApplication1
dotnet run
Open in de Development-omgeving het getoonde lokale adres met /swagger erachter. Voor HTTPS kan je lokale ontwikkelcertificaat nodig zijn.
Authenticatie en rollen
- POST /auth/register maakt via UserManager<User> een account aan met gebruikersnaam/e-mailadres en wachtwoord. Nieuwe accounts krijgen uitsluitend de rol Gast.
- POST /auth/login controleert gegevens via SignInManager<User> en stuurt bij succes een HTTP-authenticatiecookie terug.
- Een gebruiker moet opnieuw inloggen na een rolwijziging, zodat de nieuwe rollen in de authenticatiecontext worden opgenomen.
- Wachtwoorden worden beheerd door Identity en worden niet als platte tekst opgeslagen.
Voorbeeldregistratie (uitsluitend lokale testgegevens)
{ "email": "gast-voorbeeld@example.com", "password": "KiesZelfEenSterkTestWachtwoord!1" }
Lokaal de rol Editor of Admin toekennen
Registreer eerst via /auth/register een testaccount. Voer vervolgens in SQL Server Object Explorer of SSMS op PortfolioDb de onderstaande query uit, nadat je het e-mailadres en de gewenste rol hebt aangepast. Dit is een lokale beheerprocedure, geen publiek API-endpoint. Tijdens de ontwikkeling is voor de testaccounts ook tijdelijk UserManager.AddToRoleAsync gebruikt; die tijdelijke toewijzing is uit de definitieve RoleSeeder verwijderd. De onderstaande SQL-procedure is bedoeld om dezelfde roltoewijzing lokaal te reproduceren. Maak vooraf eventueel een databasebackup.
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
WHERE u.Email = @Email AND r.Name = @Rol
  AND NOT EXISTS (
      SELECT 1 FROM dbo.AspNetUserRoles ur
      WHERE ur.UserId = u.Id AND ur.RoleId = r.Id
  );

SELECT u.Email, r.Name AS Rol
FROM dbo.AspNetUsers u
JOIN dbo.AspNetUserRoles ur ON ur.UserId = u.Id
JOIN dbo.AspNetRoles r ON r.Id = ur.RoleId
WHERE u.Email = @Email;
Deze procedure laat de rol Gast bestaan naast Editor/Admin. Dat is toegestaan: de policy controleert of de gebruiker Editor of Admin heeft. Log daarna uit en opnieuw in. Zet nooit een automatische rolpromotie voor een vast e-mailadres in de definitieve RoleSeeder.
Route- en toegangsregels
Methode	Route	Toegang	Voorbeelden van HTTP-uitkomsten
GET	/projects	Iedereen	200
GET	/projects/{id}	Iedereen	200, 404
POST	/projects	Editor of Admin	201, 400; anoniem 401; Gast 403
PUT	/projects/{id}	Editor of Admin	200, 400, 404; anoniem 401; Gast 403
DELETE	/projects/{id}	Editor of Admin	Successtatus, 404; anoniem 401; Gast 403
GET	/blogposts	Iedereen	200
GET	/blogposts/{id}	Iedereen	200, 404
POST	/blogposts	Editor of Admin	201, 400; anoniem 401; Gast 403
PUT	/blogposts/{id}	Editor of Admin	200, 400, 404; anoniem 401; Gast 403
DELETE	/blogposts/{id}	Editor of Admin	Successtatus, 404; anoniem 401; Gast 403
POST	/auth/register	Iedereen	201, 400
POST	/auth/login	Iedereen	200 + Set-Cookie, 401


CanEditPortfolio wordt in Program.cs geconfigureerd met RequireRole("Editor", "Admin"). De zes schrijfroutes gebruiken .RequireAuthorization("CanEditPortfolio"). De GET-routes blijven publiek. Voor de cookie-authenticatie zijn redirects uitgeschakeld ten gunste van HTTP 401 en 403.
Invoervalidatie
Bij Projects zijn titel, beschrijving en categorie verplicht en moet GitHubUrl een absolute HTTP(S)-URL zijn. Bij Blogposts zijn titel, inhoud en een ingevulde publicatiedatum verplicht. Ongeldige POST/PUT-verzoeken krijgen een 400 Bad Request. Onbekende ID's geven 404 Not Found, mits de gebruiker voor de schrijfroutes geautoriseerd is.
Automatische tests
Ga in de solution-hoofdmap staan en voer uit:
dotnet restore
dotnet test
Bij de lokale uitvoering op 18 november 2026 rapporteerde de ontwikkelaar 40 van de 40 tests geslaagd. Met dotnet test kan de beoordelaar de tests zelf uitvoeren. Dit resultaat is geen vervanging voor de gevraagde schermafbeelding van de actuele testuitvoer.

Belangrijke testbestanden:
- WebApplication1.Tests/Projects/ProjectEndpointTests.cs
- WebApplication1.Tests/Blogposts/BlogpostEndpointTests.cs
- WebApplication1.Tests/Authentication/AuthEndpointTests.cs
- WebApplication1.Tests/Infrastructure/TestUsers.cs
- WebApplication1.Tests/Infrastructure/CustomWebApplicationFactory.cs
De tests gebruiken HTTP-verzoeken via WebApplicationFactory<Program>. De factory vervangt de SQL Server-registratie van AppDbContext door SQLite Data Source=:memory:. Daardoor zijn de testaccounts en testgegevens gescheiden van PortfolioDb. Tests controleren de statuscodes, responsegegevens en waar relevant de database-effecten van CREATE/UPDATE/DELETE. Er zijn ook tests voor ongeldige invoer, onbekende ID's, anonieme schrijfpogingen en Gast/Editor/Admin.

Beperking: SQLite is niet identiek aan SQL Server. SQL Server-specifieke verschillen vallen buiten deze in-memory-tests.
Inlevering en verantwoording
Voor Brightspace hoort naast deze README een PDF met een route-/testmatrix, screenshots voor GET/registratie/login/401/403/Editor/Admin, en een schermafbeelding van de dotnet test-uitvoer. Voeg geen cookie-inhoud of wachtwoorden toe aan screenshots. De procesverantwoording, bronnen, feedback, bestede uren (LU3 en LU4) en AI-gebruik komen afzonderlijk in Boulder.

AIAS-niveau 2 en transparantie: bij de uitvoering is ChatGPT gebruikt voor uitleg en voor concrete code- en testvoorstellen. Dat laatste gaat verder dan uitsluitend planning/structuur zoals de opdracht dit niveau omschrijft. Vermeld het werkelijke gebruik daarom volledig in Boulder en bespreek zo nodig met de docent hoe dit beoordeeld wordt. Beschrijf per bewijsstuk welke suggesties zijn gebruikt, wat zelfstandig is gecontroleerd/aangepast en wat de eigen bijdrage is. De vereiste commitgeschiedenis (minimaal zes commits over minstens drie weken) moet aan de hand van git log worden gecontroleerd.
Bronnen
- ASP.NET Core Identity in APIs (Microsoft Learn)
- Role-based authorization (Microsoft Learn)
- Integration tests in ASP.NET Core (Microsoft Learn)
- Testing with dotnet test and xUnit (Microsoft Learn)