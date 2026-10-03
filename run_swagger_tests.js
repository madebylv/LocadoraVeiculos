const puppeteer = require('puppeteer');
const fs = require('fs');
const path = require('path');

const EVIDENCE_DIR = path.join(__dirname, 'docs', 'evidencias');
if (!fs.existsSync(EVIDENCE_DIR)) {
  fs.mkdirSync(EVIDENCE_DIR, { recursive: true });
}

async function sleep(ms) {
  return new Promise(resolve => setTimeout(resolve, ms));
}

(async () => {
  console.log('Iniciando navegador com Puppeteer...');
  const browser = await puppeteer.launch({
    headless: 'new',
    defaultViewport: { width: 1366, height: 1100 },
    args: ['--no-sandbox', '--disable-setuid-sandbox']
  });

  const page = await browser.newPage();

  console.log('Acessando Swagger UI em http://localhost:5000/...');
  await page.goto('http://localhost:5000/', { waitUntil: 'networkidle2' });
  await sleep(2000);

  // 1. Screenshot Geral do Swagger UI
  await page.screenshot({ path: path.join(EVIDENCE_DIR, '01_swagger_visao_geral.png'), fullPage: false });
  console.log('Screenshot 01_swagger_visao_geral.png salvo.');

  // Expandir todas as seções (tags)
  const tagButtons = await page.$$('.opblock-tag');
  console.log(`Encontradas ${tagButtons.length} seções/tags.`);
  for (const tagBtn of tagButtons) {
    try {
      const isExpanded = await page.evaluate(el => el.parentElement.classList.contains('is-open') || el.classList.contains('is-open'), tagBtn);
      if (!isExpanded) {
        await tagBtn.click();
        await sleep(300);
      }
    } catch (e) {
      console.log('Erro ao expandir tag:', e.message);
    }
  }
  await sleep(1000);

  // Função para executar e capturar uma operação
  async function testOperation({ id, tag, pathStr, method, body, params, screenshotName }) {
    console.log(`\n--- Testando [${method}] ${pathStr} (${screenshotName}) ---`);
    
    // Tenta localizar por id ou pelo seletor de método e caminho
    let opBlock = await page.$(`#${id}`);
    if (!opBlock) {
      const selector = `.opblock-${method.toLowerCase()}[data-path="${pathStr}"]`;
      opBlock = await page.$(selector);
    }

    if (!opBlock) {
      // Procura em todos os opblocks
      const allOps = await page.$$('.opblock');
      for (const op of allOps) {
        const text = await page.evaluate(el => el.textContent, op);
        if (text.includes(pathStr) && text.includes(method.toUpperCase())) {
          opBlock = op;
          break;
        }
      }
    }

    if (!opBlock) {
      console.error(`ERRO: Operação [${method}] ${pathStr} não encontrada no DOM!`);
      return;
    }

    // Scroll até o bloco
    await page.evaluate(el => el.scrollIntoView({ behavior: 'auto', block: 'center' }), opBlock);
    await sleep(400);

    // Expandir operação se fechada
    const isOpen = await page.evaluate(el => el.classList.contains('is-open'), opBlock);
    if (!isOpen) {
      const summary = await opBlock.$('.opblock-summary');
      if (summary) {
        await summary.click();
        await sleep(500);
      }
    }

    // Clicar em "Try it out"
    const tryOutBtn = await opBlock.$('.try-out__btn');
    if (tryOutBtn) {
      const isCancel = await page.evaluate(el => el.textContent.includes('Cancel'), tryOutBtn);
      if (!isCancel) {
        await tryOutBtn.click();
        await sleep(400);
      }
    }

    // Preencher Parâmetros
    if (params) {
      for (const [key, val] of Object.entries(params)) {
        // Encontra input por placeholder ou atributo data-name
        const inputs = await opBlock.$$('input, select');
        for (const input of inputs) {
          const placeholder = await page.evaluate(el => el.placeholder || el.getAttribute('data-name') || el.name, input);
          if (placeholder && placeholder.toLowerCase().includes(key.toLowerCase())) {
            await page.evaluate(el => el.value = '', input);
            await input.type(String(val));
            await sleep(200);
          }
        }
      }
    }

    // Preencher Body
    if (body) {
      const textarea = await opBlock.$('textarea.body-param__text');
      if (textarea) {
        await page.evaluate((el, val) => {
          el.value = val;
          el.dispatchEvent(new Event('change', { bubbles: true }));
          el.dispatchEvent(new Event('input', { bubbles: true }));
        }, textarea, JSON.stringify(body, null, 2));
        await sleep(300);
      }
    }

    // Clicar em "Execute"
    const executeBtn = await opBlock.$('button.execute');
    if (executeBtn) {
      await executeBtn.click();
      await sleep(1800); // Aguardar chamada HTTP e resposta
    }

    // Scroll para focar o bloco de resposta
    await page.evaluate(el => el.scrollIntoView({ behavior: 'auto', block: 'center' }), opBlock);
    await sleep(400);

    // Tirar screenshot do opBlock
    if (screenshotName) {
      await opBlock.screenshot({ path: path.join(EVIDENCE_DIR, `${screenshotName}.png`) });
      console.log(`Evidência salva: ${screenshotName}.png`);
    }
  }

  // Bateria Completa de Testes

  // --- 1. FABRICANTES ---
  await testOperation({
    id: 'operations-Fabricantes-post_api_Fabricantes',
    method: 'post',
    pathStr: '/api/Fabricantes',
    body: { nome: 'Toyota', paisOrigem: 'Japão' },
    screenshotName: '02_post_fabricante_toyota'
  });

  await testOperation({
    id: 'operations-Fabricantes-post_api_Fabricantes',
    method: 'post',
    pathStr: '/api/Fabricantes',
    body: { nome: 'Volkswagen', paisOrigem: 'Alemanha' },
    screenshotName: '03_post_fabricante_volkswagen'
  });

  await testOperation({
    id: 'operations-Fabricantes-get_api_Fabricantes',
    method: 'get',
    pathStr: '/api/Fabricantes',
    screenshotName: '04_get_fabricantes_todos'
  });

  await testOperation({
    id: 'operations-Fabricantes-get_api_Fabricantes__id_',
    method: 'get',
    pathStr: '/api/Fabricantes/{id}',
    params: { id: 1 },
    screenshotName: '05_get_fabricante_por_id'
  });

  // --- 2. CATEGORIAS ---
  await testOperation({
    id: 'operations-Categorias-post_api_Categorias',
    method: 'post',
    pathStr: '/api/Categorias',
    body: { nome: 'SUV Compacto', descricao: 'Veículos utilitários esportivos com excelente conforto e espaço', valorDiariaBase: 180.0 },
    screenshotName: '06_post_categoria_suv'
  });

  await testOperation({
    id: 'operations-Categorias-post_api_Categorias',
    method: 'post',
    pathStr: '/api/Categorias',
    body: { nome: 'Sedan Executivo', descricao: 'Veículos confortáveis de alto padrão para viagens corporativas', valorDiariaBase: 250.0 },
    screenshotName: '07_post_categoria_sedan'
  });

  await testOperation({
    id: 'operations-Categorias-get_api_Categorias',
    method: 'get',
    pathStr: '/api/Categorias',
    screenshotName: '08_get_categorias_todas'
  });

  await testOperation({
    id: 'operations-Categorias-get_api_Categorias__id_',
    method: 'get',
    pathStr: '/api/Categorias/{id}',
    params: { id: 1 },
    screenshotName: '09_get_categoria_por_id'
  });

  // --- 3. CLIENTES ---
  await testOperation({
    id: 'operations-Clientes-post_api_Clientes',
    method: 'post',
    pathStr: '/api/Clientes',
    body: { nome: 'Lucas Ribeiro Silva', cpf: '123.456.789-00', email: 'lucas.silva@email.com', telefone: '(31) 98765-4321' },
    screenshotName: '10_post_cliente_lucas'
  });

  await testOperation({
    id: 'operations-Clientes-post_api_Clientes',
    method: 'post',
    pathStr: '/api/Clientes',
    body: { nome: 'Mariana Duarte Costa', cpf: '987.654.321-11', email: 'mariana.costa@email.com', telefone: '(31) 99887-7665' },
    screenshotName: '11_post_cliente_mariana'
  });

  // Teste de validação: CPF Duplicado (Status 400)
  await testOperation({
    id: 'operations-Clientes-post_api_Clientes',
    method: 'post',
    pathStr: '/api/Clientes',
    body: { nome: 'Lucas Clone', cpf: '123.456.789-00', email: 'lucas.clone@email.com', telefone: '(31) 91111-2222' },
    screenshotName: '12_post_cliente_cpf_duplicado_erro_400'
  });

  await testOperation({
    id: 'operations-Clientes-get_api_Clientes',
    method: 'get',
    pathStr: '/api/Clientes',
    screenshotName: '13_get_clientes_todos'
  });

  await testOperation({
    id: 'operations-Clientes-get_api_Clientes__id_',
    method: 'get',
    pathStr: '/api/Clientes/{id}',
    params: { id: 1 },
    screenshotName: '14_get_cliente_por_id'
  });

  // --- 4. VEICULOS ---
  await testOperation({
    id: 'operations-Veiculos-post_api_Veiculos',
    method: 'post',
    pathStr: '/api/Veiculos',
    body: { modelo: 'Corolla Cross XRE 2.0', anoFabricacao: 2024, quilometragem: 15000, placa: 'BRA2E19', cor: 'Prata', fabricanteId: 1, categoriaId: 1 },
    screenshotName: '15_post_veiculo_corolla'
  });

  await testOperation({
    id: 'operations-Veiculos-post_api_Veiculos',
    method: 'post',
    pathStr: '/api/Veiculos',
    body: { modelo: 'Jetta GLI 350 TSI', anoFabricacao: 2023, quilometragem: 28000, placa: 'PUC8A24', cor: 'Cinza Puro', fabricanteId: 2, categoriaId: 2 },
    screenshotName: '16_post_veiculo_jetta'
  });

  await testOperation({
    id: 'operations-Veiculos-get_api_Veiculos',
    method: 'get',
    pathStr: '/api/Veiculos',
    screenshotName: '17_get_veiculos_todos'
  });

  await testOperation({
    id: 'operations-Veiculos-get_api_Veiculos__id_',
    method: 'get',
    pathStr: '/api/Veiculos/{id}',
    params: { id: 1 },
    screenshotName: '18_get_veiculo_por_id'
  });

  // --- 5. ALUGUEIS ---
  await testOperation({
    id: 'operations-Alugueis-post_api_Alugueis',
    method: 'post',
    pathStr: '/api/Alugueis',
    body: {
      dataInicio: '2026-10-01T08:00:00.000Z',
      dataPrevistaDevolucao: '2026-10-06T08:00:00.000Z',
      dataDevolucao: null,
      valorDiaria: 180.0,
      valorTotal: null,
      quilometragemInicial: 15000,
      quilometragemFinal: null,
      clienteId: 1,
      veiculoId: 1
    },
    screenshotName: '19_post_aluguel_criar'
  });

  await testOperation({
    id: 'operations-Alugueis-get_api_Alugueis',
    method: 'get',
    pathStr: '/api/Alugueis',
    screenshotName: '20_get_alugueis_todos'
  });

  // Devolução com cálculo automático e atualização de KM
  await testOperation({
    id: 'operations-Alugueis-put_api_Alugueis__id__devolucao',
    method: 'put',
    pathStr: '/api/Alugueis/{id}/devolucao',
    params: { id: 1, dataDevolucao: '2026-10-06T10:00:00', quilometragemFinal: 15450 },
    screenshotName: '21_put_aluguel_devolucao_calculo'
  });

  // --- 6. CONSULTAS AVANÇADAS (LINQ JOINS) ---
  await testOperation({
    id: 'operations-Consultas-get_api_Consultas_veiculos_detalhados',
    method: 'get',
    pathStr: '/api/Consultas/veiculos-detalhados',
    params: { fabricante: 'Toyota' },
    screenshotName: '22_consulta_inner_join_veiculos_detalhados'
  });

  await testOperation({
    id: 'operations-Consultas-get_api_Consultas_alugueis_por_cliente',
    method: 'get',
    pathStr: '/api/Consultas/alugueis-por-cliente',
    params: { cpf: '123' },
    screenshotName: '23_consulta_inner_join_alugueis_por_cliente'
  });

  await testOperation({
    id: 'operations-Consultas-get_api_Consultas_total_gasto_por_cliente',
    method: 'get',
    pathStr: '/api/Consultas/total-gasto-por-cliente',
    params: { valorMinimo: 0 },
    screenshotName: '24_consulta_inner_join_agrupamento_total_gasto'
  });

  await testOperation({
    id: 'operations-Consultas-get_api_Consultas_clientes_com_ou_sem_aluguel',
    method: 'get',
    pathStr: '/api/Consultas/clientes-com-ou-sem-aluguel',
    params: { apenasSemAluguel: false },
    screenshotName: '25_consulta_left_join_clientes_com_ou_sem_aluguel'
  });

  await testOperation({
    id: 'operations-Consultas-get_api_Consultas_veiculos_disponibilidade',
    method: 'get',
    pathStr: '/api/Consultas/veiculos-disponibilidade',
    params: { apenasDisponiveis: true },
    screenshotName: '26_consulta_left_join_veiculos_disponibilidade'
  });

  // --- 7. ATUALIZAÇÕES E EXCLUSÕES ---
  await testOperation({
    id: 'operations-Fabricantes-put_api_Fabricantes__id_',
    method: 'put',
    pathStr: '/api/Fabricantes/{id}',
    params: { id: 1 },
    body: { id: 1, nome: 'Toyota Motors do Brasil', paisOrigem: 'Japão / Brasil' },
    screenshotName: '27_put_fabricante_atualizar'
  });

  await testOperation({
    id: 'operations-Clientes-put_api_Clientes__id_',
    method: 'put',
    pathStr: '/api/Clientes/{id}',
    params: { id: 1 },
    body: { id: 1, nome: 'Lucas Ribeiro Silva Santos', cpf: '123.456.789-00', email: 'lucas.santos@email.com', telefone: '(31) 98888-7777' },
    screenshotName: '28_put_cliente_atualizar'
  });

  console.log('\n=============================================');
  console.log('Todos os testes e prints executados com êxito!');
  console.log('=============================================');
  await browser.close();
})();
