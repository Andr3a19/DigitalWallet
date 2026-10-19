# DigitalWallet - Piattaforma Bancaria Client-Server Distribuita

Un sistema finanziario distribuito sviluppato in **C# 12** e **.NET** che implementa un'architettura **Client-Server** per la gestione di conti digitali, bonifici e transazioni monetarie protette.

> **Stato del Progetto:**  
> • **Versione Beta v0.5 (Core Banking Engine):** Completata  
> • **Versione v0.8 - Sicurezza, Identità, DLT & Idempotenza:** In fase di sviluppo attivo  
> • **Versione v1.0 (Ecosistema Enterprise con Sicurezza & Multi-Valuta):** In fase di sviluppo  

---

## Visione Architetturale dell'Ecosistema

Il sistema è progettato per simulare l'infrastruttura reale di un istituto bancario moderno, separando nettamente il backend dai client dedicati a diversi profili utente:

```text
                                 ┌─────────────────────────────────────────────────┐
                                 │            SERVER (DigitalWallet.Api)           │
                                 │  • ASP.NET Core REST API & WebSockets (SignalR) │
                                 │  • Entity Framework Core + SQLite               │
                                 │  • Autenticazione JWT & RBAC (Admin/Customer)   │
                                 │  • Audit Crittografico SHA-256 & Idempotenza    │
                                 └────────────────────────▲────────────────────────┘
                                                          │
                             Canale HTTP / WebSockets     │     Canale HTTP / WebSockets
                               (Token Ruolo: Admin)       │      (Token Ruolo: Customer)
                                                          │
                       ┌──────────────────────────────────┴──────────────────────────────────┐
                       │                                                                     │
            ┌──────────┴───────────────┐                                          ┌──────────┴───────────────┐
            │   CLIENT 1: BACKOFFICE   │                                          │  CLIENT 2: HOME BANKING  │
            │   (Operatore di Banca)   │                                          │     (Cliente Privato)    │
            ├──────────────────────────┤                                          ├──────────────────────────┤
            │ • Anagrafica globale     │                                          │ • Login & Profilo sicuro │
            │ • Verifica Audit Ledger  │                                          │ • Gestione propri conti  │
            │ • Monitoraggio conti     │                                          │ • Bonifici P2P con IBAN  │
            │ • Tassi cambio EUR/CHF   │                                          │ • Notifiche accrediti    │
            └──────────────────────────┘                                          └──────────────────────────┘
```

---

## Tabella di Marcia (Roadmap)

### [x] Fase 1: Beta v0.5 - Core Banking Engine (Completata)
*Le fondamenta del motore transazionale e della comunicazione di rete:*
- [x] Setup dell'architettura Client-Server (Soluzione multi-progetto in Visual Studio).
- [x] Sviluppo del backend **ASP.NET Core Web API** con documentazione interattiva tramite **Scalar**.
- [x] Persistenza relazionale su database **SQLite** con approccio Code-First tramite **Entity Framework Core (EF Core)**.
- [x] Modellazione delle entità di base: `Wallet` (conti) e `Transaction` (movimenti contabili).
- [x] Protezione da attacchi di *over-posting* tramite DTO dedicati (`CreateWalletRequest`, `DepositRequest`, `TransferRequest`).
- [x] Implementazione del **trasferimento P2P (Bonifico)** protetto da **transazioni atomiche ACID** (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`) con doppia registrazione contabile.
- [x] Endpoint per l'estratto conto con ordinamento cronologico discendente via LINQ (`OrderByDescending`).
- [x] Sviluppo del servizio client HTTP asincrono (**`WalletApiClient`**) con gestione robusta delle eccezioni di rete.

---

### [ ] Fase 2: Versione v0.8 - Sicurezza, Identità, DLT & Idempotenza (In Corso)
*Protezione avanzata, prevenzione del double-spending e riservatezza bancaria:*
- [x] **Modello Utente e Autenticazione:** Introduzione della tabella `Users` con password protette da hashing crittografico a senso unico (PBKDF2/BCrypt, nessuna password in chiaro).
- [ ] **Autenticazione Stateless con JWT:** Rilascio di JSON Web Token firmati digitalmente all'accesso (`POST /api/auth/login`).
- [ ] **Role-Based Access Control (RBAC):** Separazione dei ruoli tramite autorizzazioni dedicate:
  - Ruolo `Admin` per gli impiegati di filiale (accesso ad audit completi e viste complessive).
  - Ruolo `Customer` per i clienti privati.
- [ ] **Data Isolation (Segreto Bancario):** Vincolo di proprietà dei conti (un cliente può consultare ed operare esclusivamente sui propri wallet tramite validazione dei claim del token).
- [ ] **Relazione 1-a-Molti:** Possibilità per un singolo utente di essere intestatario di più conti.
- [ ] **Generatore di IBAN:** Assegnazione di codici IBAN fittizi strutturati (es. `IT99DWT...` e `CH99DWT...`) per evitare l'esposizione diretta degli ID sequenziali del database.
- [ ] **Idempotenza sui Bonifici (Anti Double-Spending):** Tracciamento delle richieste tramite header `Idempotency-Key` (GUID) e tabella dedicata per prevenire addebiti duplicati causati da latenze o retry di rete.
- [ ] **Audit Trail Crittografico (Blockchain Ledger):** Concatenamento a senso unico delle transazioni tramite hashing SHA-256 (`PreviousHash` e `Hash`) con endpoint dedicato `GET /api/wallets/{id}/verify-audit` per rilevare manomissioni non autorizzate al database SQLite.

---

### [ ] Fase 3: Versione v1.0 - Ecosistema Enterprise & Real-Time (Rilascio Finale)
*Funzionalità avanzate orientate al contesto bancario elvetico/internazionale:*
- [ ] **Supporto Multi-Valuta (EUR / CHF):**
  - Gestione di conti sia in Euro sia in Franchi Svizzeri.
  - Implementazione di un servizio di cambio valuta interno con tassi di conversione per bonifici transfrontalieri.
- [ ] **Due Client Indipendenti:**
  - *DigitalWallet.Backoffice:* Console riservata agli operatori di banca per verifiche, anagrafiche, verifica integrità ledger e configurazione tassi.
  - *DigitalWallet.HomeBanking:* Applicazione cliente per autenticazione personale, consultazione del saldo e invio bonifici.
- [ ] **Notifiche Push in Tempo Reale (SignalR / WebSockets):**
  - Connessione bidirezionale persistente tra Client e Server.
  - Ricezione immediata a terminale di notifiche push live all'accredito di un bonifico in entrata.

---

## Tecnologie di Riferimento

- **Linguaggio & Runtime:** C# 12 / .NET 10.0 (LTS)
- **Backend Framework:** ASP.NET Core Web API
- **ORM & Data Access:** Entity Framework Core (EF Core) con provider SQLite
- **Sicurezza & Crittografia:** JSON Web Token (JWT), Hashing crittografico password, SHA-256 Hash Chaining
- **Documentazione API:** OpenAPI nativo e interfaccia moderna **Scalar**
- **Comunicazione di Rete:** `HttpClient`, `System.Net.Http.Json`, WebSockets (SignalR)
- **Pattern Architetturali:** Client-Server, MVC, DTO Pattern, Repository Pattern, ACID Transactions, RBAC, Idempotency Pattern, Tamper-Evident Ledger, Pure Dependency Injection

---

## Come Avviare la Versione Attuale (Beta v0.5)

### Prerequisiti
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download) installato sul sistema.
- Visual Studio 2022/2026 con carichi di lavoro Web e Desktop.

### Esecuzione Multipla (Server + Client)
1. Clona il repository:
   ```bash
   git clone https://github.com/Andr3a19/DigitalWallet.git
   cd DigitalWallet
   ```
2. Apri la soluzione `DigitalWallet.sln` in Visual Studio.
3. Fai clic destro sulla Soluzione in *Esplora soluzioni* > **Proprietà** > **Progetto di avvio**:
   - Seleziona **Progetti di avvio multipli**.
   - Imposta l'azione su **Avvia** sia per `DigitalWallet.Api` sia per `DigitalWallet.Client`.
4. Premi **F5**:
   - Il Server avvierà le API e l'interfaccia **Scalar** all'indirizzo `https://localhost:7164/scalar/v1`.
   - Il Client aprirà la propria finestra da terminale interrogando il database SQLite via rete.