# DigitalWallet - Sistema Client-Server per Conti e Bonifici P2P

Un'applicazione distribuita sviluppata in **C# 14** e **.NET 10 (LTS)** che implementa un'architettura **Client-Server** per la gestione di portafogli digitali, autenticazione crittografica stateless, trasferimenti monetari P2P e gestione della concorrenza.

> **Stato del Progetto:**  
> • **Versione v0.5 (Core Transaction Engine):** Completata  
> • **Versione v0.8 (Sicurezza, RBAC, Concorrenza & Multi-Valuta):** Completata  
> • **Versione v1.0 (Cambio Valuta, Recipient Verification, Audit Trail & Real-Time):** In fase di sviluppo  
>   - Motore di Cambio Valuta: **Completato**  
>   - Separazione Cambio Interno / Bonifico P2P con Recipient Verification: **In sviluppo**  
>   - Idempotenza, Audit Trail, Backoffice, SignalR: **In sviluppo**

---

## Architettura del Sistema

La soluzione separa nettamente il backend REST dalla logica di presentazione client:

```text
                                 ┌─────────────────────────────────────────────────┐
                                 │            SERVER (DigitalWallet.Api)           │
                                 │  • ASP.NET Core REST API (.NET 10 LTS)          │
                                 │  • Entity Framework Core + SQLite               │
                                 │  • Autenticazione JWT & RBAC (Admin/Customer)   │
                                 │  • Concurrency Control & Deadlock Avoidance     │
                                 └────────────────────────▲────────────────────────┘
                                                          │
                             Canale HTTP / REST           │     Canale HTTP / REST
                             (Bearer Token: Admin)        │     (Bearer Token: Customer)
                                                          │
                       ┌──────────────────────────────────┴──────────────────────────────────┐
                       │                                                                     │
            ┌──────────┴───────────────┐                                          ┌──────────┴───────────────┐
            │   CLIENT 1: BACKOFFICE   │                                          │     CLIENT 2: CLI APP    │
            │  (Operatore / Amministr.) │                                          │     (Cliente Privato)    │
            │     [In Sviluppo v1.0]   │                                          │       [Attivo v0.8]      │
            ├──────────────────────────┤                                          ├──────────────────────────┤
            │ • Monitoraggio globale   │                                          │ • Login & Registrazione  │
            │ • Anagrafica conti       │                                          │ • Apertura e saldo conti │
            │ • Audit Log verificabile │                                          │ • Bonifici P2P immediati │
            │ • Gestione tassi cambio  │                                          │ • Estratto conto storico │
            └──────────────────────────┘                                          └──────────────────────────┘
```

---

## Soluzioni Ingegneristiche e Scelte di Design

### 1. Dominio Incapsulato (Rich Domain Model)
Le entità di dominio (`Wallet`, `Transaction`, `User`) non sono anemiche:
- Lo stato interno è protetto da mutazioni incontrollate (`private set` / `init`).
- I metodi di business (`Credit`, `Debit`) impongono il rispetto delle invarianti di dominio (importi strettamente positivi, divieto di scoperto per saldo insufficiente).
- La logica di consistenza risiede nell'entità stessa, non dispersa nei controller.

### 2. Controllo della Concorrenza e Prevenzione Deadlock
- **Optimistic Concurrency Control:** Ogni conto dispone di un `ConcurrencyToken` (GUID) mappato su EF Core. In caso di transazioni o depositi simultanei sullo stesso conto, il conflitto viene intercettato tramite `DbUpdateConcurrencyException`, impedendo fenomeni di double-spending o race condition.
- **Deadlock Avoidance:** Nei trasferimenti P2P tra due conti, i record vengono acquisiti secondo un ordinamento deterministico basato sulle chiavi primarie (`Math.Min` e `Math.Max`), eliminando il rischio di deadlock a lock incrociato in scenari ad alta concorrenza.

### 3. Transazionalità ACID
I bonifici tra portafogli sono protetti da transazioni esplicite del database (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`), garantendo che addebito del mittente, accredito del destinatario e doppie registrazioni contabili siano indivisibili.

### 4. Sicurezza Applicativa e Data Isolation
- **Autenticazione Stateless JWT:** Validazione rigorosa tramite chiave simmetrica HMAC-SHA256 e durata controllata del token.
- **Protezione Credenziali:** Hashing crittografico salato PBKDF2 tramite `PasswordHasher<T>` di ASP.NET Core Identity.
- **Prevenzione IDOR / BOLA:** Controllo di autorizzazione a livello di singola risorsa tramite i claim d'identità (`NameIdentifier`), impedendo a un utente di visualizzare o movimentare conti altrui.
- **Principio del Privilegio Minimo:** La registrazione pubblica assegna in modo forzato il ruolo `Customer`, bloccando tentativi di privilege escalation verso `Admin`.

### 5. Motore di Cambio Valuta (scelta deliberata per la demo)
Il servizio `CurrencyExchangeService` implementa la conversione EUR↔CHF applicata automaticamente sui bonifici cross-currency. I tassi sono **fissi e hardcoded**:
- EUR → CHF: 0.95
- CHF → EUR: 1.05

Arrotondamento monetario con `MidpointRounding.AwayFromZero` (convenzione bancaria) per garantire coerenza con i centesimi.

Il client visualizza, per i bonifici cross-currency, l'importo inviato, il tasso applicato, l'importo ricevuto e il saldo residuo. Per i bonifici in valuta identica il tasso non viene mostrato (sarebbe rumore informativo).

Motivazione della scelta di tassi fissi (invece di un'API esterna):
- Restare su free tier (nessuna dipendenza da servizi a pagamento)
- Determinismo nei test (stessi input → stessi output, indipendentemente dalla rete)
- Il focus del progetto è la logica di conversione, non l'integrazione con provider esterni

In un contesto di produzione si integrerebbe un provider (es. exchangerate-api.com) con caching e fallback su tasso salvato.

---

## Tabella di Marcia (Roadmap)

### [x] Fase 1: Beta v0.5 - Architettura di Base & Gestione Transazioni (Completata)
- [x] Setup dell'architettura Client-Server multi-progetto in Visual Studio (.NET Solution)
- [x] Backend **ASP.NET Core Web API** documentato tramite interfaccia interattiva **Scalar** (OpenAPI)
- [x] Persistenza relazionale su database **SQLite** con approccio Code-First tramite **Entity Framework Core (EF Core)**
- [x] Modellazione del dominio: entità `Wallet` (conti) e `Transaction` (movimenti contabili) con stato incapsulato
- [x] Protezione da attacchi di *over-posting* tramite DTO dedicati (`CreateWalletRequest`, `DepositRequest`, `TransferRequest`)
- [x] Trasferimenti P2P protetti da **transazioni atomiche ACID** (`BeginTransactionAsync`, `CommitAsync`, `RollbackAsync`) con doppia registrazione contabile
- [x] Servizio client HTTP asincrono (**`WalletApiClient`**) con gestione centralizzata delle eccezioni e dei codici di stato di rete

---

### [x] Fase 2: Versione v0.8 - Sicurezza, Identità & Concorrenza (Completata)
- [x] **Autenticazione Stateless con JWT:** Rilascio e validazione di token crittografati con firma simmetrica HMAC-SHA256 (`POST /api/auth/login`)
- [x] **Protezione Password:** Hashing crittografico con salt PBKDF2 tramite `PasswordHasher<T>` di ASP.NET Core Identity (nessun dato sensibile in chiaro)
- [x] **Role-Based Access Control (RBAC):** Protezione degli endpoint con `[Authorize]` e separazione dei ruoli (`Customer` e `Admin`)
- [x] **Data Isolation (Prevenzione IDOR/BOLA):** Verifica rigorosa della proprietà dei conti tramite i claim del token (`NameIdentifier`) per bloccare accessi non autorizzati
- [x] **Prevenzione Privilege Escalation:** Assegnazione forzata del ruolo `Customer` in fase di registrazione pubblica
- [x] **Optimistic Concurrency Control:** Token di concorrenza (`ConcurrencyToken` / GUID) su ogni conto per sventare race condition e double-spending, con gestione di `DbUpdateConcurrencyException`
- [x] **Prevenzione Deadlock su Database:** Ordinamento deterministico delle chiavi primarie (`Math.Min` / `Math.Max`) durante l'acquisizione delle risorse nei trasferimenti concorrenti
- [x] **Generatore di IBAN Sintetico:** Creazione di codici IBAN a 27 caratteri con controllo preventivo anti-collisione su indice univoco (formato dimostrativo)
- [x] **Supporto Multi-Valuta:** Modellazione dell'enum `Currency` (`EUR`, `CHF`) e conversione automatica sui trasferimenti cross-currency tramite `CurrencyExchangeService` (tassi fissi)
- [x] **Client CLI Integrato:** Interfaccia a terminale con flusso iniziale di autenticazione, iniezione del Bearer Token su `HttpClient` e parsing numerico indipendente dalla cultura dell'OS

---

### [ ] Fase 3: Versione v1.0 - Integrità dei Dati, Real-Time & Estensioni (In Sviluppo)
- [x] **Motore di Cambio Valuta:** Servizio di conversione EUR↔CHF con applicazione automatica dei tassi sui bonifici cross-currency, visualizzazione del tasso e dell'importo convertito lato client
- [ ] **Separazione Cambio Interno / Bonifico P2P con Recipient Verification:** Distinzione di dominio tra due operazioni oggi fuse in un unico endpoint:
  - Due endpoint dedicati: `POST /api/wallets/exchange` (cambio tra conti propri) e `POST /api/wallets/transfer` (bonifico verso terzi via IBAN), con logica di trasferimento condivisa in `ExecuteTransferAsync`
  - Nuovi valori `CambioInUscita` / `CambioInEntrata` in `TransactionType` per distinguere il cambio interno dal bonifico P2P nello storico
  - `TransferRequest` aggiornato: il destinatario P2P è identificato tramite `DestinationIban` (string) invece dell'ID interno
  - Endpoint `GET /api/wallets/by-iban/{iban}` con DTO ridotto (`OwnerName`, `Currency`, `Iban`) per mostrare in anteprima il titolare del conto prima del bonifico
  - Endpoint `POST /api/wallets/verify-recipient` con matching tra IBAN e nome digitato dall'utente (**Confirmation of Payee**):
    - Match esatto case-insensitive → procedi
    - Close match calcolato con distanza di Levenshtein (soglia ≤ 3) → **blocca** (comportamento scelto: prudente, nessun warning ignorabile)
    - Nessun match → **blocca**
  - Client: selezione numerata locale per i conti propri (l'utente sceglie `1`, `2`, `3` e il client mappa agli ID reali), IBAN per il destinatario terzo
  - Menu ristrutturato con due voci separate: "Cambia valuta tra i tuoi conti" e "Bonifico P2P (IBAN)"
  - Lavoro suddiviso in 4 commit atomici: selezione numerata (client), endpoint lookup + verification (server + client), split exchange/transfer (server + client), documentazione
- [ ] **Idempotenza sui Bonifici:** Tracciamento delle richieste tramite header `Idempotency-Key` (GUID) per neutralizzare doppi addebiti in caso di retry di rete
- [ ] **Audit Log Crittografico (Hash-Chain):** Concatenamento progressivo delle transazioni tramite hashing SHA-256 (`PreviousHash` e `Hash`) per rilevare manomissioni del registro contabile
- [ ] **Client Dedicato Backoffice:** Seconda interfaccia console riservata ad utenti con ruolo `Admin` per verifiche anagrafiche e audit di sistema
- [ ] **Notifiche Real-Time via SignalR:** Connessione WebSocket bidirezionale per notificare istantaneamente al client ricevente l'accredito di un bonifico

---

## Tecnologie Adottate

- **Linguaggio:** C# 14
- **Runtime:** .NET 10.0 SDK (LTS)
- **Web API:** ASP.NET Core
- **Data Access:** Entity Framework Core con provider SQLite
- **Sicurezza:** JWT Bearer Authentication, PBKDF2 Password Hashing
- **Documentazione:** Microsoft OpenAPI con Scalar UI
- **Client Networking:** `HttpClient` e `System.Net.Http.Json`

---

## Come Avviare la Soluzione

### Prerequisiti
- [.NET SDK 10.0 (LTS)](https://dotnet.microsoft.com/download) installato sul sistema.
- Visual Studio 2022 o Visual Studio Code con C# Dev Kit.

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
   - Il **Server** avvierà le API e la documentazione Scalar su `https://localhost:7164/scalar/v1`.
   - Il **Client** aprirà il terminale mostrando il flusso iniziale di Login o Registrazione.