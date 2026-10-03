# DigitalWallet - Piattaforma Client-Server per Portafoglio Digitale

Un'applicazione finanziaria ad architettura Client-Server sviluppata in **C#** e **.NET**.

**Stato del Progetto:** In fase di sviluppo (Work in Progress)

---

## Panoramica del Progetto
Il progetto implementa un sistema finanziario completo composto da due applicazioni indipendenti che comunicano in rete:
- **Backend (Server):** API Web sviluppata con **ASP.NET Core**, documentazione interattiva tramite **Scalar** e persistenza su database relazionale **SQLite** gestito con **Entity Framework Core (EF Core)**.
- **Logica di Business e Sicurezza:** gestione di depositi, trasferimenti di denaro peer-to-peer (P2P) con transazioni conformi ai principi ACID e tracciamento dell'estratto conto.
- **Frontend (Client):** applicazione client autonoma in C# che interagisce con il backend tramite protocollo HTTP utilizzando `HttpClient`.

---

## Tabella di Marcia (Roadmap)
- [x] Setup iniziale del progetto (ASP.NET Core Web API e OpenAPI/Scalar)
- [x] Definizione dei modelli di dominio (`Wallet`, `Transaction`)
- [ ] Configurazione del database relazionale (`WalletDbContext` e SQLite)
- [ ] Implementazione dei Controller REST API (creazione wallet, depositi, trasferimenti)
- [ ] Sviluppo dell'applicazione Client