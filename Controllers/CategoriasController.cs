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
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriasController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> ObterTodas()
        {
            try
            {
                var categorias = await _context.Categorias.ToListAsync();
                return Ok(categorias);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar categorias: {ex.Message}");
            }
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> ObterPorId(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound($"Categoria com ID {id} não encontrada.");

            return Ok(categoria);
        }

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> Criar([FromBody] Categoria categoria)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _context.Categorias.Add(categoria);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterPorId), new { id = categoria.Id }, categoria);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar categoria: {ex.Message}");
            }
        }

        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Categoria categoria)
        {
            if (id != categoria.Id)
                return BadRequest("O ID informado na rota não coincide com o ID do corpo.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var existe = await _context.Categorias.AnyAsync(c => c.Id == id);
            if (!existe)
                return NotFound($"Categoria com ID {id} não encontrada.");

            try
            {
                _context.Entry(categoria).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar categoria: {ex.Message}");
            }
        }

        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Deletar(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound($"Categoria com ID {id} não encontrada.");

            var temVeiculos = await _context.Veiculos.AnyAsync(v => v.CategoriaId == id);
            if (temVeiculos)
                return BadRequest("Não é possível excluir uma categoria que possui veículos vinculados.");

            try
            {
                _context.Categorias.Remove(categoria);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir categoria: {ex.Message}");
            }
        }
    }
}
