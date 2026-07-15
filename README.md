# Portfolio Analytics API

Uma Web API em .NET 8 robusta e escalável, focada em algoritmos de finanças quantitativas, análise de risco de mercado e otimização de rebalanceamento de carteiras de investimentos de acordo com alocações-alvo.

---

## Como Executar o Projeto

### Pré-requisitos

* [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) instalado.

### Rodando a API

1. Pelo terminal, entre no diretório da API:
```bash
cd src/PortfolioAnalytics.API

```


2. Execute a aplicação:
```bash
dotnet run

```


3. Abra seu navegador e acesse a URL exibida no console (ex: `http://localhost:5123`). O Swagger UI carregará automaticamente na página raiz!

---

## Executando os Testes e Cobertura (Coverage)

Tanto os testes unitários (lógicas matemáticas em memória) quanto os testes de integração (simulações de rotas HTTP com servidores virtuais) podem ser disparados com um único comando na raiz da solução:

```bash
# Executa todos os 21 testes de uma vez
dotnet test

```

### Coleta de Cobertura de Código (Code Coverage)

Para coletar a cobertura excluindo de forma definitiva as classes do projeto de testes para não distorcer as métricas reais:

```bash
dotnet test /p:CollectCoverage=true /p:Exclude="[PortfolioAnalytics.Tests]*"

```

---

## Diferenciais Técnicos da Entrega

- **Design de Código Baseado em Princípios S.O.L.I.D.**: Separação clara de responsabilidades, dependências invertidas (`IDataContext`, `IPerformanceCalculator`, etc.), permitindo testar algoritmos matemáticos puramente em memória.
- **100% de Cobertura de Testes (Core)**: Lógica matemática complexa de performance, volatilidade e otimização coberta por testes unitários e funcionais abrangendo todos os *edge cases* e caminhos críticos.
- **Ambiente de Testes de Integração Robusto**: Servidor web simulado em memória usando `WebApplicationFactory` para testar as controllers do ponto de vista do cliente HTTP.
- **Mitigação Defensiva de Inconsistências de Dados**: O sistema possui mecanismos automáticos para normalizar metas financeiras (`TargetAllocation`) cuja soma divirja de 100%, tratamento seguro de caracteres maiúsculos/minúsculos em símbolos e descarte de dados corrompidos.
- **Swagger/OpenAPI UI Interativo**: Documentação de endpoints enriquecida automaticamente com comentários XML do código fonte e disponível por padrão na URL raiz da API.

---

## Arquitetura de Pastas (Clean Single-Project)

Buscando o equilíbrio ideal entre rapidez de entrega de uma POC e os benefícios de isolamento da Clean Architecture, a estrutura de pastas foi organizada da seguinte forma:

```text
portfolio-analytics-api/
├── PortfolioAnalytics.sln
├── src/
│   └── PortfolioAnalytics.API/
│       ├── Controllers/      # Exposição das rotas HTTP (Thin Controllers)
│       ├── Data/             # Gerenciamento em memória e carga do SeedData.json
│       ├── Models/           # Classes de domínio da aplicação
│       │   └── DTOs/         # Objetos de transporte de dados e entrada/saída
│       ├── Services/         # Motores de cálculo e lógica quantitativa (Use Cases)
│       ├── Program.cs        # Pipeline HTTP e Container de DI
│       └── SeedData.json     # Base de dados em memória
└── tests/
    └── PortfolioAnalytics.Tests/
        ├── IntegrationTests/ # Testes de fluxo de API ponta a ponta
        └── ServicesTests/    # Testes unitários com mocks dos motores de cálculo

```

---

## Modelagem Matemática e Fórmulas Financeiras

### 1. Performance e Retorno Anualizado

Mapeia a valorização da carteira comparando o capital inicialmente investido contra as cotações atuais de mercado.

* **Valor Atual do Portfólio ($V_{total}$):**

$$V_{total} = \sum_{i=1}^{n} (Q_i \times P_{atual\_i})$$



*(Onde $Q_i$ é a quantidade do ativo $i$ e $P_{atual\_i}$ o seu preço atualizado).*
* **Retorno Total ($R_{total}$):**

$$R_{total} = \frac{V_{total} - I_{total}}{I_{total}} \times 100$$



*(Onde $I_{total}$ é o investimento inicial de custo médio do portfólio).*
* **Retorno Anualizado ($R_{anual}$):**
Calculado de forma exponencial baseando-se no tempo corrido desde a criação da carteira:

$$R_{anual} = \left( \left(1 + \frac{R_{total}}{100}\right)^{\frac{365}{D}} - 1 \right) \times 100$$



*(Onde $D$ é a quantidade de dias decorridos entre a criação da carteira e a data de cálculo).*

---

### 2. Volatilidade Anualizada do Portfólio (Método do Portfólio Sintético)

Avalia o desvio padrão dos retornos combinados do portfólio com base nas suas variações diárias sequenciais de preço.

* **Retorno Diário do Ativo ($R_{t}$):**

$$R_{t} = \frac{P_t - P_{t-1}}{P_{t-1}}$$


* **Retorno Diário do Portfólio Sintético ($R_{p,t}$):**

$$R_{p,t} = \sum_{i=1}^{n} (R_{i,t} \times w_i)$$



*(Onde $R_{i,t}$ é o retorno diário do ativo $i$ no dia $t$, e $w_i$ é o peso financeiro atual do ativo no portfólio).*
* **Desvio Padrão Amostral Diário ($\sigma_{diário}$):**

$$\sigma_{diário} = \sqrt{\frac{\sum_{t=1}^{N} (R_{p,t} - \bar{R}_p)^2}{N - 1}}$$



*(Onde $\bar{R}_p$ é a média simples dos retornos do portfólio no período observado e $N$ o total de dias de histórico).*
* **Volatilidade Anualizada ($\sigma_{anual}$):**
Ajustada considerando a premissa padrão de **252 dias úteis** por ano financeiro:

$$\sigma_{anual} = \sigma_{diário} \times \sqrt{252} \times 100$$



*Nota de Robustez:* Se algum ativo do portfólio não possuir o histórico de preços mínimo necessário ($N < 2$), o indicador geral de volatilidade é reportado como `null` para evitar distorções de amostra.

---

### 3. Índice de Concentração de Herfindahl-Hirschman (HHI)

Métrica acadêmica robusta utilizada para qualificar o nível de diversificação e a exposição concentrada de ativos na carteira.

* **Fórmula do HHI:**

$$HHI = \sum_{i=1}^{n} (W_i)^2$$



*(Onde $W_i$ representa o peso decimal do ativo $i$ em relação ao valor total da carteira. Ex: $20\% = 0.20$).*
* **Classificação de Risco pelo HHI:**
* **$HHI < 0.15$ (Baixo Risco / Altamente Diversificado):** A exposição individual do portfólio está pulverizada.
* **$0.15 \le HHI \le 0.25$ (Médio Risco / Concentração Moderada):** Alerta amarelo para exposição em poucas teses.
* **$HHI > 0.25$ (Alto Risco / Extremamente Concentrado):** Carteira excessivamente vulnerável ao risco específico de poucos ativos.


* **Regras Defensivas Adicionais:**
* **Alerta de Ativo Único:** Disparado quando um único ativo representa $> 20\%$ do portfólio total.
* **Alerta Setorial:** Disparado se a exposição concentrada em um único setor ultrapassar o limite recomendado de $> 40\%$.



---

### 4. Algoritmo de Otimização de Rebalanceamento Proporcional

Encontra os desvios contra a meta de investimento (`TargetAllocation`) e sugere transações inteiras para retornar ao equilíbrio.

* **Mitigação Defensiva de Normalização:**
Se a soma do $TargetAllocation$ das posições não totalizar exatamente $100\%$ ($1.0$), o otimizador calcula o fator de normalização de forma proporcional:

$$TargetNormalizado_i = \frac{TargetOriginal_i}{\sum_{j=1}^{n} TargetOriginal_j}$$


* **Divergência Financeira ($D_i$):**

$$D_i = (V_{total} \times TargetNormalizado_i) - V_{real\_i}$$


* **Sugestão de Cotas Operacionais ($Q_i$):**

$$Q_i = \text{Arredondar para o inteiro mais próximo}\left(\frac{|D_i|}{P_{atual\_i}}\right)$$


* Se $D_i > 0$ e $Q_i \ge 1$ $\rightarrow$ Sugerir ação de **compra (BUY)**.
* Se $D_i < 0$ e $Q_i \ge 1$ $\rightarrow$ Sugerir ação de **venda (SELL)**.


## 🛠️ Tecnologias Utilizadas

* **C# e .NET 8** (ASP.NET Core Minimal APIs/Controllers)
* **xUnit** (Framewok de Testes de Alta Performance)
* **Microsoft.AspNetCore.Mvc.Testing** (Mocks Web Server para testes funcionais/integração)
* **System.Text.Json** (Serializador e Parser JSON nativo de alta performance)
* **Swashbuckle.AspNetCore** (Motor Swagger/OpenAPI)
