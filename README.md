# DigitalWallet - Piattaforma Bancaria Client-Server Distribuita

Un sistema finanziario distribuito sviluppato in **C# 14** e **.NET 10 (LTS)** che implementa un'architettura **Client-Server** per la gestione di conti digitali, autenticazione crittografica, bonifici P2P e transazioni monetarie concorrenti.

> **Stato del Progetto:**  
> • **Versione v0.5 (Core Banking Engine):** Completata  
> • **Versione v0.8 (Sicurezza, RBAC, Concorrenza & Multi-Valuta):** Completata  
> • **Versione v1.0 (Ecosistema Enterprise con SignalR & Audit Trail DLT):** In fase di sviluppo  

---

## Visione Architetturale dell'Ecosistema

Il sistema simula l'infrastruttura di un istituto bancario moderno, separando nettamente il backend REST dalle applicazioni client dedicate a diversi profili utente:

```text
                                 ┌─────────────────────────────────────────────────┐
                                 │            SERVER (DigitalWallet.Api)           │
                                 │  • ASP.NET Core REST API (.NET 10 LTS)          │
                                 │  • Entity Framework Core + SQLite               │
                                 │  • Autenticazione JWT & RBAC (Admin/Customer)   │
                                 │  • Concurrency Control & Deadlock Prevention    │
                                 └────────────────────────▲────────────────────────┘
                                                          │
                             Canale HTTP / REST           │     Canale HTTP / REST
                             (Bearer Token: Admin)        │     (Bearer Token: Customer)
                                                          │
                       ┌──────────────────────────────────┴──────────────────────────────────┐
                       │                                                                     │
            ┌──────────┴───────────────┐                                          ┌──────────┴───────────────┐
            │   CLIENT 1: BACKOFFICE   │                                          │  CLIENT 2: HOME BANKING  │
            │   (Operatore di Banca)   │                                          │     (Cliente Privato)    │
            │     [In Sviluppo v1.0]   │                                          │       [Attivo v0.8]      │
            ├──────────────────────────┤                                          ├──────────────────────────┤
            │ • Anagrafica globale     │                                          │ • Login & Registrazione  │
            │ • Monitoraggio conti     │                                          │ • Gestione propri conti  │
            │ • Tassi cambio EUR/CHF   │                                          │ • Bonifici P2P con IBAN  │
            │ • Audit Ledger DLT       │                                          │ • Storico movimenti      │
            └──────────────────────────┘                                          └──────────────────────────┘
```

---

## Tabella di Marcia (Roadmap)

### [x] Fase 1: Beta v0.5 - Core Banking Engine (Completata)
- [x] Setup dell'architettura Client-Server (Soluzione multi-progetto in Visual Studio).
- [x] Backend **ASP.NET Core Web API** documentato tramite interfaccia **Scalar**.
- [x] Persistenza relazionale su database **SQLite** con approccio Code-First tramite **Entity Framework Core (EF Core)**.
- [x] Modellazione entità: `Wallet` (conti) e `Transaction` (movimenti contabili).
- [x] Protezione da attacchi di *over-posting* tramite DTO dedicati (`CreateWalletRequest`, `DepositRequest`, `TransferRequest`).
- [x] Trasferimento P2P protetto da **transazioni atomiche ACID** (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`) con doppia registrazione contabile.
- [x] Servizio client HTTP asincrono (**`WalletApiClient`**) con gestione delle eccezioni di rete.

---

### [x] Fase 2: Versione v0.8 - Sicurezza, Identità & Concorrenza (Completata)
- [x] **Autenticazione Stateless con JWT:** Rilascio e validazione di JSON Web Token crittografati con firma simmetrica HMAC-SHA256 (`POST /api/auth/login`).
- [x] **Protezione Password:** Hashing crittografico salato PBKDF2 tramite `PasswordHasher<T>` di ASP.NET Core Identity (nessuna password memorizzata in chiaro).
- [x] **Role-Based Access Control (RBAC):** Protezione degli endpoint con `[Authorize]` e separazione dei ruoli (`Customer` e `Admin`).
- [x] **Data Isolation (Prevenzione IDOR/BOLA):** Verifica rigorosa della proprietà dei conti tramite i claim del token (`NameIdentifier`) per impedire accessi o addebiti non autorizzati su conti terzi.
- [x] **Prevenzione Privilege Escalation:** Restrizione automatica del ruolo `Customer` in fase di auto-registrazione pubblica.
- [x] **Optimistic Concurrency Control:** Token di concorrenza (`ConcurrencyToken` / GUID) su ogni conto per sventare race condition e double-spending, con gestione di `DbUpdateConcurrencyException`.
- [x] **Prevenzione Deadlock su Database:** Ordinamento deterministico delle chiavi primarie (`Math.Min` / `Math.Max`) durante i trasferimenti bidirezionali concorrenti.
- [x] **Generatore di IBAN Realistico:** Assegnazione di codici IBAN a 27 caratteri con verifica preventiva anti-collisione su indice univoco.
- [x] **Supporto Multi-Valuta di Base:** Modellazione dell'enum `Currency` (`EUR`, `CHF`) con controlli di blocco sui bonifici cross-currency sprovvisti di tasso di cambio.
- [x] **Client CLI Integrato:** Interfaccia console con flusso iniziale di Login/Registrazione, iniezione del Bearer Token su `HttpClient` e parsing numerico indipendente dal sistema operativo.

---

### [ ] Fase 3: Versione v1.0 - Ecosistema Enterprise & Real-Time (In Sviluppo)
- [ ] **Motore di Cambio Valuta:** Implementazione del servizio di conversione transfrontaliero EUR/CHF con applicazione automatica dei tassi di cambio sui bonifici.
- [ ] **Idempotenza sui Bonifici:** Tracciamento delle richieste tramite header `Idempotency-Key` (GUID) per prevenire doppi addebiti in caso di retry di rete.
- [ ] **Audit Trail Crittografico (Tamper-Evident Ledger):** Concatenamento a blocchi delle transazioni tramite hashing SHA-256 (`PreviousHash` e `Hash`) per verificare l'integrità del registro contabile.
- [ ] **Client Dedicato Backoffice:** Seconda applicazione console riservata agli operatori di filiale per verifiche anagrafiche e audit di sistema.
- [ ] **Notifiche Real-Time via SignalR:** Connessione WebSocket bidirezionale per notificare istantaneamente al client ricevente l'accredito di un bonifico.

---

## Tecnologie e Principi Adottati

- **Linguaggio & Runtime:** C# 14 / .NET 10.0 (LTS)
- **Backend Web API:** ASP.NET Core con Minimal/Controller APIs
- **ORM & Data Access:** Entity Framework Core (EF Core) con provider SQLite
- **Sicurezza:** JWT Bearer Authentication, PBKDF2 Password Hashing, Claim-based Authorization
- **Documentazione Interattiva:** OpenAPI nativo con interfaccia **Scalar**
- **Networking Client:** `HttpClient` con `System.Net.Http.Json` e gestione asincrona `async/await`
- **Pattern Architetturali:** Client-Server, DTO Pattern, Rich Domain Model (Invarianti di business), ACID Transactions, Optimistic Concurrency Control, Deadlock Avoidance, Dependency Injection

---

## Come Avviare la Soluzione

### Prerequisiti
- [.NET SDK 10.0 (LTS)](https://dotnet.microsoft.com/download) installato sul sistema.
- Visual Studio 2022/2026 con carichi di lavoro *Sviluppo ASP.NET e Web*.

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
   - Il **Server** avvierà le API e l'interfaccia **Scalar** all'indirizzo `https://localhost:7164/scalar/v1`.
   - Il **Client** aprirà il terminale mostrando il menu iniziale per effettuare il **Login** o la **Registrazione** di un nuovo utente.