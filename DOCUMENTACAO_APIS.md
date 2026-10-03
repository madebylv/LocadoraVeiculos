# Documentação Completa das APIs - Locadora de Veículos

**Projeto**: Sistema de Gestão de Locadora de Veículos (LocadoraVeiculos)  
**Instituição**: PUC Minas - Tecnologia em Análise e Desenvolvimento de Sistemas (TADS)  
**Versão da API**: v1.0  
**Framework**: ASP.NET Core (.NET 8.0) com Entity Framework Core e SQL Server  
**Documentação Interativa (Swagger UI)**: `http://localhost:5000/`  

---

## 1. Visão Geral da Arquitetura

A API foi projetada segundo os princípios da arquitetura **RESTful**, utilizando o protocolo HTTP/HTTPS com troca de mensagens no formato **JSON** (`application/json`). Todas as operações CRUD (Create, Read, Update, Delete) e consultas avançadas estão organizadas em controladores especializados e integradas ao banco de dados relacional via Entity Framework Core.

### 1.1. Padrões de Resposta HTTP

A API segue rigorosamente as convenções de códigos de status HTTP:

| Código HTTP | Significado | Situação de Aplicação |
|---|---|---|
| `200 OK` | Sucesso na requisição | Operações de consulta (`GET`), atualizações parciais ou processamentos com retorno de payload. |
| `201 Created` | Recurso criado com sucesso | Cadastro bem-sucedido (`POST`), retornando o cabeçalho `Location` e o objeto criado. |
| `204 No Content` | Sucesso sem corpo de retorno | Atualizações integrais (`PUT`) ou exclusões (`DELETE`) concluídas. |
| `400 Bad Request` | Requisição inválida | Dados inconsistentes, violação de unicidade (ex: CPF duplicado, Placa duplicada) ou IDs de rota incompatíveis. |
| `404 Not Found` | Recurso não encontrado | Identificador (ID) consultado não existe na base de dados. |
| `500 Internal Server Error` | Erro interno do servidor | Exceções não tratadas durante o processamento no banco ou regra de negócio. |

---

## 2. Modelos de Dados (Schemas / Entidades)

### 2.1. Fabricante
Representa as montadoras/fabricantes dos veículos.
- `id` (int, PK autoincremento): Identificador único.
- `nome` (string, max: 100, obrigatório): Nome da fabricante.
- `paisOrigem` (string, max: 50, opcional): País de origem da marca.

### 2.2. Categoria
Define a classificação e o valor base da locação.
- `id` (int, PK autoincremento): Identificador único.
- `nome` (string, max: 50, obrigatório): Nome da categoria (ex: Econômico, SUV, Luxo).
- `descricao` (string, max: 200, opcional): Detalhes da categoria.
- `valorDiariaBase` (decimal, obrigatório): Valor cobrado por dia de locação.

### 2.3. Cliente
Representa a pessoa física que realiza a locação.
- `id` (int, PK autoincremento): Identificador único.
- `nome` (string, max: 150, obrigatório): Nome completo do cliente.
- `cpf` (string, max: 14, obrigatório, único): CPF do cliente (com ou sem máscara).
- `email` (string, max: 100, obrigatório, formato email): E-mail de contato.
- `telefone` (string, max: 20, opcional): Telefone de contato.

### 2.4. Veiculo
Representa os automóveis disponíveis na frota da locadora.
- `id` (int, PK autoincremento): Identificador único.
- `modelo` (string, max: 100, obrigatório): Modelo comercial do carro.
- `anoFabricacao` (int, obrigatório): Ano de fabricação do veículo.
- `quilometragem` (int, obrigatório): Quilometragem atual do odômetro.
- `placa` (string, max: 8, obrigatório, único): Placa do veículo.
- `cor` (string, max: 30, opcional): Cor predominante.
- `fabricanteId` (int, FK obrigatória): ID do fabricante vinculado.
- `categoriaId` (int, FK obrigatória): ID da categoria vinculada.

### 2.5. Aluguel
Registra o contrato de locação e devolução.
- `id` (int, PK autoincremento): Identificador único do aluguel.
- `dataInicio` (DateTime, obrigatório): Data e hora de retirada do veículo.
- `dataPrevistaDevolucao` (DateTime, obrigatório): Data prevista para devolução.
- `dataDevolucao` (DateTime, opcional): Data real da devolução do veículo.
- `valorDiaria` (decimal, obrigatório): Valor da diária contratada.
- `valorTotal` (decimal, opcional): Valor final calculado após a devolução.
- `quilometragemInicial` (int, obrigatório): Quilometragem no momento da retirada.
- `quilometragemFinal` (int, opcional): Quilometragem no momento da devolução.
- `clienteId` (int, FK obrigatória): ID do cliente locatário.
- `veiculoId` (int, FK obrigatória): ID do veículo locado.

---

## 3. Catálogo Completo de Endpoints

### 3.1. Fabricantes (`/api/Fabricantes`)

#### `GET /api/Fabricantes`
- **Descrição**: Retorna a lista completa de fabricantes cadastrados.
- **Parâmetros**: Nenhum.
- **Códigos de Resposta**:
  - `200 OK`: Lista de fabricantes.
  - `500 Internal Server Error`: Erro no banco de dados.
- **Exemplo de Retorno (200 OK)**:
```json
[
  {
    "id": 1,
    "nome": "Toyota",
    "paisOrigem": "Japão"
  },
  {
    "id": 2,
    "nome": "Volkswagen",
    "paisOrigem": "Alemanha"
  }
]
```

#### `GET /api/Fabricantes/{id}`
- **Descrição**: Obtém os dados de um fabricante específico pelo seu ID.
- **Parâmetros**:
  - `id` (Rota, int, obrigatório): Identificador do fabricante.
- **Códigos de Resposta**:
  - `200 OK`: Fabricante encontrado.
  - `404 Not Found`: Fabricante inexistente.

#### `POST /api/Fabricantes`
- **Descrição**: Cadastra um novo fabricante.
- **Parâmetros**:
  - Corpo da Requisição (`application/json`):
```json
{
  "nome": "Toyota",
  "paisOrigem": "Japão"
}
```
- **Códigos de Resposta**:
  - `201 Created`: Fabricante cadastrado com sucesso.
  - `400 Bad Request`: Dados obrigatórios não preenchidos.
  - `500 Internal Server Error`: Falha ao persistir dados.

#### `PUT /api/Fabricantes/{id}`
- **Descrição**: Atualiza os dados de um fabricante existente.
- **Parâmetros**:
  - `id` (Rota, int, obrigatório): ID do fabricante.
  - Corpo da Requisição (`application/json`):
```json
{
  "id": 1,
  "nome": "Toyota Motors do Brasil",
  "paisOrigem": "Japão / Brasil"
}
```
- **Códigos de Resposta**:
  - `204 No Content`: Atualização realizada com sucesso.
  - `400 Bad Request`: Incompatibilidade de IDs ou dados inválidos.
  - `404 Not Found`: Fabricante não encontrado.

#### `DELETE /api/Fabricantes/{id}`
- **Descrição**: Exclui um fabricante pelo ID (com validação de integridade referencial).
- **Parâmetros**:
  - `id` (Rota, int, obrigatório): ID do fabricante a excluir.
- **Códigos de Resposta**:
  - `204 No Content`: Fabricante excluído.
  - `400 Bad Request`: Fabricante possui veículos vinculados e não pode ser removido.
  - `404 Not Found`: Fabricante não encontrado.

---

### 3.2. Categorias (`/api/Categorias`)

#### `GET /api/Categorias`
- **Descrição**: Lista todas as categorias de veículos.
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Categorias/{id}`
- **Descrição**: Busca os dados de uma categoria por ID.
- **Códigos de Resposta**: `200 OK`, `404 Not Found`.

#### `POST /api/Categorias`
- **Descrição**: Cadastra uma nova categoria de veículo.
- **Exemplo de Corpo**:
```json
{
  "nome": "SUV Compacto",
  "descricao": "Veículos utilitários esportivos com excelente conforto e espaço",
  "valorDiariaBase": 180.00
}
```
- **Códigos de Resposta**: `201 Created`, `400 Bad Request`, `500 Internal Server Error`.

#### `PUT /api/Categorias/{id}`
- **Descrição**: Atualiza dados da categoria.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

#### `DELETE /api/Categorias/{id}`
- **Descrição**: Remove uma categoria caso não possua veículos atrelados.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

---

### 3.3. Clientes (`/api/Clientes`)

#### `GET /api/Clientes`
- **Descrição**: Lista todos os clientes cadastrados.
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Clientes/{id}`
- **Descrição**: Retorna o cliente especificado pelo ID.
- **Códigos de Resposta**: `200 OK`, `404 Not Found`.

#### `POST /api/Clientes`
- **Descrição**: Cadastra um novo cliente com validação de unicidade de CPF.
- **Exemplo de Corpo**:
```json
{
  "nome": "Lucas Ribeiro Silva",
  "cpf": "123.456.789-00",
  "email": "lucas.silva@email.com",
  "telefone": "(31) 98765-4321"
}
```
- **Códigos de Resposta**:
  - `201 Created`: Cliente cadastrado com sucesso.
  - `400 Bad Request`: CPF já cadastrado no sistema ou modelo inválido.
  - `500 Internal Server Error`: Erro no banco de dados.

#### `PUT /api/Clientes/{id}`
- **Descrição**: Atualiza os dados de um cliente existente.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

#### `DELETE /api/Clientes/{id}`
- **Descrição**: Remove o cliente se ele não possuir histórico de aluguéis.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

---

### 3.4. Veículos (`/api/Veiculos`)

#### `GET /api/Veiculos`
- **Descrição**: Lista todos os veículos cadastrados incluindo os objetos aninhados de Fabricante e Categoria.
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Veiculos/{id}`
- **Descrição**: Retorna os detalhes de um veículo específico por ID.
- **Códigos de Resposta**: `200 OK`, `404 Not Found`.

#### `POST /api/Veiculos`
- **Descrição**: Cadastra um novo veículo, validando existência do Fabricante, da Categoria e unicidade da Placa.
- **Exemplo de Corpo**:
```json
{
  "modelo": "Corolla Cross XRE 2.0",
  "anoFabricacao": 2024,
  "quilometragem": 15000,
  "placa": "BRA2E19",
  "cor": "Prata",
  "fabricanteId": 1,
  "categoriaId": 1
}
```
- **Códigos de Resposta**: `201 Created`, `400 Bad Request`, `500 Internal Server Error`.

#### `PUT /api/Veiculos/{id}`
- **Descrição**: Atualiza os dados do veículo.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

#### `DELETE /api/Veiculos/{id}`
- **Descrição**: Exclui o veículo se não houver histórico de aluguel associado.
- **Códigos de Resposta**: `204 No Content`, `400 Bad Request`, `404 Not Found`.

---

### 3.5. Aluguéis (`/api/Alugueis`)

#### `GET /api/Alugueis`
- **Descrição**: Lista todos os contratos de locação com dados completos de Cliente e Veículo.
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Alugueis/{id}`
- **Descrição**: Obtém detalhes de um aluguel por ID.
- **Códigos de Resposta**: `200 OK`, `404 Not Found`.

#### `POST /api/Alugueis`
- **Descrição**: Registra uma locação de veículo.
- **Exemplo de Corpo**:
```json
{
  "dataInicio": "2026-10-01T08:00:00Z",
  "dataPrevistaDevolucao": "2026-10-06T08:00:00Z",
  "dataDevolucao": null,
  "valorDiaria": 180.00,
  "valorTotal": null,
  "quilometragemInicial": 15000,
  "quilometragemFinal": null,
  "clienteId": 1,
  "veiculoId": 1
}
```
- **Códigos de Resposta**: `201 Created`, `400 Bad Request`, `500 Internal Server Error`.

#### `PUT /api/Alugueis/{id}/devolucao`
- **Descrição**: Registra a devolução do veículo, calcula automaticamente os dias utilizados e o valor total (`totalDias * valorDiaria`), e sincroniza a quilometragem do veículo no estoque.
- **Parâmetros**:
  - `id` (Rota, int): ID do aluguel.
  - `dataDevolucao` (Query, DateTime): Data/hora real da devolução.
  - `quilometragemFinal` (Query, int): Quilometragem no odômetro na entrega.
- **Códigos de Resposta**:
  - `200 OK`: Devolução concluída com sucesso e cálculo financeiro realizado.
  - `400 Bad Request`: Quilometragem final inferior à inicial.
  - `404 Not Found`: Aluguel não localizado.

#### `DELETE /api/Alugueis/{id}`
- **Descrição**: Exclui um registro de locação pelo ID.
- **Códigos de Resposta**: `204 No Content`, `404 Not Found`.

---

### 3.6. Consultas Avançadas (`/api/Consultas`)

#### `GET /api/Consultas/veiculos-detalhados`
- **Tipo de Junção**: `INNER JOIN` (Veículos + Fabricantes + Categorias).
- **Parâmetros**: `fabricante` (Query, string, opcional), `modelo` (Query, string, opcional).
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Consultas/alugueis-por-cliente`
- **Tipo de Junção**: `INNER JOIN` (Aluguéis + Clientes + Veículos).
- **Parâmetros**: `cpf` (Query, string, opcional).
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Consultas/total-gasto-por-cliente`
- **Tipo de Junção**: `INNER JOIN` + Agrupamento (`group by` Cliente) + `Sum(ValorTotal)`.
- **Parâmetros**: `valorMinimo` (Query, decimal, padrão: 0).
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Consultas/clientes-com-ou-sem-aluguel`
- **Tipo de Junção**: `LEFT OUTER JOIN` (Clientes com GroupJoin em Aluguéis e DefaultIfEmpty).
- **Parâmetros**: `apenasSemAluguel` (Query, bool, padrão: false).
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.

#### `GET /api/Consultas/veiculos-disponibilidade`
- **Tipo de Junção**: `LEFT OUTER JOIN` (Veículos com Aluguéis ativos não devolvidos).
- **Parâmetros**: `apenasDisponiveis` (Query, bool, padrão: true).
- **Códigos de Resposta**: `200 OK`, `500 Internal Server Error`.
