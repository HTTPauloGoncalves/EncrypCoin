# 🪙 EncrypCoin

Monitoramento de criptomoedas com preços em tempo real, alertas personalizados e integração com CoinGecko, Binance e CryptoPanic.
Desenvolvido em **ASP.NET + PostgreSQL + Redis + Docker**, com frontend em **HTML, JS e TailwindCSS**.

---

### 🚀 Tecnologias

| Camada             | Tecnologias                               |
| ------------------ | ----------------------------------------- |
| **Frontend**       | HTML, JavaScript, TailwindCSS             |
| **Backend (API)**  | ASP.NET 9 (C#), SignalR (WebSockets), JWT |
| **Banco**          | PostgreSQL                                |
| **Cache**          | Redis                                     |
| **Infraestrutura** | Docker, Docker Compose                    |
| **APIs externas**  | CoinGecko, Binance, CryptoPanic           |

---

### 🧱 Arquitetura

```mermaid
flowchart TD
  A[Frontend JS + Tailwind] -->|HTTP/JSON| B[ASP.NET API]
  B -->|Consulta externa| C[CoinGecko / Binance API]
  B --> D[Redis Cache]
  B --> E[PostgreSQL - Usuários / Alertas]
  B -->|WebSocket| A
```

---

### ⚙️ Estrutura do Projeto

```
/encrypcoin/
├── backend/               # ASP.NET API
│   ├── Controllers/
│   ├── Models/
│   ├── Services/
│   └── Dockerfile
│
├── frontend/              # HTML + JS + Tailwind
│   ├── index.html
│   ├── js/
│   ├── css/
│   └── Dockerfile
│
├── docker-compose.yml
└── README.md
```

---

### 🧠 Funcionalidades

* ✅ Preços e variações em tempo real
* 🔐 Login e autenticação JWT
* 💾 Cache inteligente com Redis
* 📊 Histórico e gráficos de moedas
* 🔔 Alertas personalizados
* 🌐 Feed de notícias de mercado

---

### 🐳 Como rodar o projeto

```bash
# 1. Clone o repositório
git clone https://github.com/HTTPauloGoncalves/EncrypCoin
cd EncrypCoin\Backend\EncrypCoin.API\EncrypCoin.API

# 2. Suba redis
docker pull redis
docker run --name redis-server -p 6379:6379 -d redis

# 3. Ajuste o appsettings e rode o projeto
"Redis": "localhost:6379"
```
---

### 📈 Próximos passos

* [ ] Adicionar painel de alertas
* [ ] Gráficos interativos com Chart.js
* [ ] WebSocket em tempo real (Binance)
* [ ] Deploy na AWS (EC2 + S3 + Route53)
* [ ] Dark/Light mode
* [ ] Testes automatizados
* [ ] Notificações via e-mail ou push
* [ ] Integração com múltiplas exchanges
* [ ] Painel de estatísticas avançadas
* [ ] Dashboard mobile-friendly
* [ ] Documentação da API pública
* [ ] Suporte a múltiplas moedas fiduciárias
