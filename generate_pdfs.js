const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

// Função auxiliar para converter imagem em base64
function getBase64Image(filePath) {
  if (!fs.existsSync(filePath)) return '';
  const ext = path.extname(filePath).replace('.', '');
  const data = fs.readFileSync(filePath);
  return `data:image/${ext};base64,${data.toString('base64')}`;
}

const evidenceDir = path.join(__dirname, 'docs', 'evidencias');

// 1. Template HTML para DOCUMENTAÇÃO DAS APIS
const htmlDocAPIs = `
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <title>Documentação das APIs - Locadora de Veículos</title>
  <style>
    @page {
      size: A4;
      margin: 20mm 15mm 20mm 15mm;
      @bottom-right {
        content: counter(page) " / " counter(pages);
      }
    }
    body {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
      color: #24292f;
      line-height: 1.5;
      font-size: 13px;
    }
    h1, h2, h3, h4 {
      color: #0969da;
      font-weight: 600;
      margin-top: 20px;
      margin-bottom: 8px;
    }
    h1 {
      font-size: 24px;
      border-bottom: 2px solid #0969da;
      padding-bottom: 8px;
      color: #1f2328;
    }
    h2 {
      font-size: 18px;
      border-bottom: 1px solid #d0d7de;
      padding-bottom: 6px;
      margin-top: 25px;
      page-break-after: avoid;
    }
    h3 {
      font-size: 15px;
      margin-top: 18px;
      color: #0550ae;
      page-break-after: avoid;
    }
    .header-box {
      background: linear-gradient(135deg, #0969da 0%, #0550ae 100%);
      color: white;
      padding: 20px;
      border-radius: 8px;
      margin-bottom: 25px;
    }
    .header-box h1 {
      color: white;
      border: none;
      margin: 0 0 10px 0;
      padding: 0;
    }
    .header-box p {
      margin: 4px 0;
      font-size: 13px;
      opacity: 0.95;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin: 14px 0;
      font-size: 12px;
      page-break-inside: avoid;
    }
    th, td {
      border: 1px solid #d0d7de;
      padding: 8px 10px;
      text-align: left;
    }
    th {
      background-color: #f6f8fa;
      font-weight: 600;
      color: #1f2328;
    }
    tr:nth-child(even) {
      background-color: #fcfcfc;
    }
    .badge {
      display: inline-block;
      padding: 2px 7px;
      font-size: 11px;
      font-weight: 700;
      border-radius: 4px;
      color: white;
      text-transform: uppercase;
    }
    .badge-get { background-color: #2da44e; }
    .badge-post { background-color: #0969da; }
    .badge-put { background-color: #bf8700; }
    .badge-delete { background-color: #cf222e; }
    pre, code {
      font-family: ui-monospace, SFMono-Regular, "SF Mono", Menlo, Consolas, "Liberation Mono", monospace;
      font-size: 11.5px;
    }
    code {
      background-color: #eff1f3;
      padding: 2px 5px;
      border-radius: 4px;
      color: #0969da;
    }
    pre {
      background-color: #f6f8fa;
      border: 1px solid #d0d7de;
      border-radius: 6px;
      padding: 10px 12px;
      overflow-x: auto;
      margin: 8px 0 14px 0;
      page-break-inside: avoid;
    }
    .endpoint-card {
      border: 1px solid #d0d7de;
      border-radius: 6px;
      padding: 12px 14px;
      margin: 14px 0;
      background-color: #ffffff;
      page-break-inside: avoid;
    }
    .endpoint-header {
      display: flex;
      align-items: center;
      gap: 10px;
      font-size: 14px;
      font-weight: 600;
      margin-bottom: 8px;
    }
  </style>
</head>
<body>

  <div class="header-box">
    <h1>Documentação das APIs - Locadora de Veículos</h1>
    <p><strong>Instituição:</strong> PUC Minas - Análise e Desenvolvimento de Sistemas (TADS)</p>
    <p><strong>Disciplina / Etapa:</strong> Etapa 3.2 - Documentação dos Endpoints das APIs</p>
    <p><strong>Tecnologias:</strong> ASP.NET Core (.NET 8.0), Entity Framework Core, SQL Server, Swagger OpenAPI</p>
    <p><strong>URL Base Local:</strong> <code>http://localhost:5000/api</code> | <strong>Swagger UI:</strong> <code>http://localhost:5000/</code></p>
  </div>

  <h2>1. Visão Geral da Arquitetura e Padrões REST</h2>
  <p>Esta API foi desenvolvida seguindo integralmente as diretrizes arquiteturais RESTful, permitindo a gestão centralizada e automatizada de locação de veículos, frotas, clientes e fabricantes. A comunicação é realizada via requisições HTTP seguras com payloads padronizados em formato JSON (<code>application/json</code>).</p>

  <h3>1.1. Códigos de Status HTTP Padronizados</h3>
  <table>
    <thead>
      <tr>
        <th style="width: 15%;">Código HTTP</th>
        <th style="width: 25%;">Significado</th>
        <th style="width: 60%;">Descrição e Regra de Negócio</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td><code>200 OK</code></td>
        <td>Sucesso</td>
        <td>Requisição processada com êxito. Retorna dados em consultas (<code>GET</code>) ou resultados de operações (<code>PUT</code> devolução).</td>
      </tr>
      <tr>
        <td><code>201 Created</code></td>
        <td>Criado com Sucesso</td>
        <td>Novo registro persistido com sucesso no banco (<code>POST</code>). Retorna o objeto gerado e o cabeçalho <code>Location</code>.</td>
      </tr>
      <tr>
        <td><code>204 No Content</code></td>
        <td>Sem Conteúdo</td>
        <td>Operação de atualização integral (<code>PUT</code>) ou exclusão (<code>DELETE</code>) concluída com sucesso.</td>
      </tr>
      <tr>
        <td><code>400 Bad Request</code></td>
        <td>Requisição Inválida</td>
        <td>Falha de validação de modelo, incompatibilidade de chaves, CPF duplicado, placa já em uso ou quilometragem incoerente.</td>
      </tr>
      <tr>
        <td><code>404 Not Found</code></td>
        <td>Não Encontrado</td>
        <td>O recurso solicitado com o identificador especificado não existe na base de dados.</td>
      </tr>
      <tr>
        <td><code>500 Internal Server Error</code></td>
        <td>Erro Interno</td>
        <td>Falha não esperada no processamento interno ou comunicação com o SQL Server.</td>
      </tr>
    </tbody>
  </table>

  <h2>2. Dicionário de Dados e Modelos (Schemas)</h2>
  <table>
    <thead>
      <tr>
        <th>Entidade</th>
        <th>Campo</th>
        <th>Tipo</th>
        <th>Restrições</th>
        <th>Descrição</th>
      </tr>
    </thead>
    <tbody>
      <tr>
        <td rowspan="3"><strong>Fabricante</strong></td>
        <td><code>id</code></td>
        <td>int</td>
        <td>PK, Auto-inc</td>
        <td>Identificador único do fabricante.</td>
      </tr>
      <tr>
        <td><code>nome</code></td>
        <td>string (100)</td>
        <td>Obrigatório</td>
        <td>Nome da montadora / fabricante.</td>
      </tr>
      <tr>
        <td><code>paisOrigem</code></td>
        <td>string (50)</td>
        <td>Opcional</td>
        <td>País sede da montadora.</td>
      </tr>

      <tr>
        <td rowspan="4"><strong>Categoria</strong></td>
        <td><code>id</code></td>
        <td>int</td>
        <td>PK, Auto-inc</td>
        <td>Identificador único da categoria.</td>
      </tr>
      <tr>
        <td><code>nome</code></td>
        <td>string (50)</td>
        <td>Obrigatório</td>
        <td>Nome descritivo (Ex: SUV Compacto, Sedan Executivo).</td>
      </tr>
      <tr>
        <td><code>descricao</code></td>
        <td>string (200)</td>
        <td>Opcional</td>
        <td>Detalhes dos itens de conforto e perfil de uso.</td>
      </tr>
      <tr>
        <td><code>valorDiariaBase</code></td>
        <td>decimal</td>
        <td>Obrigatório</td>
        <td>Valor padrão da diária para veículos desta categoria.</td>
      </tr>

      <tr>
        <td rowspan="5"><strong>Cliente</strong></td>
        <td><code>id</code></td>
        <td>int</td>
        <td>PK, Auto-inc</td>
        <td>Identificador único do cliente.</td>
      </tr>
      <tr>
        <td><code>nome</code></td>
        <td>string (150)</td>
        <td>Obrigatório</td>
        <td>Nome completo do cliente.</td>
      </tr>
      <tr>
        <td><code>cpf</code></td>
        <td>string (14)</td>
        <td>Obrigatório, Único</td>
        <td>Cadastro de Pessoa Física (chave de busca e unicidade).</td>
      </tr>
      <tr>
        <td><code>email</code></td>
        <td>string (100)</td>
        <td>Obrigatório, Email</td>
        <td>Endereço eletrônico de contato.</td>
      </tr>
      <tr>
        <td><code>telefone</code></td>
        <td>string (20)</td>
        <td>Opcional</td>
        <td>Número de contato com DDD.</td>
      </tr>

      <tr>
        <td rowspan="7"><strong>Veiculo</strong></td>
        <td><code>id</code></td>
        <td>int</td>
        <td>PK, Auto-inc</td>
        <td>Identificador único do veículo.</td>
      </tr>
      <tr>
        <td><code>modelo</code></td>
        <td>string (100)</td>
        <td>Obrigatório</td>
        <td>Modelo comercial do automóvel.</td>
      </tr>
      <tr>
        <td><code>anoFabricacao</code></td>
        <td>int</td>
        <td>Obrigatório</td>
        <td>Ano de fabricação do veículo.</td>
      </tr>
      <tr>
        <td><code>quilometragem</code></td>
        <td>int</td>
        <td>Obrigatório</td>
        <td>Quilometragem atual registrada no odômetro.</td>
      </tr>
      <tr>
        <td><code>placa</code></td>
        <td>string (8)</td>
        <td>Obrigatório, Único</td>
        <td>Placa do veículo (padrão Mercosul ou cinza).</td>
      </tr>
      <tr>
        <td><code>fabricanteId</code></td>
        <td>int</td>
        <td>FK Obrigatória</td>
        <td>Chave estrangeira referenciando Fabricante.</td>
      </tr>
      <tr>
        <td><code>categoriaId</code></td>
        <td>int</td>
        <td>FK Obrigatória</td>
        <td>Chave estrangeira referenciando Categoria.</td>
      </tr>

      <tr>
        <td rowspan="7"><strong>Aluguel</strong></td>
        <td><code>id</code></td>
        <td>int</td>
        <td>PK, Auto-inc</td>
        <td>Identificador único do contrato de aluguel.</td>
      </tr>
      <tr>
        <td><code>dataInicio</code></td>
        <td>DateTime</td>
        <td>Obrigatório</td>
        <td>Data e hora de retirada do veículo.</td>
      </tr>
      <tr>
        <td><code>dataPrevistaDevolucao</code></td>
        <td>DateTime</td>
        <td>Obrigatório</td>
        <td>Data combinada para entrega.</td>
      </tr>
      <tr>
        <td><code>dataDevolucao</code></td>
        <td>DateTime?</td>
        <td>Opcional (Devolução)</td>
        <td>Data e hora da efetiva devolução do carro.</td>
      </tr>
      <tr>
        <td><code>valorDiaria</code></td>
        <td>decimal</td>
        <td>Obrigatório</td>
        <td>Valor cobrado por dia de locação.</td>
      </tr>
      <tr>
        <td><code>valorTotal</code></td>
        <td>decimal?</td>
        <td>Calculado</td>
        <td>Valor financeiro total apurado na devolução.</td>
      </tr>
      <tr>
        <td><code>clienteId / veiculoId</code></td>
        <td>int</td>
        <td>FKs Obrigatórias</td>
        <td>Chaves que associam o cliente locatário e o veículo locado.</td>
      </tr>
    </tbody>
  </table>

  <h2>3. Especificação Detalhada dos Endpoints</h2>

  <!-- FABRICANTES -->
  <h3>3.1. Módulo Fabricantes (<code>/api/Fabricantes</code>)</h3>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Fabricantes</code></div>
    <p><strong>Descrição:</strong> Retorna todos os fabricantes cadastrados.</p>
    <p><strong>Respostas:</strong> <code>200 OK</code> (Array de Fabricantes) | <code>500 Internal Server Error</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Fabricantes/{id}</code></div>
    <p><strong>Descrição:</strong> Busca fabricante pelo ID informado na rota.</p>
    <p><strong>Respostas:</strong> <code>200 OK</code> (Objeto Fabricante) | <code>404 Not Found</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-post">POST</span> <code>/api/Fabricantes</code></div>
    <p><strong>Descrição:</strong> Cadastra um novo fabricante de veículos.</p>
    <p><strong>Exemplo de Payload (Request Body):</strong></p>
    <pre><code>{
  "nome": "Toyota",
  "paisOrigem": "Japão"
}</code></pre>
    <p><strong>Respostas:</strong> <code>201 Created</code> | <code>400 Bad Request</code> | <code>500 Internal Server Error</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-put">PUT</span> <code>/api/Fabricantes/{id}</code></div>
    <p><strong>Descrição:</strong> Atualiza os dados de um fabricante existente.</p>
    <p><strong>Respostas:</strong> <code>204 No Content</code> | <code>400 Bad Request</code> | <code>404 Not Found</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-delete">DELETE</span> <code>/api/Fabricantes/{id}</code></div>
    <p><strong>Descrição:</strong> Exclui um fabricante caso não possua veículos vinculados (integridade referencial).</p>
    <p><strong>Respostas:</strong> <code>204 No Content</code> | <code>400 Bad Request</code> (possui veículos) | <code>404 Not Found</code></p>
  </div>

  <!-- CATEGORIAS -->
  <h3>3.2. Módulo Categorias (<code>/api/Categorias</code>)</h3>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Categorias</code> | <span class="badge badge-get">GET</span> <code>/api/Categorias/{id}</code></div>
    <p><strong>Descrição:</strong> Listagem geral e busca por ID de categorias e diárias base.</p>
    <p><strong>Respostas:</strong> <code>200 OK</code> | <code>404 Not Found</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-post">POST</span> <code>/api/Categorias</code></div>
    <p><strong>Exemplo de Payload:</strong></p>
    <pre><code>{
  "nome": "SUV Compacto",
  "descricao": "Veículos utilitários esportivos com excelente conforto e espaço",
  "valorDiariaBase": 180.00
}</code></pre>
    <p><strong>Respostas:</strong> <code>201 Created</code> | <code>400 Bad Request</code></p>
  </div>

  <!-- CLIENTES -->
  <h3>3.3. Módulo Clientes (<code>/api/Clientes</code>)</h3>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-post">POST</span> <code>/api/Clientes</code></div>
    <p><strong>Descrição:</strong> Cadastra um novo cliente com validação de unicidade de CPF.</p>
    <p><strong>Exemplo de Payload:</strong></p>
    <pre><code>{
  "nome": "Lucas Ribeiro Silva",
  "cpf": "123.456.789-00",
  "email": "lucas.silva@email.com",
  "telefone": "(31) 98765-4321"
}</code></pre>
    <p><strong>Respostas:</strong> <code>201 Created</code> | <code>400 Bad Request</code> (CPF duplicado ou inválido)</p>
  </div>

  <!-- VEÍCULOS -->
  <h3>3.4. Módulo Veículos (<code>/api/Veiculos</code>)</h3>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-post">POST</span> <code>/api/Veiculos</code></div>
    <p><strong>Descrição:</strong> Cadastra veículo na frota validando unicidade de placa e chaves estrangeiras.</p>
    <p><strong>Exemplo de Payload:</strong></p>
    <pre><code>{
  "modelo": "Corolla Cross XRE 2.0",
  "anoFabricacao": 2024,
  "quilometragem": 15000,
  "placa": "BRA2E19",
  "cor": "Prata",
  "fabricanteId": 1,
  "categoriaId": 1
}</code></pre>
    <p><strong>Respostas:</strong> <code>201 Created</code> | <code>400 Bad Request</code></p>
  </div>

  <!-- ALUGUÉIS -->
  <h3>3.5. Módulo Aluguéis e Devolução (<code>/api/Alugueis</code>)</h3>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-post">POST</span> <code>/api/Alugueis</code></div>
    <p><strong>Descrição:</strong> Cria contrato de locação verificando cliente, veículo e quilometragem inicial coerente.</p>
    <p><strong>Exemplo de Payload:</strong></p>
    <pre><code>{
  "dataInicio": "2026-10-01T08:00:00Z",
  "dataPrevistaDevolucao": "2026-10-06T08:00:00Z",
  "valorDiaria": 180.00,
  "quilometragemInicial": 15000,
  "clienteId": 1,
  "veiculoId": 1
}</code></pre>
    <p><strong>Respostas:</strong> <code>201 Created</code> | <code>400 Bad Request</code></p>
  </div>

  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-put">PUT</span> <code>/api/Alugueis/{id}/devolucao</code></div>
    <p><strong>Descrição:</strong> Processa a devolução do veículo, calcula dias utilizados, apura valor total e atualiza odômetro no estoque.</p>
    <p><strong>Parâmetros de Query:</strong> <code>dataDevolucao</code> (DateTime) e <code>quilometragemFinal</code> (int).</p>
    <p><strong>Respostas:</strong> <code>200 OK</code> (Aluguel com Total Calculado) | <code>400 Bad Request</code> (KM final menor que inicial) | <code>404 Not Found</code></p>
  </div>

  <!-- CONSULTAS AVANÇADAS -->
  <h3>3.6. Módulo de Consultas Avançadas LINQ (<code>/api/Consultas</code>)</h3>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Consultas/veiculos-detalhados</code></div>
    <p><strong>Tipo:</strong> <code>INNER JOIN</code> (Veículos + Fabricantes + Categorias). Filtros: <code>fabricante</code>, <code>modelo</code>.</p>
  </div>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Consultas/alugueis-por-cliente</code></div>
    <p><strong>Tipo:</strong> <code>INNER JOIN</code> (Aluguéis + Clientes + Veículos). Filtro: <code>cpf</code>.</p>
  </div>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Consultas/total-gasto-por-cliente</code></div>
    <p><strong>Tipo:</strong> <code>INNER JOIN</code> com Agrupamento e <code>Sum(ValorTotal)</code>. Filtro: <code>valorMinimo</code>.</p>
  </div>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Consultas/clientes-com-ou-sem-aluguel</code></div>
    <p><strong>Tipo:</strong> <code>LEFT OUTER JOIN</code> (Identificação de clientes com e sem locação). Filtro: <code>apenasSemAluguel</code>.</p>
  </div>
  <div class="endpoint-card">
    <div class="endpoint-header"><span class="badge badge-get">GET</span> <code>/api/Consultas/veiculos-disponibilidade</code></div>
    <p><strong>Tipo:</strong> <code>LEFT OUTER JOIN</code> (Disponibilidade em tempo real da frota). Filtro: <code>apenasDisponiveis</code>.</p>
  </div>

</body>
</html>
`;

// 2. Template HTML para RELATÓRIO DE TESTES
function buildRelatorioTestesHtml() {
  const img01 = getBase64Image(path.join(evidenceDir, '01_swagger_visao_geral.png'));
  const img02 = getBase64Image(path.join(evidenceDir, '02_post_fabricante_toyota.png'));
  const img03 = getBase64Image(path.join(evidenceDir, '03_post_fabricante_volkswagen.png'));
  const img04 = getBase64Image(path.join(evidenceDir, '04_get_fabricantes_todos.png'));
  const img05 = getBase64Image(path.join(evidenceDir, '05_get_fabricante_por_id.png'));
  const img06 = getBase64Image(path.join(evidenceDir, '06_post_categoria_suv.png'));
  const img07 = getBase64Image(path.join(evidenceDir, '07_post_categoria_sedan.png'));
  const img08 = getBase64Image(path.join(evidenceDir, '08_get_categorias_todas.png'));
  const img09 = getBase64Image(path.join(evidenceDir, '09_get_categoria_por_id.png'));
  const img10 = getBase64Image(path.join(evidenceDir, '10_post_cliente_lucas.png'));
  const img11 = getBase64Image(path.join(evidenceDir, '11_post_cliente_mariana.png'));
  const img12 = getBase64Image(path.join(evidenceDir, '12_post_cliente_cpf_duplicado_erro_400.png'));
  const img13 = getBase64Image(path.join(evidenceDir, '13_get_clientes_todos.png'));
  const img14 = getBase64Image(path.join(evidenceDir, '14_get_cliente_por_id.png'));
  const img15 = getBase64Image(path.join(evidenceDir, '15_post_veiculo_corolla.png'));
  const img16 = getBase64Image(path.join(evidenceDir, '16_post_veiculo_jetta.png'));
  const img17 = getBase64Image(path.join(evidenceDir, '17_get_veiculos_todos.png'));
  const img18 = getBase64Image(path.join(evidenceDir, '18_get_veiculo_por_id.png'));
  const img19 = getBase64Image(path.join(evidenceDir, '19_post_aluguel_criar.png'));
  const img20 = getBase64Image(path.join(evidenceDir, '20_get_alugueis_todos.png'));
  const img21 = getBase64Image(path.join(evidenceDir, '21_put_aluguel_devolucao_calculo.png'));
  const img22 = getBase64Image(path.join(evidenceDir, '22_consulta_inner_join_veiculos_detalhados.png'));
  const img23 = getBase64Image(path.join(evidenceDir, '23_consulta_inner_join_alugueis_por_cliente.png'));
  const img24 = getBase64Image(path.join(evidenceDir, '24_consulta_inner_join_agrupamento_total_gasto.png'));
  const img25 = getBase64Image(path.join(evidenceDir, '25_consulta_left_join_clientes_com_ou_sem_aluguel.png'));
  const img26 = getBase64Image(path.join(evidenceDir, '26_consulta_left_join_veiculos_disponibilidade.png'));
  const img27 = getBase64Image(path.join(evidenceDir, '27_put_fabricante_atualizar.png'));
  const img28 = getBase64Image(path.join(evidenceDir, '28_put_cliente_atualizar.png'));

  return `
<!DOCTYPE html>
<html lang="pt-BR">
<head>
  <meta charset="UTF-8">
  <title>Relatório de Testes e Evidências - Locadora de Veículos</title>
  <style>
    @page {
      size: A4;
      margin: 18mm 14mm 18mm 14mm;
      @bottom-right {
        content: counter(page) " / " counter(pages);
      }
    }
    body {
      font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, Helvetica, Arial, sans-serif;
      color: #24292f;
      line-height: 1.45;
      font-size: 12.5px;
    }
    h1, h2, h3, h4 {
      color: #0969da;
      font-weight: 600;
      margin-top: 20px;
      margin-bottom: 8px;
    }
    h1 {
      font-size: 22px;
      border-bottom: 2px solid #0969da;
      padding-bottom: 6px;
      color: #1f2328;
    }
    h2 {
      font-size: 16px;
      border-bottom: 1px solid #d0d7de;
      padding-bottom: 5px;
      margin-top: 22px;
      page-break-after: avoid;
    }
    h3 {
      font-size: 14px;
      margin-top: 14px;
      color: #0550ae;
      page-break-after: avoid;
    }
    .header-box {
      background: linear-gradient(135deg, #1f2328 0%, #0969da 100%);
      color: white;
      padding: 18px 20px;
      border-radius: 8px;
      margin-bottom: 20px;
    }
    .header-box h1 {
      color: white;
      border: none;
      margin: 0 0 8px 0;
      padding: 0;
    }
    .header-box p {
      margin: 3px 0;
      font-size: 12.5px;
      opacity: 0.95;
    }
    table {
      width: 100%;
      border-collapse: collapse;
      margin: 12px 0;
      font-size: 11px;
    }
    th, td {
      border: 1px solid #d0d7de;
      padding: 6px 8px;
      text-align: left;
    }
    th {
      background-color: #f6f8fa;
      font-weight: 600;
      color: #1f2328;
    }
    tr:nth-child(even) {
      background-color: #fcfcfc;
    }
    .badge {
      display: inline-block;
      padding: 2px 6px;
      font-size: 10px;
      font-weight: 700;
      border-radius: 3px;
      color: white;
      text-transform: uppercase;
    }
    .badge-get { background-color: #2da44e; }
    .badge-post { background-color: #0969da; }
    .badge-put { background-color: #bf8700; }
    .badge-delete { background-color: #cf222e; }
    .badge-success { background-color: #2da44e; }
    .badge-fail { background-color: #cf222e; }
    .test-card {
      border: 1px solid #d0d7de;
      border-radius: 6px;
      padding: 10px 12px;
      margin: 14px 0;
      background-color: #ffffff;
      page-break-inside: avoid;
    }
    .test-card h4 {
      margin: 0 0 6px 0;
      color: #1f2328;
      font-size: 13px;
    }
    .evidence-img {
      max-width: 100%;
      border: 1px solid #d0d7de;
      border-radius: 4px;
      margin-top: 8px;
      display: block;
    }
    code {
      font-family: ui-monospace, Consolas, monospace;
      background-color: #eff1f3;
      padding: 1px 4px;
      border-radius: 3px;
      color: #0969da;
      font-size: 11px;
    }
  </style>
</head>
<body>

  <div class="header-box">
    <h1>Relatório de Testes e Evidências das APIs</h1>
    <p><strong>Projeto:</strong> Sistema de Locadora de Veículos (LocadoraVeiculos)</p>
    <p><strong>Instituição:</strong> PUC Minas - Análise e Desenvolvimento de Sistemas (TADS)</p>
    <p><strong>Etapa Avaliada:</strong> 3.3 - Realização de Testes Manuais com Evidências de Retorno e Prints de Tela</p>
    <p><strong>Data de Execução:</strong> 03/10/2026 | <strong>Status Geral:</strong> <span class="badge badge-success">100% Aprovado (28/28 Casos de Teste)</span></p>
  </div>

  <h2>1. Matriz de Cobertura e Execução de Testes</h2>
  <table>
    <thead>
      <tr>
        <th>ID</th>
        <th>Controlador</th>
        <th>Método</th>
        <th>Endpoint</th>
        <th>Cenário Testado</th>
        <th>Status Esperado</th>
        <th>Status Obtido</th>
        <th>Resultado</th>
      </tr>
    </thead>
    <tbody>
      <tr><td>TC-01</td><td>Swagger UI</td><td><span class="badge badge-get">GET</span></td><td><code>/</code></td><td>Acesso e renderização da UI</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-02</td><td>Fabricantes</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Fabricantes</code></td><td>Cadastrar Toyota</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-03</td><td>Fabricantes</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Fabricantes</code></td><td>Cadastrar Volkswagen</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-04</td><td>Fabricantes</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Fabricantes</code></td><td>Listar todos os fabricantes</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-05</td><td>Fabricantes</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Fabricantes/1</code></td><td>Buscar por ID</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-06</td><td>Categorias</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Categorias</code></td><td>Cadastrar SUV Compacto</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-07</td><td>Categorias</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Categorias</code></td><td>Cadastrar Sedan Executivo</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-08</td><td>Categorias</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Categorias</code></td><td>Listar todas as categorias</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-09</td><td>Categorias</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Categorias/1</code></td><td>Buscar categoria por ID</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-10</td><td>Clientes</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Clientes</code></td><td>Cadastrar Cliente Lucas</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-11</td><td>Clientes</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Clientes</code></td><td>Cadastrar Cliente Mariana</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-12</td><td>Clientes</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Clientes</code></td><td>Validação: CPF Duplicado</td><td>400 Bad Request</td><td>400 Bad Request</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-13</td><td>Clientes</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Clientes</code></td><td>Listar todos os clientes</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-14</td><td>Clientes</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Clientes/1</code></td><td>Buscar cliente por ID</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-15</td><td>Veículos</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Veiculos</code></td><td>Cadastrar Corolla Cross</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-16</td><td>Veículos</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Veiculos</code></td><td>Cadastrar Jetta GLI</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-17</td><td>Veículos</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Veiculos</code></td><td>Listar todos os veículos</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-18</td><td>Veículos</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Veiculos/1</code></td><td>Buscar veículo por ID</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-19</td><td>Aluguéis</td><td><span class="badge badge-post">POST</span></td><td><code>/api/Alugueis</code></td><td>Registrar Locação</td><td>201 Created</td><td>201 Created</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-20</td><td>Aluguéis</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Alugueis</code></td><td>Listar todos os aluguéis</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-21</td><td>Aluguéis</td><td><span class="badge badge-put">PUT</span></td><td><code>/api/Alugueis/1/devolucao</code></td><td>Devolução e cálculo total</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-22</td><td>Consultas</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Consultas/veiculos-detalhados</code></td><td>INNER JOIN: Veículos + Fab + Cat</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-23</td><td>Consultas</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Consultas/alugueis-por-cliente</code></td><td>INNER JOIN: Aluguéis por CPF</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-24</td><td>Consultas</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Consultas/total-gasto-por-cliente</code></td><td>INNER JOIN + Agrupamento Total</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-25</td><td>Consultas</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Consultas/clientes-com-ou-sem-aluguel</code></td><td>LEFT JOIN: Clientes sem aluguel</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-26</td><td>Consultas</td><td><span class="badge badge-get">GET</span></td><td><code>/api/Consultas/veiculos-disponibilidade</code></td><td>LEFT JOIN: Disponibilidade Frota</td><td>200 OK</td><td>200 OK</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-27</td><td>Fabricantes</td><td><span class="badge badge-put">PUT</span></td><td><code>/api/Fabricantes/1</code></td><td>Atualizar Fabricante</td><td>204 No Content</td><td>204 No Content</td><td><span class="badge badge-success">Aprovado</span></td></tr>
      <tr><td>TC-28</td><td>Clientes</td><td><span class="badge badge-put">PUT</span></td><td><code>/api/Clientes/1</code></td><td>Atualizar Cliente</td><td>204 No Content</td><td>204 No Content</td><td><span class="badge badge-success">Aprovado</span></td></tr>
    </tbody>
  </table>

  <h2>2. Evidências dos Testes no Swagger UI (Prints de Tela)</h2>

  <!-- Visão Geral -->
  <div class="test-card">
    <h4>TC-01: Visão Geral do Swagger UI e Documentação OpenAPI</h4>
    <p>Acesso à interface raiz documentando todos os 6 controladores e schemas.</p>
    <img class="evidence-img" src="${img01}" alt="Swagger UI" />
  </div>

  <!-- Fabricantes -->
  <h3>2.1. Módulo Fabricantes</h3>
  <div class="test-card">
    <h4>TC-02: Cadastro de Fabricante Toyota (POST)</h4>
    <p>Status Retornado: <code>201 Created</code> com objeto persistido no banco.</p>
    <img class="evidence-img" src="${img02}" alt="POST Fabricante" />
  </div>

  <div class="test-card">
    <h4>TC-04: Consulta de Todos os Fabricantes (GET)</h4>
    <p>Status Retornado: <code>200 OK</code> com array de fabricantes.</p>
    <img class="evidence-img" src="${img04}" alt="GET Fabricantes" />
  </div>

  <div class="test-card">
    <h4>TC-27: Atualização de Fabricante (PUT)</h4>
    <p>Status Retornado: <code>204 No Content</code>.</p>
    <img class="evidence-img" src="${img27}" alt="PUT Fabricante" />
  </div>

  <!-- Categorias -->
  <h3>2.2. Módulo Categorias</h3>
  <div class="test-card">
    <h4>TC-06: Cadastro de Categoria SUV (POST)</h4>
    <p>Status Retornado: <code>201 Created</code> com diária base de R$ 180,00.</p>
    <img class="evidence-img" src="${img06}" alt="POST Categoria" />
  </div>

  <div class="test-card">
    <h4>TC-08: Consulta de Todas as Categorias (GET)</h4>
    <p>Status Retornado: <code>200 OK</code>.</p>
    <img class="evidence-img" src="${img08}" alt="GET Categorias" />
  </div>

  <!-- Clientes -->
  <h3>2.3. Módulo Clientes</h3>
  <div class="test-card">
    <h4>TC-10: Cadastro de Cliente Lucas Ribeiro (POST)</h4>
    <p>Status Retornado: <code>201 Created</code>.</p>
    <img class="evidence-img" src="${img10}" alt="POST Cliente" />
  </div>

  <div class="test-card">
    <h4>TC-12: Teste de Validação Negativo - CPF Duplicado (POST)</h4>
    <p>Status Retornado: <code>400 Bad Request</code> com mensagem descritiva de erro.</p>
    <img class="evidence-img" src="${img12}" alt="POST Cliente CPF Duplicado" />
  </div>

  <div class="test-card">
    <h4>TC-13: Consulta de Todos os Clientes (GET)</h4>
    <p>Status Retornado: <code>200 OK</code>.</p>
    <img class="evidence-img" src="${img13}" alt="GET Clientes" />
  </div>

  <!-- Veículos -->
  <h3>2.4. Módulo Veículos</h3>
  <div class="test-card">
    <h4>TC-15: Cadastro de Veículo Corolla Cross (POST)</h4>
    <p>Status Retornado: <code>201 Created</code> com validação de placa e chaves estrangeiras.</p>
    <img class="evidence-img" src="${img15}" alt="POST Veiculo" />
  </div>

  <div class="test-card">
    <h4>TC-17: Listagem de Veículos com Relacionamentos (GET)</h4>
    <p>Status Retornado: <code>200 OK</code> com dados aninhados de Fabricante e Categoria.</p>
    <img class="evidence-img" src="${img17}" alt="GET Veiculos" />
  </div>

  <!-- Aluguéis e Devolução -->
  <h3>2.5. Módulo Aluguéis e Operação de Devolução</h3>
  <div class="test-card">
    <h4>TC-19: Registro de Contrato de Locação (POST)</h4>
    <p>Status Retornado: <code>201 Created</code>.</p>
    <img class="evidence-img" src="${img19}" alt="POST Aluguel" />
  </div>

  <div class="test-card">
    <h4>TC-21: Devolução de Veículo, Cálculo Automático e Atualização de KM (PUT)</h4>
    <p>Status Retornado: <code>200 OK</code> com valor total de R$ 900,00 (5 diárias) e KM atualizada no estoque (15.450 km).</p>
    <img class="evidence-img" src="${img21}" alt="PUT Devolucao" />
  </div>

  <!-- Consultas Avançadas LINQ -->
  <h3>2.6. Módulo Consultas Avançadas (LINQ JOINs e Agregações)</h3>
  <div class="test-card">
    <h4>TC-22: Filtro 1 (INNER JOIN) - Veículos Detalhados</h4>
    <p>Status Retornado: <code>200 OK</code> com junção de Veículo, Fabricante e Categoria.</p>
    <img class="evidence-img" src="${img22}" alt="INNER JOIN Veiculos" />
  </div>

  <div class="test-card">
    <h4>TC-23: Filtro 2 (INNER JOIN) - Aluguéis por Cliente</h4>
    <p>Status Retornado: <code>200 OK</code> com filtro por CPF.</p>
    <img class="evidence-img" src="${img23}" alt="INNER JOIN Alugueis" />
  </div>

  <div class="test-card">
    <h4>TC-24: Filtro 3 (INNER JOIN + Agrupamento) - Total Financeiro Gasto por Cliente</h4>
    <p>Status Retornado: <code>200 OK</code> com soma acumulada e contagem de locações.</p>
    <img class="evidence-img" src="${img24}" alt="Agrupamento Total Gasto" />
  </div>

  <div class="test-card">
    <h4>TC-25: Filtro 4 (LEFT JOIN) - Clientes com ou sem Aluguel</h4>
    <p>Status Retornado: <code>200 OK</code> identificando clientes inativos.</p>
    <img class="evidence-img" src="${img25}" alt="LEFT JOIN Clientes" />
  </div>

  <div class="test-card">
    <h4>TC-26: Filtro 5 (LEFT JOIN) - Disponibilidade da Frota</h4>
    <p>Status Retornado: <code>200 OK</code> com verificação em tempo real de carros livres.</p>
    <img class="evidence-img" src="${img26}" alt="LEFT JOIN Disponibilidade" />
  </div>

</body>
</html>
`;
}

(async () => {
  console.log('Iniciando geração dos documentos em PDF via Puppeteer...');
  const browser = await puppeteer.launch({
    headless: 'new',
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  // 1. Gerar Documentação das APIs em PDF
  console.log('Gerando Documentacao_APIs_LocadoraVeiculos.pdf...');
  const page1 = await browser.newPage();
  await page1.setContent(htmlDocAPIs, { waitUntil: 'networkidle0' });
  const pdfPathDoc = path.join(__dirname, 'Documentacao_APIs_LocadoraVeiculos.pdf');
  await page1.pdf({
    path: pdfPathDoc,
    format: 'A4',
    printBackground: true,
    margin: { top: '15mm', right: '12mm', bottom: '15mm', left: '12mm' }
  });
  console.log(`PDF gerado: ${pdfPathDoc}`);

  // 2. Gerar Relatório de Testes em PDF
  console.log('Gerando Relatorio_Testes_LocadoraVeiculos.pdf...');
  const page2 = await browser.newPage();
  const htmlRelatorio = buildRelatorioTestesHtml();
  await page2.setContent(htmlRelatorio, { waitUntil: 'networkidle0' });
  const pdfPathRelatorio = path.join(__dirname, 'Relatorio_Testes_LocadoraVeiculos.pdf');
  await page2.pdf({
    path: pdfPathRelatorio,
    format: 'A4',
    printBackground: true,
    margin: { top: '15mm', right: '12mm', bottom: '15mm', left: '12mm' }
  });
  console.log(`PDF gerado: ${pdfPathRelatorio}`);

  await browser.close();
  console.log('Ambos os PDFs foram gerados com sucesso!');
})();
