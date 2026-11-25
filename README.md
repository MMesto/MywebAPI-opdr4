# MywebAPI – beveiligd en getest (WPFW opdracht 4)

Dit is de backend van mijn portfoliosite. In opdracht 3 kon iedereen projecten en blogposts aanmaken, wijzigen en verwijderen. In opdracht 4 heb ik daar **gebruikers met rollen** aan toegevoegd: lezen mag iedereen, maar schrijven mag alleen een **Editor** of **Admin**. Daarnaast heb ik een **xUnit-testproject** gemaakt.

## Waarmee is het gebouwd?

- **.NET 10** en **ASP.NET Core Web API** met controllers
- **SQL Server Express** + **Entity Framework Core**
- **ASP.NET Core Identity** voor gebruikers, wachtwoorden, rollen en het inlogcookie
- **xUnit** + EF Core **InMemory** voor de tests
- **Swagger UI** om de API te testen

## Hoe zit het in elkaar?

```
MywebAPI-opdr4
 ├─ MywebAPI.slnx          → de solution (API + tests)
 ├─ MywebAPI/              → de API
 │   ├─ Auth/              → namen van de rollen en de policy
 │   ├─ Controllers/       → Projects, Blogposts en Auth (register/login/logout)
 │   ├─ Services/          → de logica
 │   ├─ Data/              → DbContext, entiteiten, migraties en de seeder
 │   ├─ Dtos/              → wat de API ontvangt en terugstuurt
 │   └─ Program.cs
 └─ MywebAPI.Tests/        → de xUnit-tests
```

## Wat heb je nodig?

- .NET 10 SDK
- SQL Server Express (`localhost\SQLEXPRESS`), ik log in met mijn Windows-account
- EF Core tools: `dotnet tool install -g dotnet-ef`

## 1. Database en migraties

De connectionstring staat in `MywebAPI/appsettings.json`. Er staat geen wachtwoord in, want ik gebruik Windows-login.

```powershell
dotnet ef database update --project .\MywebAPI
```

Dit maakt alles aan:
- `InitialCreate` → de tabellen `Projects` en `Blogposts`
- `AddIdentity` → de Identity-tabellen (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles`, …)
- `SeedRoles` → de drie rollen **Gast**, **Editor** en **Admin**

## 2. API starten

```powershell
dotnet run --project .\MywebAPI
```

Swagger staat dan op: http://localhost:5229/swagger

## 3. Gebruikers en rollen

| Rol | Wat mag je | Hoe krijg je de rol |
|---|---|---|
| niet ingelogd | alle GET-routes | – |
| Gast | alle GET-routes | automatisch bij registreren |
| Editor | GET + POST / PUT / DELETE | lokaal toekennen (zie hieronder) |
| Admin | GET + POST / PUT / DELETE | lokaal toekennen (zie hieronder) |

### Een gewone gebruiker (Gast)

1. `POST /api/auth/register` met `{ "email": "...", "wachtwoord": "..." }` → je krijgt de rol **Gast**
2. `POST /api/auth/login` met dezelfde gegevens → de API zet een cookie (`.AspNetCore.Identity.Application`)

Bij registreren krijg je altijd de rol Gast. Je kunt zelf geen andere rol kiezen.

### Editor en Admin (alleen lokaal)

Wachtwoorden horen niet in de repo. Daarom zet ik de gegevens in **user-secrets**. Dat is een bestand op mijn eigen computer, buiten de projectmap. Als de API in Development start, maakt `GebruikerSeeder` de Editor en Admin aan en geeft hij ze hun rol.

```powershell
dotnet user-secrets set "SeedUsers:Editor:Email" "editor@portfolio.local" --project .\MywebAPI
dotnet user-secrets set "SeedUsers:Editor:Wachtwoord" "<eigen wachtwoord>" --project .\MywebAPI
dotnet user-secrets set "SeedUsers:Admin:Email" "admin@portfolio.local" --project .\MywebAPI
dotnet user-secrets set "SeedUsers:Admin:Wachtwoord" "<eigen wachtwoord>" --project .\MywebAPI
dotnet run --project .\MywebAPI
```

Het wachtwoord moet minimaal 6 tekens hebben, met een hoofdletter, een kleine letter, een cijfer en een speciaal teken.

**Of via SSMS:** een bestaande gebruiker een rol geven:
```sql
INSERT INTO AspNetUserRoles (UserId, RoleId)
SELECT Id, 'rol-editor' FROM AspNetUsers WHERE Email = 'iemand@voorbeeld.nl';
```
Voor Admin gebruik je `'rol-admin'`.

> **Let op:** na een rolwijziging moet je opnieuw inloggen. De rollen zitten in het cookie, en het oude cookie heeft nog de oude rol.

## 4. Wie mag wat?

- Alle GET-routes zijn publiek (`[AllowAnonymous]`).
- POST, PUT en DELETE hebben de policy **`KanSchrijven`** nodig. Die is: rol **Editor of Admin**.
- Niet ingelogd → **401**. Wel ingelogd maar geen Editor of Admin → **403**.

## 5. Tests

```powershell
dotnet test
```

De tests staan in `MywebAPI.Tests` en gebruiken elk een **eigen InMemory-database**. Mijn echte SQL Server-database wordt dus nooit aangeraakt.

| Bestand | Wat wordt getest |
|---|---|
| `ProjectControllerTests` / `BlogpostsControllerTests` | alle 10 CRUD-routes: antwoord + wat er in de database gebeurt, ook bij een onbekend id |
| `ValidatieTests` | ongeldige invoer (lege titel, te lange titel, foute URL, fout e-mailadres, …) |
| `AutorisatieTests` | of POST/PUT/DELETE de policy hebben en GET publiek is, en of de policy Editor/Admin toelaat en Gast/niet ingelogd weigert |
| `AuthControllerTests` | registreren (altijd Gast), zwak wachtwoord, dubbel e-mailadres, login met cookie, fout wachtwoord, uitloggen |

### Coverage

```powershell
dotnet test --settings coverage.runsettings --collect:"XPlat Code Coverage"
reportgenerator -reports:"MywebAPI.Tests/TestResults/*/coverage.cobertura.xml" -targetdir:"coverage-report" -reporttypes:"Html;TextSummary"
```

ReportGenerator installeer je één keer met `dotnet tool install -g dotnet-reportgenerator-globaltool`.

## Endpoints

| Methode | Route | Wie | Gelukt | Fout |
|---|---|---|---|---|
| POST | /api/auth/register | iedereen | 201 | 400 |
| POST | /api/auth/login | iedereen | 200 + cookie | 401 |
| POST | /api/auth/logout | iedereen | 204 | – |
| GET | /api/projects | iedereen | 200 | – |
| GET | /api/projects/{id} | iedereen | 200 | 404 |
| POST | /api/projects | Editor, Admin | 201 | 400, 401, 403 |
| PUT | /api/projects/{id} | Editor, Admin | 204 | 400, 401, 403, 404 |
| DELETE | /api/projects/{id} | Editor, Admin | 204 | 401, 403, 404 |
| GET | /api/blogposts | iedereen | 200 | – |
| GET | /api/blogposts/{id} | iedereen | 200 | 404 |
| POST | /api/blogposts | Editor, Admin | 201 | 400, 401, 403 |
| PUT | /api/blogposts/{id} | Editor, Admin | 204 | 400, 401, 403, 404 |
| DELETE | /api/blogposts/{id} | Editor, Admin | 204 | 401, 403, 404 |