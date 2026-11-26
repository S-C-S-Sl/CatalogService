# CatalogService

REST-Dienst für den Produktkatalog. Produkte werden dauerhaft in einer MongoDB gespeichert.

## Stand (26.11.2026)

Branch `feature/CWS-2-produkte-speichern`, aufbauend auf `feature/create-api`. Jira-Projekt `CWS` (Tickets CWS-2 und CWS-3).

| Bereich | Stand |
|---|---|
| Produkte anlegen, abrufen, ändern, löschen, auflisten | umgesetzt, gespeichert in MongoDB |
| Unit-Tests (Api, Logic, Interactors) | 36 Tests, grün |
| Container-Tests gegen MongoDB (`mongo:8.3`) | 9 Tests; vor der Umbenennung des API-Projekts und der Umstellung auf `IAsyncEnumerable` grün, danach noch nicht wieder ausgeführt |
| Docker-Image | nach der Umbenennung des API-Projekts noch nicht gebaut |
| Automatische Tests über HTTP gegen den laufenden Dienst | nicht vorhanden (CWS-3) |

## Aufbau

| Projekt | Aufgabe |
|---|---|
| `CatalogService.Api` | ASP.NET-Core-Dienst, Controller, Start und Konfiguration |
| `CatalogService.Interactors` | Anwendungsfälle, Datentransferobjekte der Schnittstelle, Mapping |
| `CatalogService.Logic` | Domänenobjekte, ohne Abhängigkeiten |
| `CatalogService.Resources` | Schnittstellen zu Außenwelt (`IProductRepository`, `IProductIdGenerator`) |
| `CatalogService.Resources.MongoDb` | Speicherung in MongoDB, Storage-Dokumente und Mapping |
| `CatalogService.Resources.RabbitMq` | Platzhalter, noch ohne Inhalt |
| `*.Tests` | Unit-Tests je Projekt; `CatalogService.Resources.MongoDb.Tests` startet MongoDB per Testcontainers |

## Schnittstelle (Version 1)

| Methode | Pfad | Antwort |
|---|---|---|
| `GET` | `/v1/products` | 200, Liste aller Produkte nach Namen sortiert |
| `GET` | `/v1/products/{id}` | 200 oder 404 |
| `POST` | `/v1/products` | 201 mit Verweis auf das neue Produkt, 400 bei ungültigen Angaben |
| `PUT` | `/v1/products/{id}` | 200, 404 oder 400; ersetzt alle Angaben |
| `DELETE` | `/v1/products/{id}` | 204 oder 404 |

Außerdem `/openapi/v1.json`, `/health/live` und `/health/ready`. Anfragen zum Ausprobieren stehen in `CatalogService.Api/CatalogService.Api.http`.

Ein Produkt hat Id, Name (1 bis 200 Zeichen), Beschreibung (optional, höchstens 2000 Zeichen) und einen Preis, der nicht negativ ist.

## Konfiguration

| Einstellung | Bedeutung |
|---|---|
| `MongoDb:ConnectionString` | Verbindung zur MongoDB, **Pflicht**. Fehlt sie, startet der Dienst nicht. |
| `MongoDb:DatabaseName` | Datenbank, Standard `catalog` |
| `MongoDb:ProductCollectionName` | Sammlung, Standard `products` |

In der Umgebung heißt die Einstellung `MongoDb__ConnectionString`. In `appsettings.Development.json` steht `mongodb://localhost:27017`.

## Lokal starten

```
docker run -d --name catalog-mongo -p 27017:27017 mongo:8.3
dotnet run --project CatalogService.Api --launch-profile http
```

Der Dienst hört dann auf `http://localhost:5091`.

## Tests ausführen

```
dotnet test CatalogService.slnx
```

Die Container-Tests starten ihre MongoDB selbst und brauchen dafür eine laufende Docker-Engine. Ohne Docker schlagen sie fehl, die übrigen Tests laufen.

## Pipeline (GitHub Actions)

| Workflow | Aufgabe | Läuft bei |
|---|---|---|
| `ci.yml` | bauen und testen, veröffentlicht nichts | immer: jeder Push auf jeden Branch und jeder Pull Request |
| `publish.yml` | Version bestimmen (GitVersion), Image bauen und nach `ghcr.io/s-c-s-sl/catalogservice` veröffentlichen | nur beim Merge eines Pull Requests nach `master` oder `develop` (Push auf diesen Branch) und bei Tags `v*` |

Die beiden Workflows sind voneinander unabhängig. `publish.yml` führt keine Tests aus und wartet nicht auf `ci.yml`. Damit nur geprüfter Code veröffentlicht wird, sollte der Schutz der Branches `master` und `develop` einen grünen CI-Lauf vor dem Merge verlangen.

## Bekannte offene Punkte

- `/health/ready` prüft die Datenbank nicht und meldet auch dann 200, wenn MongoDB nicht erreichbar ist.
- Das Chart im Cluster legt die Connection String für Dienste als `ConnectionStrings__MongoDb` ab, der Dienst liest `MongoDb:ConnectionString`. Vor dem Ausrollen im Cluster muss einer der Namen angepasst werden.
- Die Prüfung des Dienstes vor dem Ausrollen in ein produktives Cluster ist noch nicht gelöst (CWS-3).
