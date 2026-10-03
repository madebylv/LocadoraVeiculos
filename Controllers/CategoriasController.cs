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
    /// Controlador responsável pelo gerenciamento de Categorias de veículos (ex: Econômico, SUV, Luxo).
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public CategoriasController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a listagem completa de todas as categorias cadastradas.
        /// </summary>
        /// <response code="200">Retorna a lista de categorias cadastradas.</response>
        /// <response code="500">Erro interno do servidor ao buscar as categorias.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Categoria>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Obtém os dados detalhados de uma categoria específica pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) da categoria.</param>
        /// <response code="200">Categoria encontrada com sucesso.</response>
        /// <response code="404">Categoria não encontrada com o ID especificado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Categoria), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Categoria>> ObterPorId(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
                return NotFound($"Categoria com ID {id} não encontrada.");

            return Ok(categoria);
        }

        /// <summary>
        /// Cadastra uma nova categoria de veículos com seu respectivo valor de diária base.
        /// </summary>
        /// <param name="categoria">Objeto contendo o nome e valor base da diária da categoria.</param>
        /// <response code="201">Categoria cadastrada com sucesso.</response>
        /// <response code="400">Dados inválidos fornecidos no corpo da requisição.</response>
        /// <response code="500">Erro interno do servidor ao cadastrar categoria.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Categoria), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Atualiza os dados de uma categoria existente.
        /// </summary>
        /// <param name="id">Identificador único (ID) da categoria a ser atualizada.</param>
        /// <param name="categoria">Objeto contendo os dados atualizados da categoria.</param>
        /// <response code="204">Categoria atualizada com sucesso.</response>
        /// <response code="400">ID da rota não coincide com o corpo ou dados inválidos.</response>
        /// <response code="404">Categoria não encontrada com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao atualizar a categoria.</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Exclui uma categoria do sistema pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) da categoria a ser excluída.</param>
        /// <response code="204">Categoria excluída com sucesso.</response>
        /// <response code="400">Não é possível excluir uma categoria que possui veículos vinculados.</response>
        /// <response code="404">Categoria não encontrada com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao excluir categoria.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
