using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocadoraVeiculos.Data;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
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
        /// Filtro 1 (INNER JOIN): Retorna veículos juntando dados do Fabricante e da Categoria
        /// </summary>
        [HttpGet("veiculos-detalhados")]
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
        /// Filtro 2 (INNER JOIN): Retorna aluguéis juntando Cliente e Veículo, filtrando por CPF ou período
        /// </summary>
        [HttpGet("alugueis-por-cliente")]
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
        /// Filtro 3 (INNER JOIN + Agrupamento): Total financeiro gasto em aluguéis por cliente acima de um valor mínimo
        /// </summary>
        [HttpGet("total-gasto-por-cliente")]
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
        /// Filtro 4 (LEFT OUTER JOIN): Lista todos os clientes e seus aluguéis (inclui clientes sem nenhum aluguel)
        /// </summary>
        [HttpGet("clientes-com-ou-sem-aluguel")]
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
        /// Filtro 5 (LEFT OUTER JOIN): Lista veículos e verifica se estão atualmente disponíveis (sem aluguel em aberto)
        /// </summary>
        [HttpGet("veiculos-disponibilidade")]
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
