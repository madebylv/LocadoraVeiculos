using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocadoraVeiculos.Controllers
{
    /// <summary>
    /// Controlador responsável pela gestão das operações de Aluguel e Devolução de Veículos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a listagem completa de todos os aluguéis registrados, incluindo dados de Cliente e Veículo.
        /// </summary>
        /// <response code="200">Lista de aluguéis retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao buscar aluguéis.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Aluguel>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Aluguel>>> ObterTodos()
        {
            try
            {
                var alugueis = await _context.Alugueis
                    .Include(a => a.Cliente)
                    .Include(a => a.Veiculo)
                    .ToListAsync();

                return Ok(alugueis);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar aluguéis: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém os dados detalhados de um aluguel específico pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do aluguel.</param>
        /// <response code="200">Aluguel encontrado com sucesso.</response>
        /// <response code="404">Aluguel não encontrado com o ID especificado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Aluguel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Aluguel>> ObterPorId(int id)
        {
            var aluguel = await _context.Alugueis
                .Include(a => a.Cliente)
                .Include(a => a.Veiculo)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (aluguel == null)
                return NotFound($"Aluguel com ID {id} não encontrado.");

            return Ok(aluguel);
        }

        /// <summary>
        /// Registra a locação (aluguel) de um veículo para um cliente.
        /// </summary>
        /// <param name="aluguel">Objeto contendo datas, valor da diária, quilometragem inicial, ID do cliente e ID do veículo.</param>
        /// <response code="201">Aluguel registrado com sucesso.</response>
        /// <response code="400">Dados inválidos, cliente/veículo não encontrados ou quilometragem incoerente.</response>
        /// <response code="500">Erro interno do servidor ao registrar o aluguel.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Aluguel), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Aluguel>> Criar([FromBody] Aluguel aluguel)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clienteExiste = await _context.Clientes.AnyAsync(c => c.Id == aluguel.ClienteId);
            if (!clienteExiste)
                return BadRequest($"Cliente com ID {aluguel.ClienteId} não encontrado.");

            var veiculo = await _context.Veiculos.FindAsync(aluguel.VeiculoId);
            if (veiculo == null)
                return BadRequest($"Veículo com ID {aluguel.VeiculoId} não encontrado.");

            // Validação de quilometragem inicial coerente
            if (aluguel.QuilometragemInicial < veiculo.Quilometragem)
                return BadRequest($"A quilometragem inicial ({aluguel.QuilometragemInicial}) não pode ser menor que a quilometragem atual do veículo ({veiculo.Quilometragem}).");

            try
            {
                _context.Alugueis.Add(aluguel);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterPorId), new { id = aluguel.Id }, aluguel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar aluguel: {ex.Message}");
            }
        }

        /// <summary>
        /// Registra a devolução do veículo, calcula o valor total com base nos dias utilizados e atualiza a quilometragem do veículo.
        /// </summary>
        /// <param name="id">Identificador único (ID) do aluguel.</param>
        /// <param name="dataDevolucao">Data e hora em que a devolução ocorreu.</param>
        /// <param name="quilometragemFinal">Quilometragem registrada no odômetro no momento da devolução.</param>
        /// <response code="200">Devolução registrada com sucesso e total financeiro calculado.</response>
        /// <response code="400">Quilometragem final menor que a inicial ou dados inconsistentes.</response>
        /// <response code="404">Aluguel não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao processar a devolução.</response>
        [HttpPut("{id}/devolucao")]
        [ProducesResponseType(typeof(Aluguel), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> RegistrarDevolucao(int id, [FromQuery] DateTime dataDevolucao, [FromQuery] int quilometragemFinal)
        {
            var aluguel = await _context.Alugueis.Include(a => a.Veiculo).FirstOrDefaultAsync(a => a.Id == id);
            if (aluguel == null)
                return NotFound($"Aluguel com ID {id} não encontrado.");

            if (quilometragemFinal < aluguel.QuilometragemInicial)
                return BadRequest("A quilometragem final não pode ser menor que a inicial.");

            try
            {
                aluguel.DataDevolucao = dataDevolucao;
                aluguel.QuilometragemFinal = quilometragemFinal;

                // Cálculo automático de dias e valor total
                int totalDias = (int)Math.Ceiling((dataDevolucao - aluguel.DataInicio).TotalDays);
                if (totalDias <= 0) totalDias = 1;
                aluguel.ValorTotal = totalDias * aluguel.ValorDiaria;

                // Atualiza também a quilometragem do veículo no estoque
                if (aluguel.Veiculo != null)
                {
                    aluguel.Veiculo.Quilometragem = quilometragemFinal;
                }

                await _context.SaveChangesAsync();
                return Ok(aluguel);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao registrar devolução: {ex.Message}");
            }
        }

        /// <summary>
        /// Exclui um registro de aluguel pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do aluguel a ser excluído.</param>
        /// <response code="204">Aluguel excluído com sucesso.</response>
        /// <response code="404">Aluguel não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao excluir aluguel.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deletar(int id)
        {
            var aluguel = await _context.Alugueis.FindAsync(id);
            if (aluguel == null)
                return NotFound($"Aluguel com ID {id} não encontrado.");

            try
            {
                _context.Alugueis.Remove(aluguel);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir aluguel: {ex.Message}");
            }
        }
    }
}
