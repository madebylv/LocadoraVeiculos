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
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Fabricantes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Fabricante>>> ObterTodos()
        {
            try
            {
                var fabricantes = await _context.Fabricantes.ToListAsync();
                return Ok(fabricantes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar fabricantes: {ex.Message}");
            }
        }

        // GET: api/Fabricantes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Fabricante>> ObterPorId(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound($"Fabricante com ID {id} não encontrado.");

            return Ok(fabricante);
        }

        // POST: api/Fabricantes
        [HttpPost]
        public async Task<ActionResult<Fabricante>> Criar([FromBody] Fabricante fabricante)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Fabricantes.Add(fabricante);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterPorId), new { id = fabricante.Id }, fabricante);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar fabricante: {ex.Message}");
            }
        }

        // PUT: api/Fabricantes/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Fabricante fabricante)
        {
            if (id != fabricante.Id)
                return BadRequest("O ID informado na rota não coincide com o ID do corpo da requisição.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Fabricantes.AnyAsync(f => f.Id == id);
            if (!existe)
                return NotFound($"Fabricante com ID {id} não encontrado.");

            try
            {
                _context.Entry(fabricante).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar fabricante: {ex.Message}");
            }
        }

        // DELETE: api/Fabricantes/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound($"Fabricante com ID {id} não encontrado.");

            var temVeiculos = await _context.Veiculos.AnyAsync(v => v.FabricanteId == id);
            if (temVeiculos)
                return BadRequest("Não é possível excluir um fabricante que possui veículos vinculados.");

            try
            {
                _context.Fabricantes.Remove(fabricante);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir fabricante: {ex.Message}");
            }
        }
    }
}
