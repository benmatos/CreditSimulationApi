# CreditSimulationApi

API para simulação de crédito, desenvolvida em C#.

## Descrição

A **CreditSimulationApi** fornece endpoints para simular propostas de crédito, facilitando o cálculo de condições, valores e aprovação de propostas conforme critérios configuráveis. É adequada para integrações com sistemas financeiros, fintechs, bancos e aplicações que necessitam de análise de crédito automatizada.

## Principais Funcionalidades

- Simulação de propostas de crédito com diferentes parâmetros.
- Cálculo de taxas, parcelas e condições de pagamento.
- Regras de aprovação automática baseadas em critérios configuráveis.
- API estruturada para integração com sistemas externos.

## Tecnologias Utilizadas

- **C#** (.NET)
- Docker (opcional para deploy)
- RESTful APIs

## Como Executar

### Pré-requisitos

- [.NET 7.0+](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) (opcional)

### Executando localmente

1. Clone o repositório:
    ```bash
    git clone https://github.com/benmatos/CreditSimulationApi.git
    cd CreditSimulationApi
    ```
2. Execute o projeto:
    ```bash
    dotnet run
    ```

### Usando Docker

```bash
docker build -t creditsimulationapi .
docker run -p 8080:80 creditsimulationapi
```

## Endpoints Principais

| Método | Rota                | Descrição                        |
|--------|---------------------|----------------------------------|
| POST   | /api/proposal       | Simula uma nova proposta de crédito |
| GET    | /api/proposal/{id}  | Obtém detalhes de uma proposta   |

*Veja a documentação Swagger gerada automaticamente acessando `/swagger` quando o serviço estiver rodando.*

## Contribuição

1. Faça um fork
2. Crie uma branch (`git checkout -b feature/sua-feature`)
3. Commit suas alterações (`git commit -am 'feat: minha feature'`)
4. Push para a branch (`git push origin feature/sua-feature`)
5. Abra um Pull Request

## Licença

Este projeto está sob licença privada.

---

> Mais informações ou dúvidas, entre em contato pelo [GitHub](https://github.com/benmatos).
