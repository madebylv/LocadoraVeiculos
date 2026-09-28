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
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Veiculos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Veiculo>>> ObterTodos()
        {
            try
            {
                var veiculos = await _context.Veiculos
                    .Include(v => v.Fabricante)
                    .Include(v => v.Categoria)
                    .ToListAsync();

                return Ok(veiculos);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar veículos: {ex.Message}");
            }
        }

        // GET: api/Veiculos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Veiculo>> ObterPorId(int id)
        {
            var veiculo = await _context.Veiculos
                .Include(v => v.Fabricante)
                .Include(v => v.Categoria)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (veiculo == null)
                return NotFound($"Veículo com ID {id} não encontrado.");

            return Ok(veiculo);
        }

        // POST: api/Veiculos
        [HttpPost]
        public async Task<ActionResult<Veiculo>> Criar([FromBody] Veiculo veiculo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var fabricanteExiste = await _context.Fabricantes.AnyAsync(f => f.Id == veiculo.FabricanteId);
            if (!fabricanteExiste)
                return BadRequest($"Fabricante com ID {veiculo.FabricanteId} não existe.");

            var categoriaExiste = await _context.Categorias.AnyAsync(c => c.Id == veiculo.CategoriaId);
            if (!categoriaExiste)
                return BadRequest($"Categoria com ID {veiculo.CategoriaId} não existe.");

            var placaJaExiste = await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa);
            if (placaJaExiste)
                return BadRequest($"Já existe um veículo cadastrado com a placa '{veiculo.Placa}'.");

            try
            {
                _context.Veiculos.Add(veiculo);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterPorId), new { id = veiculo.Id }, veiculo);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar veículo: {ex.Message}");
            }
        }

        // PUT: api/Veiculos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Veiculo veiculo)
        {
            if (id != veiculo.Id)
                return BadRequest("O ID da rota difere do ID do corpo da requisição.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var veiculoBanco = await _context.Veiculos.FindAsync(id);
            if (veiculoBanco == null)
                return NotFound($"Veículo com ID {id} não encontrado.");

            var placaEmUso = await _context.Veiculos.AnyAsync(v => v.Placa == veiculo.Placa && v.Id != id);
            if (placaEmUso)
                return BadRequest($"A placa '{veiculo.Placa}' já está em uso por outro veículo.");

            try
            {
                veiculoBanco.Modelo = veiculo.Modelo;
                veiculoBanco.AnoFabricacao = veiculo.AnoFabricacao;
                veiculoBanco.Quilometragem = veiculo.Quilometragem;
                veiculoBanco.Placa = veiculo.Placa;
                veiculoBanco.Cor = veiculo.Cor;
                veiculoBanco.FabricanteId = veiculo.FabricanteId;
                veiculoBanco.CategoriaId = veiculo.CategoriaId;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar veículo: {ex.Message}");
            }
        }

        // DELETE: api/Veiculos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var veiculo = await _context.Veiculos.FindAsync(id);
            if (veiculo == null)
                return NotFound($"Veículo com ID {id} não encontrado.");

            var temAlugueis = await _context.Alugueis.AnyAsync(a => a.VeiculoId == id);
            if (temAlugueis)
                return BadRequest("Não é possível excluir um veículo que já possui histórico de aluguéis.");

            try
            {
                _context.Veiculos.Remove(veiculo);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir veículo: {ex.Message}");
            }
        }
    }
}
