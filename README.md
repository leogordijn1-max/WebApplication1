# Portfolio Web API

## Projectbeschrijving

Dit project is gemaakt voor WPFW opdracht 3 van De Haagse Hogeschool. Het is een backend voor mijn persoonlijke portfoliosite, waarmee projecten en blogposts kunnen worden opgeslagen, opgehaald, aangepast en verwijderd.

## Gebruikte technologieën

- C# en ASP.NET Core (.NET 10)
- Minimal API
- Entity Framework Core
- SQL Server
- Swagger UI

## Architectuur

Ik gebruik Minimal API in combinatie met Vertical Slice Architecture.

De code is ingedeeld per functionaliteit. De mappen `Features/Projects` en `Features/Blogposts` bevatten afzonderlijke onderdelen voor Create, GetAll, GetById, Update en Delete.

Elke functionaliteit heeft een endpoint voor de HTTP-route en een handler voor het uitvoeren van de bewerking. De gegevens worden via `AppDbContext` en Entity Framework Core opgeslagen in SQL Server.

Voor de communicatie met de API gebruik ik DTO's. Hierdoor worden database-entiteiten niet rechtstreeks ontvangen of teruggestuurd.

## Database configureren

De applicatie gebruikt SQL Server Express met de database `PortfolioDb`.

De verbinding staat in `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=PortfolioDb;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Om de applicatie lokaal te gebruiken, moet SQL Server Express geïnstalleerd zijn en moet de instantie `SQLEXPRESS` draaien.

De applicatie gebruikt Windows-authenticatie om verbinding te maken met SQL Server.

## Database aanmaken

De database wordt aangemaakt en bijgewerkt met Entity Framework Core-migraties.

Open een terminal in de map met het `.csproj`-bestand en voer uit:

```bash
dotnet ef database update
```

Hiermee worden de bestaande migraties toegepast en worden de tabellen `Projects` en `Blogposts` aangemaakt.

Als de EF Core CLI nog niet geïnstalleerd is, kan deze worden geïnstalleerd met:

```bash
dotnet tool install --global dotnet-ef
```

Start daarna de applicatie met:

```bash
dotnet run
```

De endpoints kunnen worden getest via `/swagger` wanneer de applicatie in de Development-omgeving draait.

## Applicatie starten

Open het project in Visual Studio of een terminal.

Voer vanuit de map met het `.csproj`-bestand uit:

```bash
dotnet restore
dotnet run
```

Open daarna de Swagger UI via het lokale adres dat de applicatie toont, gevolgd door `/swagger`.

## API-endpoints

| Methode | Route | Functie |
|---|---|---|
| GET | `/projects` | Alle projecten ophalen |
| GET | `/projects/{id}` | Eén project ophalen |
| POST | `/projects` | Project toevoegen |
| PUT | `/projects/{id}` | Project aanpassen |
| DELETE | `/projects/{id}` | Project verwijderen |
| GET | `/blogposts` | Alle blogposts ophalen |
| GET | `/blogposts/{id}` | Eén blogpost ophalen |
| POST | `/blogposts` | Blogpost toevoegen |
| PUT | `/blogposts/{id}` | Blogpost aanpassen |
| DELETE | `/blogposts/{id}` | Blogpost verwijderen |

## Testen

De endpoints kunnen worden getest via Swagger UI.

Bij het testen is gecontroleerd of projecten en blogposts kunnen worden aangemaakt, opgehaald, aangepast en verwijderd. Ook is gecontroleerd of gegevens na het herstarten van de applicatie in SQL Server bewaard blijven.