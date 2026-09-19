# Documentation d'architecture de ChandorWeb

## 1. Présentation générale

`ChandorWeb` est une solution de gestion d'église pour **Chandelier d'Or**. Elle couvre notamment :

- l'authentification des administrateurs ;
- les membres, leurs types, rôles et groupes d'âge ;
- les départements, responsables, commissions et équipes ;
- le calendrier et les programmes de l'église ;
- les ministères, activités et présences ;
- les comptes, revenus, dépenses et transactions ;
- les tableaux de bord et statistiques ;
- les notifications.

La solution contient trois projets :

```text
ChandorWeb.sln
│
├── AdimSystem
│   └── Première interface/prototype en Blazor Server
│
├── ChandorAdmin
│   └── Interface d'administration actuelle en Blazor WebAssembly
│
└── ChandorProject.Shared
    └── DTO, modèles de réponse et validations partagés
```

Le backend, les contrôleurs API, Entity Framework et la base de données ne sont pas présents dans ce dépôt. Les interfaces communiquent avec une API distante dont l'adresse principale est configurée dans le fichier `.env` à la racine du dépôt. `appsettings.json` fournit une valeur de secours.

## 2. Architecture globale

Le projet utilise une architecture frontend en couches :

```text
Navigateur
   │
   ▼
Pages et composants Razor
   │
   ▼
Interfaces de services
   │
   ▼
Implémentations Services/Api
   │
   ▼
ChandorApiHttp
   │ HTTP + JWT
   ▼
API distante
   │
   ▼
Backend et base de données
```

Ce n'est pas une Clean Architecture complète, car ce dépôt ne contient que le frontend et les contrats partagés. Il ne contient pas les couches serveur telles que les entités, repositories, cas d'utilisation, contrôleurs ou accès à la base de données.

## 3. Projet principal : ChandorAdmin

Le projet [`ChandorAdmin`](../ChandorAdmin/ChandorAdmin/ChandorAdmin.csproj) cible **.NET 9** et utilise **Blazor WebAssembly**.

Avec Blazor WebAssembly :

1. le navigateur télécharge l'application ;
2. le code C# est exécuté dans le navigateur via WebAssembly ;
3. le navigateur appelle directement l'API distante ;
4. l'API effectue la logique serveur et l'accès à la base de données.

### 3.1 Point d'entrée

Le point d'entrée est [`Program.cs`](../ChandorAdmin/ChandorAdmin/Program.cs).

Il effectue les opérations suivantes :

- création de l'application Blazor WebAssembly ;
- chargement de `appsettings.json` ;
- enregistrement des composants racines ;
- initialisation de Syncfusion ;
- configuration de l'autorisation ;
- configuration de l'état d'authentification ;
- configuration des clients HTTP ;
- enregistrement des interfaces et services dans l'injection de dépendances ;
- lancement de l'application.

Exemple d'injection de dépendances :

```csharp
builder.Services.AddScoped<IMemberService, MemberService>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
builder.Services.AddScoped<IFinanceService, FinanceService>();
```

Lorsqu'un composant demande `IMemberService`, Blazor lui fournit une instance de `MemberService`.

Dans Blazor WebAssembly, les services `Scoped` vivent généralement pendant toute la durée de l'application dans l'onglet courant.

### 3.2 Démarrage dans le navigateur

Le navigateur charge d'abord [`wwwroot/index.html`](../ChandorAdmin/ChandorAdmin/wwwroot/index.html).

Ce fichier :

- configure `/` comme chemin de base en local ;
- configure `/ChandorWeb/` sur GitHub Pages ;
- charge Bootstrap et les styles personnalisés ;
- charge le JavaScript Syncfusion ;
- crée l'élément `<div id="app">` ;
- charge `blazor.webassembly.js`.

Blazor monte ensuite [`App.razor`](../ChandorAdmin/ChandorAdmin/App.razor), qui contient le routeur principal.

```text
URL demandée
   │
   ▼
Router
   │
   ▼
AuthorizeRouteView
   ├── utilisateur autorisé → page + MainLayout
   └── utilisateur non autorisé → login ou message d'accès refusé
```

## 4. Architecture des dossiers de ChandorAdmin

```text
ChandorAdmin/ChandorAdmin/
│
├── Program.cs
├── App.razor
├── _Imports.razor
├── appsettings.json
│
├── Pages/
│   └── ncd/
│
├── Components/
│   ├── Auth/
│   ├── ChurchAdmin/
│   ├── Dashboard/
│   ├── Department/
│   ├── DepartmentManagement/
│   ├── Finance/
│   ├── GlobalNotification/
│   ├── Layout/
│   └── Member/
│
├── Layout/
├── Interfaces/
├── Services/
├── Configuration/
├── Data/
├── Models/
├── ViewModels/
├── Helpers/
└── wwwroot/
```

### 4.1 Pages

Le dossier `Pages` contient les composants associés à une URL.

| Fichier | Route | Fonction |
|---|---|---|
| `ChurchCalendar.razor` | `/` et `/church-calendar` | Calendrier principal |
| `Login.razor` | `/login` | Connexion |
| `ForgotPassword.razor` | `/forgot-password` | Mot de passe oublié |
| `ChurchAdmin.razor` | `/church-admin` | Tableau de bord administratif |
| `Member.razor` | `/church-members` | Gestion des membres |
| `ManageDepartment.razor` | `/departments` | Administration des départements |
| `Department.razor` | `/department/{departmentId}` | Détail d'un département |
| `FinanceStats.razor` | `/finance-stats` | Statistiques financières |
| `Transactions.razor` | `/finance-transactions` | Gestion des transactions |

Quelques anciennes pages de démonstration Syncfusion sont également présentes, par exemple `/tabs-features`, `/menubar-features` et `/syncfusion-index`.

### 4.2 Components

Les pages sont divisées en composants réutilisables. Une page coordonne généralement plusieurs composants spécialisés.

Exemple du module Membres :

```text
Member.razor
│
├── MemberGridPanel
├── MemberFilterSidebar
├── MemberEditorDialog
└── NotificationDialog
```

- `MemberGridPanel` affiche les données et gère les opérations CRUD ;
- `MemberFilterSidebar` applique les filtres ;
- `MemberEditorDialog` gère les formulaires d'ajout et de modification ;
- `NotificationDialog` affiche les confirmations, succès et erreurs.

### 4.3 Layout

Le dossier `Layout` définit la structure visuelle globale.

[`MainLayout.razor`](../ChandorAdmin/ChandorAdmin/Layout/MainLayout.razor) contient :

- la barre latérale ;
- le menu utilisateur ;
- la zone centrale qui reçoit les pages ;
- la détection d'activité utilisateur ;
- le moniteur d'inactivité.

[`NavMenu.razor`](../ChandorAdmin/ChandorAdmin/Layout/NavMenu.razor) contient le menu de navigation :

```text
Accueil
│
├── Administration
│   ├── Dashboard
│   ├── Membres
│   └── Départements
│
├── Finance
│   ├── Dashboard
│   └── Transactions
│
└── Departments
    └── Liste dynamique chargée depuis l'API
```

`AnonymousLayout.razor` est utilisé par les pages publiques comme la connexion et le mot de passe oublié.

### 4.4 Interfaces

Le dossier `Interfaces` contient les contrats des services :

```text
Interfaces/
├── Api/
├── Auth/
└── ChurchAdmin/
```

Une interface définit ce qu'un service sait faire sans imposer son implémentation.

```csharp
public interface IMemberService
{
    Task<DataResponse<IEnumerable<MemberDetailsDto>>?> GetMembersAsync();
    Task<DataResponse<MemberDto>?> AddSimpleMemberAsync(NewMemberDto member);
    Task<DataResponse<MemberDto>?> UpdateMemberAsync(UpdateMemberDto member);
    Task<DataResponse<bool>?> DeleteMemberAsync(Guid id);
}
```

### 4.5 Services

Le dossier `Services/Api` contient les implémentations qui appellent l'API distante.

Il existe des services pour :

- les comptes ;
- les groupes d'âge ;
- les utilisateurs ;
- les présences ;
- les programmes ;
- les devises ;
- les départements et équipes ;
- les courriels et téléphones ;
- les dépenses et revenus ;
- les membres et leurs activités ;
- les rôles et types de membres ;
- les ministères ;
- les notifications ;
- les transactions et catégories.

### 4.6 Configuration

Le dossier `Configuration` contient les classes liées à la configuration :

- `ChandorApiOptions` : URL et version de l'API ;
- `AuthOptions` : timeout d'inactivité et marge de rafraîchissement ;
- `CustomFormValidator` : affichage des erreurs de validation serveur.

### 4.7 Data

Le dossier `Data` contient surtout les adaptateurs Syncfusion :

- `CalendarDataAdaptor` ;
- `DepartmentCalendarDataAdaptor` ;
- `ScheduleData`.

Les adaptateurs traduisent les opérations du Scheduler Syncfusion vers les appels des services API.

### 4.8 Models et ViewModels

- `Models` contient les modèles spécifiques au frontend, par exemple la réponse d'authentification ou le modèle de l'éditeur financier.
- `ViewModels` contient les modèles spécialement préparés pour l'affichage, notamment ceux du dashboard administratif.

### 4.9 wwwroot

`wwwroot` contient les ressources publiques téléchargées par le navigateur :

```text
wwwroot/
├── css/
├── images/
├── index.html
├── manifest.webmanifest
├── service-worker.js
└── service-worker.published.js
```

## 5. Fichiers Razor et code-behind

Un composant peut être découpé en plusieurs fichiers :

```text
MemberEditorDialog.razor
MemberEditorDialog.razor.cs
MemberEditorDialog.razor.css
```

- `.razor` contient l'interface et le balisage ;
- `.razor.cs` contient la logique C# ;
- `.razor.css` contient le style isolé du composant.

Une classe `partial` permet de réunir automatiquement le `.razor` et le `.razor.cs` à la compilation.

## 6. Cycle de vie Blazor utilisé dans le projet

### 6.1 OnInitializedAsync

Appelé lors de la création initiale du composant. Il sert à charger les listes ou données de départ.

```csharp
protected override async Task OnInitializedAsync()
{
    await LoadDataAsync();
}
```

### 6.2 OnParametersSetAsync

Appelé lorsqu'une route ou un parent fournit un paramètre.

La page département reçoit par exemple :

```csharp
[Parameter]
public Guid DepartmentId { get; set; }
```

Elle recharge les informations lorsque l'identifiant change.

### 6.3 OnAfterRenderAsync

Appelé après que le composant a été affiché. Le projet l'utilise pour connecter les références des sous-composants :

```csharp
_gridRef.DialogRef = _dialogRef;
_filterRef.ContentRef = _gridRef;
_dialogRef.ContentRef = _gridRef;
```

Cette approche fonctionne, mais crée un couplage important entre la grille, le filtre et le dialogue. Plusieurs pages utilisent aussi un `Task.Delay(500)` pour attendre le rendu. Cette attente artificielle devrait à terme être remplacée par une initialisation déterministe.

## 7. Communication avec l'API

### 7.1 Configuration de l'adresse

La configuration principale de l'API se trouve dans [`.env`](../.env) à la racine du dépôt :

```dotenv
CHANDOR_API_BASE_URL=http://192.168.11.104:5227/api/
CHANDOR_API_VERSION_PATH_SEGMENT=1.0
```

Le fichier est intégré à `ChandorAdmin` pendant la compilation, puis chargé par `EmbeddedEnvConfiguration` avant la création des clients HTTP. Ses valeurs remplacent celles de [`appsettings.json`](../ChandorAdmin/ChandorAdmin/appsettings.json), qui sert de configuration de secours.

Après une modification du `.env`, il faut reconstruire ou relancer l'application. Une application Blazor WebAssembly étant téléchargée dans le navigateur, aucune donnée secrète ne doit être placée dans ce fichier.

`ChandorApiOptions` construit la racine versionnée :

```text
http://192.168.11.104:5227/api/ + v1.0/
=
http://192.168.11.104:5227/api/v1.0/
```

Les endpoints d'authentification utilisent toutefois directement `/api/Auth/...` sans segment `v1.0`.

### 7.2 ChandorApiHttp

[`ChandorApiHttp.cs`](../ChandorAdmin/ChandorAdmin/Services/Api/ChandorApiHttp.cs) centralise les requêtes HTTP.

Avant une requête, il :

1. vérifie la validité du token ;
2. construit l'URL versionnée ;
3. copie le contenu de la requête pour permettre un éventuel retry ;
4. ajoute `Authorization: Bearer <token>` ;
5. envoie la requête ;
6. tente un rafraîchissement si la réponse est `401` ;
7. rejoue la requête une fois ;
8. transforme la réponse JSON en `DataResponse<T>` ;
9. déconnecte l'utilisateur si la session est définitivement invalide.

### 7.3 Exemple complet : chargement des membres

```text
/church-members
       │
       ▼
Member.razor
       │
       ▼
MemberGridPanel.LoadData()
       │
       ▼
IMemberService.GetMembersAsync()
       │
       ▼
MemberService
       │
       ▼
ChandorApiHttp
       │
       ▼
GET /api/v1.0/Member/get-members-details
       │
       ▼
DataResponse<IEnumerable<MemberDetailsDto>>
       │
       ▼
Grille Syncfusion
```

## 8. Authentification et autorisation

Les fichiers principaux sont :

- [`AuthService.cs`](../ChandorAdmin/ChandorAdmin/Services/Auth/AuthService.cs) ;
- [`AuthState.cs`](../ChandorAdmin/ChandorAdmin/Services/Auth/AuthState.cs) ;
- [`CustomAuthenticationStateProvider.cs`](../ChandorAdmin/ChandorAdmin/Services/Auth/CustomAuthenticationStateProvider.cs) ;
- [`JwtClaimsMapper.cs`](../ChandorAdmin/ChandorAdmin/Services/Auth/JwtClaimsMapper.cs) ;
- [`InactivityMonitor.cs`](../ChandorAdmin/ChandorAdmin/Services/Auth/InactivityMonitor.cs).

### 8.1 Connexion

```text
Login.razor
   │
   ▼
AuthService.LoginAsync()
   │
   ▼
POST /api/Auth/login
   │
   ▼
Access token + refresh token
   │
   ▼
AuthState.SetSession()
   │
   ▼
localStorage
```

### 8.2 Persistance de la session

Les clés suivantes sont enregistrées dans `localStorage` :

```text
chandor.admin.auth.accessToken
chandor.admin.auth.refreshToken
chandor.admin.auth.accessExpiresAtUtc
```

Après un rechargement de la page, `AuthState` restaure les tokens et `CustomAuthenticationStateProvider` reconstruit le `ClaimsPrincipal`.

### 8.3 Rafraîchissement

Le token est rafraîchi lorsqu'il arrive dans la marge configurée de deux minutes. En cas de `401`, l'application essaie également de rafraîchir puis de rejouer la requête.

Un `SemaphoreSlim` empêche plusieurs requêtes parallèles de déclencher simultanément plusieurs rafraîchissements.

### 8.4 Inactivité

Le moniteur vérifie toutes les cinq secondes la dernière activité. Les clics, le clavier et le pointeur sont observés dans le layout. Après dix minutes sans activité, la session est supprimée et l'utilisateur est redirigé vers `/login`.

### 8.5 Protection des pages

[`Pages/_Imports.razor`](../ChandorAdmin/ChandorAdmin/Pages/_Imports.razor) applique `[Authorize]` par défaut à toutes les pages.

Les pages publiques utilisent explicitement `[AllowAnonymous]` :

- `/login` ;
- `/forgot-password`.

### 8.6 Sécurité à retenir

- Le stockage du refresh token dans `localStorage` l'expose à une éventuelle faille XSS.
- Le JWT est décodé côté client pour construire l'interface, mais sa signature n'est pas vérifiée localement.
- Le backend doit toujours vérifier le token, sa signature, son expiration, les rôles et les permissions.
- Les protections du frontend ne constituent pas une frontière de sécurité.

## 9. Bibliothèque ChandorProject.Shared

[`ChandorProject.Shared`](../ChandorProject/ChandorProject.Shared/ChandorProject.Shared.csproj) cible .NET 8.

Elle contient les DTO échangés avec l'API :

```text
DTOs/
├── Account/
├── AgeGroup/
├── AppUser/
├── Attendance/
├── ChurchProgram/
├── Currency/
├── Department/
├── Expenses/
├── Finance/
├── Income/
├── Member/
├── Ministry/
├── Notification/
├── Transaction/
└── User/
```

Les DTO suivent généralement trois usages :

- `New...Dto` pour la création ;
- `Update...Dto` pour la modification ;
- `...Dto`, `...DetailsDto` ou `...ViewDto` pour la lecture et l'affichage.

### 9.1 Réponse API standard

[`DataResponse.cs`](../ChandorProject/ChandorProject.Shared/DataResponse.cs) définit l'enveloppe commune :

```csharp
public class DataResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string?[]? Error { get; set; }
}
```

Une réponse JSON attendue ressemble à :

```json
{
  "success": true,
  "data": {},
  "message": "Operation successful",
  "error": null
}
```

### 9.2 Validation

Les DTO utilisent les Data Annotations :

```csharp
[Required]
[StringLength(100)]
[EmailAddress]
[Phone]
```

Attention : `[Required]` sur un `Guid` non nullable ne rejette pas automatiquement `Guid.Empty`. Un contrôle supplémentaire ou un `Guid?` est nécessaire pour représenter une valeur réellement non sélectionnée.

## 10. Module Membres

La page principale est [`Member.razor`](../ChandorAdmin/ChandorAdmin/Pages/ncd/Member.razor).

```text
Member.razor
│
├── recherche
├── filtre par type
├── bouton Ajouter
├── bouton Filtrer
├── MemberGridPanel
├── MemberFilterSidebar
├── MemberEditorDialog
└── NotificationDialog
```

La grille conserve :

- `AllMembers` : liste complète reçue de l'API ;
- `GridData` : liste filtrée affichée.

Les principales opérations sont :

```text
POST   Member/add-simple_member
POST   Member/add-member-with-user
PUT    Member/update-member
DELETE Member/delete-member/{id}
GET    Member/get-member/{id}
GET    Member/get-members-details
```

Les sous-écrans de `Components/Member/Management` administrent :

- les groupes d'âge ;
- les rôles ;
- les types de membres.

## 11. Module Départements

### 11.1 Administration générale

[`ManageDepartment.razor`](../ChandorAdmin/ChandorAdmin/Pages/ncd/ManageDepartment.razor) permet de gérer :

- la liste des départements ;
- leurs membres ;
- leurs responsables ;
- leurs commissions ;
- l'ajout, la modification et la suppression ;
- la recherche et l'export Excel.

Endpoints principaux :

```text
POST   Department/add-department
PUT    Department/update-department
DELETE Department/delete-department/{id}
GET    Department/get-departments
```

### 11.2 Page d'un département

La route `/department/{departmentId}` charge un département précis et ses membres.

Fonctionnalités :

- recherche ;
- filtre par genre ;
- filtres latéraux ;
- ajout d'un membre ;
- retrait d'un membre ;
- association d'un rôle.

Endpoints :

```text
GET    Department/get-department/{id}
GET    Department/get-members-by-department-id/{departmentId}
POST   Department/add-department-member/{departmentId}/{memberId}/{roleId}
DELETE Department/remove-department-member/{departmentId}/{memberId}
```

### 11.3 Menu dynamique

Le menu charge également la liste des départements pour créer des liens dynamiques. Il n'existe pas actuellement de cache client partagé, donc plusieurs composants peuvent demander la même liste à l'API.

## 12. Module Calendrier

La page [`ChurchCalendar.razor`](../ChandorAdmin/ChandorAdmin/Pages/ncd/ChurchCalendar.razor) utilise le Scheduler Syncfusion.

```text
Scheduler Syncfusion
       │
       ▼
CalendarDataAdaptor
       │
       ├── ReadAsync
       ├── InsertAsync
       ├── UpdateAsync
       └── RemoveAsync
       │
       ▼
ChurchProgramService
       │
       ▼
API
```

L'adaptateur gère :

- la lecture par période ;
- la création ;
- la modification ;
- la suppression ;
- les événements récurrents ;
- les règles et exceptions de récurrence ;
- la conversion des identifiants ;
- les erreurs renvoyées par l'API.

`DepartmentCalendarDataAdaptor` fournit le même principe pour les événements d'un département.

`ScheduleData.cs` contient environ 630 lignes de données de démonstration Syncfusion sur des thèmes astronomiques. Ce fichier ne correspond pas au domaine de l'église et semble être un reste d'exemple.

## 13. Module Finances

### 13.1 Dashboard financier

[`FinanceStats.razor.cs`](../ChandorAdmin/ChandorAdmin/Pages/ncd/FinanceStats.razor.cs) charge cinq groupes de données en parallèle :

```text
FinanceStats
   │
   ├── GetFinanceSummariesAsync
   ├── GetCashflowSeriesAsync
   ├── GetFinanceActivitiesAsync
   ├── GetIncomeByCategoriesAsync
   └── GetExpensesByCategoriesAsync
```

Il affiche :

- le résumé financier ;
- le cash-flow ;
- les activités récentes ;
- les revenus par catégorie ;
- les dépenses par catégorie.

La page utilise `Task.WhenAll` afin de ne pas attendre les cinq appels séquentiellement.

### 13.2 Transactions

La page [`Transactions.razor`](../ChandorAdmin/ChandorAdmin/Pages/ncd/Transactions.razor) assemble :

```text
TransactionGridPanel
TransactionFilterSidebar
TransactionEditorDialog
```

La grille charge par défaut les transactions du mois courant et propose :

- recherche ;
- filtre revenu/dépense ;
- filtre par catégorie ;
- filtre par période ;
- ajout ;
- modification ;
- suppression ;
- export Excel.

### 13.3 Chevauchement des services

Le projet contient plusieurs familles liées aux finances :

- `FinanceService` ;
- `TransactionService` ;
- `IncomeService` ;
- `ExpensesService`.

`IncomeService` et `ExpensesService` ressemblent à des CRUD historiques, tandis que `FinanceService` et `TransactionService` fournissent des opérations plus orientées métier et reporting. Il faudra déterminer quelle API constitue la référence afin de réduire les doublons.

### 13.4 Calcul du solde

Dans `TransactionGridPanel.UpdateTotalBalance()`, le résultat de `FormatBalance` est actuellement ignoré :

```csharp
_ = FormatBalance(incomeSum, expenseSum);
```

Si le solde doit être affiché, il faudrait l'affecter à une propriété utilisée par l'interface.

## 14. Dashboard administratif

La page [`ChurchAdmin.razor`](../ChandorAdmin/ChandorAdmin/Pages/ncd/ChurchAdmin.razor) présente :

- total des membres ;
- membres actifs ;
- visiteurs ;
- conversions ;
- activités récentes ;
- événements à venir ;
- croissance des inscriptions ;
- répartition par ministère.

Ces données sont actuellement simulées par [`ChurchAdminDashboardMockService.cs`](../ChandorAdmin/ChandorAdmin/Services/ChurchAdmin/ChurchAdminDashboardMockService.cs). Le dashboard n'est donc pas encore connecté aux vraies données de l'API.

## 15. Notifications

Deux concepts différents portent le nom de notification.

### 15.1 Notifications visuelles

`NotificationDialog.razor` affiche localement :

- succès ;
- erreurs ;
- avertissements ;
- confirmations.

Elles ne sont pas persistées dans la base de données.

### 15.2 Notifications métier

`NotificationService` communique avec l'API :

```text
POST Notification/create
GET  Notification/get-all
GET  Notification/get-user-not-expired/{userId}
```

Ces notifications semblent être des messages persistants destinés aux utilisateurs.

Des noms comme `UiNotificationDialog` et `UserNotificationService` rendraient la distinction plus claire.

## 16. Utilisateurs, AppUser et Member

Le projet possède trois concepts proches :

```text
AuthService
  Connexion JWT, refresh et déconnexion

UserService
  Gestion d'un ancien ou autre modèle utilisateur

AppUserService
  Enregistrement, connexion, confirmation d'adresse et image

MemberService
  Gestion de la personne membre de l'église
```

Une séparation logique possible serait :

- `AppUser` : compte technique d'authentification ;
- `Member` : personne membre de l'église ;
- `User` : ancien modèle à supprimer ou migrer.

La confirmation exacte nécessite toutefois le code du backend.

## 17. Syncfusion et interface graphique

L'application utilise notamment :

- DataGrid ;
- Scheduler ;
- Charts ;
- Dialogs ;
- Dropdowns ;
- DateRangePicker ;
- Sidebar ;
- Notifications ;
- formulaires et boutons.

L'interface combine :

```text
Composants Razor
     +
Composants Syncfusion
     +
Bootstrap
     +
CSS personnalisé
```

Les styles sont actuellement répartis entre plusieurs fichiers Bootstrap personnalisés, `app.css`, les CSS métier, les fichiers `.razor.css` et du CSS directement intégré dans certains composants. Une consolidation réduirait les conflits de priorité.

## 18. PWA et service worker

Le projet contient les fichiers nécessaires à une Progressive Web App :

- `manifest.webmanifest` ;
- icônes PWA ;
- `service-worker.js` ;
- `service-worker.published.js`.

Cependant, `index.html` désenregistre actuellement tous les service workers avant de charger Blazor. Le fonctionnement hors ligne est donc désactivé malgré la présence des fichiers PWA.

## 19. Déploiement GitHub Pages

Le workflow [`deploy.yml`](../.github/workflows/deploy.yml) publie uniquement `ChandorAdmin`.

Lors d'un push sur la branche `master` :

1. GitHub récupère le dépôt ;
2. installe .NET 9 ;
3. exécute `dotnet publish` ;
4. copie `index.html` vers `404.html` pour le routage client ;
5. publie `publish/wwwroot` sur GitHub Pages.

L'application est configurée pour être hébergée sous `/ChandorWeb/`.

## 20. Projet AdimSystem

[`AdimSystem`](../AdimSystem/AdimSystem.csproj) est une application **Blazor Server Interactive .NET 9**.

```text
Blazor Server
Navigateur ← connexion interactive → serveur ASP.NET

Blazor WebAssembly
Navigateur exécute l'application et appelle directement l'API
```

`AdimSystem` contient :

- de nombreuses démonstrations Syncfusion ;
- une ancienne implémentation des services API ;
- une interface calendrier/département/login ;
- un ancien layout ;
- des copies de nombreux services présents dans `ChandorAdmin`.

Il semble être un prototype ou une ancienne version, car :

- il contient beaucoup de pages `*Features.razor` ;
- ses services sont largement dupliqués ;
- il n'est pas publié par GitHub Actions ;
- son authentification est moins complète ;
- `ChandorAdmin` contient les fonctionnalités métier récentes.

La décision de le conserver, archiver ou supprimer devrait être explicite.

## 21. Points forts

- Bonne séparation entre interfaces et implémentations.
- Centralisation des appels dans `ChandorApiHttp`.
- Gestion du refresh token et retry après `401`.
- DTO dédiés à la création, modification et lecture.
- Pages découpées en composants spécialisés.
- Routes protégées par défaut.
- Usage des `CancellationToken` dans les services.
- Configuration centralisée de l'API.
- Requêtes financières parallélisées.
- Publication automatisée sur GitHub Pages.
- Solution compilable sans erreur.
- Réponse API standardisée avec `DataResponse<T>`.

## 22. Points à améliorer

### 22.1 Priorité haute

1. Décider si `AdimSystem` est encore utilisé.
2. Remplacer le dashboard simulé par une implémentation API.
3. Ajouter des tests automatisés.
4. Réévaluer le stockage du refresh token dans `localStorage`.
5. Retirer la licence Syncfusion du code source.
6. Journaliser les exceptions au lieu de les masquer avec des `catch` vides.
7. Ajouter des politiques d'autorisation par rôle ou permission.
8. Corriger le solde financier calculé mais ignoré.

### 22.2 Priorité moyenne

1. Supprimer ou isoler les pages et données de démonstration.
2. Réduire les références croisées entre composants.
3. Remplacer les `Task.Delay(500)` par une initialisation déterministe.
4. Clarifier `FinanceService` contre `TransactionService`.
5. Clarifier `UserService` contre `AppUserService`.
6. Centraliser les textes et la langue de l'interface.
7. Harmoniser les versions .NET.
8. Harmoniser les versions Syncfusion.
9. Ajouter un cache client pour les listes de référence.
10. Consolider les fichiers CSS.

### 22.3 Nommage

Plusieurs noms comportent des erreurs historiques :

```text
AdimSystem       → probablement AdminSystem
Congration       → probablement Congregation
Ministies        → probablement Ministries
GetDashboar...   → probablement GetDashboard...
```

Certaines fautes sont probablement utilisées dans les routes du backend. Leur correction doit donc être coordonnée avec l'API.

## 23. Où modifier quoi ?

| Besoin | Emplacement |
|---|---|
| Ajouter une URL | `Pages/` avec `@page` |
| Ajouter un composant réutilisable | `Components/` |
| Ajouter une entrée du menu | `Layout/NavMenu.razor` |
| Ajouter un endpoint | `Interfaces/Api`, puis `Services/Api` |
| Ajouter un contrat de données | `ChandorProject.Shared/DTOs` |
| Modifier l'URL de l'API | `.env` à la racine, puis reconstruire l'application |
| Modifier le timeout | section `Auth` de `appsettings.json` |
| Modifier la connexion | `Services/Auth/AuthService.cs` |
| Modifier le stockage du token | `Services/Auth/AuthState.cs` |
| Modifier les claims JWT | `Services/Auth/JwtClaimsMapper.cs` |
| Modifier le layout | `Layout/MainLayout.razor` |
| Modifier le menu | `Layout/NavMenu.razor` et `.razor.cs` |
| Modifier les styles globaux | `wwwroot/css` |
| Modifier le style d'un composant | fichier `.razor.css` |
| Modifier le dashboard financier | `Pages/ncd/FinanceStats.*` |
| Modifier les transactions | `Components/Finance/Transactions` |
| Modifier les membres | `Components/Member` |
| Modifier les départements | `Components/Department*` |
| Modifier le calendrier | `Data/*CalendarDataAdaptor.cs` |
| Modifier le déploiement | `.github/workflows/deploy.yml` |

## 24. Commandes utiles

### Compiler toute la solution

```bash
dotnet build ChandorWeb.sln
```

État vérifié lors de l'analyse :

```text
Build succeeded
0 erreur
5 avertissements
```

### Lancer ChandorAdmin

```bash
dotnet run --project ChandorAdmin/ChandorAdmin/ChandorAdmin.csproj
```

Adresses configurées :

```text
http://localhost:5043
https://localhost:7405
```

### Lancer AdimSystem

```bash
dotnet run --project AdimSystem/AdimSystem.csproj
```

Adresses configurées :

```text
http://localhost:5221
https://localhost:7253
```

## 25. Résumé mental du projet

Pour suivre une fonctionnalité, utiliser ce chemin :

```text
Page Razor
    ↓
Composant spécialisé
    ↓
Interface de service
    ↓
Service API
    ↓
ChandorApiHttp
    ↓
API distante
    ↓
DataResponse<DTO>
    ↓
Affichage Syncfusion
```

Responsabilités principales :

- `Pages` : représentent les URL ;
- `Components` : construisent l'interface et les fonctionnalités ;
- `Interfaces` : définissent les contrats ;
- `Services/Api` : communiquent avec le backend ;
- `Services/Auth` : gèrent JWT et session ;
- `DTOs` : transportent et valident les données ;
- `Layout` : définit la structure globale ;
- `wwwroot` : contient les ressources du navigateur ;
- `Program.cs` : configure et assemble l'application ;
- `ChandorAdmin` : application active ;
- `AdimSystem` : prototype ou ancienne interface ;
- `ChandorProject.Shared` : contrats partagés avec l'API.

## 26. Conclusion

`ChandorWeb` est principalement un frontend administratif Blazor WebAssembly connecté à une API distante. Sa structure essentielle est cohérente : présentation, interfaces, services HTTP et contrats partagés.

La principale complexité vient de l'évolution historique du dépôt : une ancienne application Blazor Server, des exemples Syncfusion, plusieurs générations de services financiers et utilisateurs, ainsi que quelques fonctionnalités encore simulées coexistent avec l'application actuelle.

Pour comprendre ou faire évoluer le produit, les dossiers prioritaires sont :

```text
ChandorAdmin/ChandorAdmin/Pages/ncd
ChandorAdmin/ChandorAdmin/Components
ChandorAdmin/ChandorAdmin/Services
ChandorAdmin/ChandorAdmin/Interfaces
ChandorAdmin/ChandorAdmin/Layout
ChandorProject/ChandorProject.Shared/DTOs
```
