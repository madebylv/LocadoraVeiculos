using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocadoraVeiculos.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Controlador responsável por consultas avançadas com junções (INNER JOIN, LEFT JOIN) e agregações LINQ.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ConsultasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ConsultasController(ApplicationContext context)
        {
            _context = context;
        }

        // =========================================================================================
        // FILTROS COM INNER JOIN
        // =========================================================================================

        /// <summary>
        /// Filtro 1 (INNER JOIN): Retorna a listagem de veículos combinando dados de Veículo, Fabricante e Categoria.
        /// </summary>
        /// <param name="fabricante">Filtro opcional por nome ou parte do nome do Fabricante.</param>
        /// <param name="modelo">Filtro opcional por modelo ou parte do modelo do Veículo.</param>
        /// <response code="200">Lista detalhada de veículos retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao processar a consulta.</response>
        [HttpGet("veiculos-detalhados")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FiltrarVeiculosDetalhados([FromQuery] string fabricante = null, [FromQuery] string modelo = null)
        {
            try
            {
                var query = from v in _context.Veiculos
                            join f in _context.Fabricantes on v.FabricanteId equals f.Id
                            join c in _context.Categorias on v.CategoriaId equals c.Id
                            select new
                            {
                                VeiculoId = v.Id,
                                Modelo = v.Modelo,
                                Ano = v.AnoFabricacao,
                                Placa = v.Placa,
                                Fabricante = f.Nome,
                                PaisOrigem = f.PaisOrigem,
                                Categoria = c.Nome,
                                DiariaBase = c.ValorDiariaBase
                            };

                if (!string.IsNullOrWhiteSpace(fabricante))
                    query = query.Where(x => x.Fabricante.Contains(fabricante));

                if (!string.IsNullOrWhiteSpace(modelo))
                    query = query.Where(x => x.Modelo.Contains(modelo));

                var resultado = await query.ToListAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao consultar veículos: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtro 2 (INNER JOIN): Retorna o histórico de aluguéis combinando Cliente e Veículo, com filtro opcional por CPF.
        /// </summary>
        /// <param name="cpf">Filtro opcional por CPF do cliente.</param>
        /// <response code="200">Lista de aluguéis e dados combinados retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao consultar aluguéis.</response>
        [HttpGet("alugueis-por-cliente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FiltrarAlugueis([FromQuery] string cpf = null)
        {
            try
            {
                var query = from a in _context.Alugueis
                            join c in _context.Clientes on a.ClienteId equals c.Id
                            join v in _context.Veiculos on a.VeiculoId equals v.Id
                            select new
                            {
                                AluguelId = a.Id,
                                ClienteNome = c.Nome,
                                ClienteCPF = c.CPF,
                                VeiculoModelo = v.Modelo,
                                VeiculoPlaca = v.Placa,
                                DataInicio = a.DataInicio,
                                DataPrevistaDevolucao = a.DataPrevistaDevolucao,
                                DataDevolucao = a.DataDevolucao,
                                ValorDiaria = a.ValorDiaria,
                                ValorTotal = a.ValorTotal
                            };

                if (!string.IsNullOrWhiteSpace(cpf))
                    query = query.Where(x => x.ClienteCPF.Contains(cpf));

                var resultado = await query.ToListAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao consultar aluguéis: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtro 3 (INNER JOIN + Agrupamento): Total financeiro gasto em aluguéis por cliente, com filtro por valor mínimo acumulado.
        /// </summary>
        /// <param name="valorMinimo">Valor financeiro mínimo total para filtragem (padrão: 0).</param>
        /// <response code="200">Relatório de total gasto agrupado por cliente retornado com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao calcular os totais.</response>
        [HttpGet("total-gasto-por-cliente")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FiltrarTotalGastoPorCliente([FromQuery] decimal valorMinimo = 0)
        {
            try
            {
                var resultado = await (from a in _context.Alugueis
                                       join c in _context.Clientes on a.ClienteId equals c.Id
                                       group a by new { c.Id, c.Nome, c.CPF } into g
                                       let totalGasto = g.Sum(x => x.ValorTotal ?? 0)
                                       where totalGasto >= valorMinimo
                                       select new
                                       {
                                           ClienteId = g.Key.Id,
                                           Nome = g.Key.Nome,
                                           CPF = g.Key.CPF,
                                           QuantidadeAlugueis = g.Count(),
                                           TotalGasto = totalGasto
                                       }).ToListAsync();

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao calcular gastos por cliente: {ex.Message}");
            }
        }

        // =========================================================================================
        // FILTROS COM LEFT OUTER JOIN (GroupJoin + DefaultIfEmpty)
        // =========================================================================================

        /// <summary>
        /// Filtro 4 (LEFT OUTER JOIN): Lista todos os clientes e seus respectivos aluguéis, permitindo identificar clientes inativos/sem aluguel.
        /// </summary>
        /// <param name="apenasSemAluguel">Se true, retorna apenas clientes que nunca realizaram aluguel.</param>
        /// <response code="200">Listagem de clientes com ou sem aluguel retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao buscar clientes.</response>
        [HttpGet("clientes-com-ou-sem-aluguel")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FiltrarClientesSemAluguel([FromQuery] bool apenasSemAluguel = false)
        {
            try
            {
                var query = from c in _context.Clientes
                            join a in _context.Alugueis on c.Id equals a.ClienteId into grupoAlugueis
                            from aluguel in grupoAlugueis.DefaultIfEmpty()
                            select new
                            {
                                ClienteId = c.Id,
                                Nome = c.Nome,
                                CPF = c.CPF,
                                Email = c.Email,
                                PossuiAluguel = aluguel != null,
                                AluguelId = (int?)aluguel.Id,
                                DataInicio = (DateTime?)aluguel.DataInicio
                            };

                if (apenasSemAluguel)
                    query = query.Where(x => !x.PossuiAluguel);

                var resultado = await query.ToListAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao buscar clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// Filtro 5 (LEFT OUTER JOIN): Lista veículos e verifica status de disponibilidade em tempo real (sem aluguel em aberto).
        /// </summary>
        /// <param name="apenasDisponiveis">Se true, filtra apenas veículos disponíveis para nova locação.</param>
        /// <response code="200">Listagem de disponibilidade da frota retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao verificar disponibilidade.</response>
        [HttpGet("veiculos-disponibilidade")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> FiltrarVeiculosDisponibilidade([FromQuery] bool apenasDisponiveis = true)
        {
            try
            {
                var query = from v in _context.Veiculos
                            join a in _context.Alugueis.Where(al => al.DataDevolucao == null) on v.Id equals a.VeiculoId into grupoAluguelAtivo
                            from aluguelAtivo in grupoAluguelAtivo.DefaultIfEmpty()
                            select new
                            {
                                VeiculoId = v.Id,
                                Modelo = v.Modelo,
                                Placa = v.Placa,
                                Ano = v.AnoFabricacao,
                                EstaAlugadoAgora = aluguelAtivo != null,
                                Status = aluguelAtivo == null ? "Disponível para Locação" : "Locado"
                            };

                if (apenasDisponiveis)
                    query = query.Where(x => !x.EstaAlugadoAgora);

                var resultado = await query.ToListAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao verificar disponibilidade de veículos: {ex.Message}");
            }
        }
    }
}
