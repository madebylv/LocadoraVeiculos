using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LocadoraVeiculos.Data;
using LocadoraVeiculos.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LocadoraVeiculos.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AlugueisController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public AlugueisController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Alugueis
        [HttpGet]
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

        // GET: api/Alugueis/5
        [HttpGet("{id}")]
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

        // POST: api/Alugueis
        [HttpPost]
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

        // PUT: api/Alugueis/5/devolucao (Registra a devolução do veículo com quilometragem final e total)
        [HttpPut("{id}/devolucao")]
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

        // DELETE: api/Alugueis/5
        [HttpDelete("{id}")]
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
