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
    /// Controlador responsável pelo gerenciamento de Fabricantes de veículos.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class FabricantesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public FabricantesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a listagem completa de todos os fabricantes cadastrados.
        /// </summary>
        /// <response code="200">Retorna a lista de fabricantes cadastrados com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao consultar o banco de dados.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Fabricante>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Obtém os dados detalhados de um fabricante específico pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do fabricante.</param>
        /// <response code="200">Fabricante encontrado com sucesso.</response>
        /// <response code="404">Fabricante não encontrado com o ID especificado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Fabricante), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Fabricante>> ObterPorId(int id)
        {
            var fabricante = await _context.Fabricantes.FindAsync(id);
            if (fabricante == null)
                return NotFound($"Fabricante com ID {id} não encontrado.");

            return Ok(fabricante);
        }

        /// <summary>
        /// Cadastra um novo fabricante no sistema.
        /// </summary>
        /// <param name="fabricante">Objeto contendo os dados do fabricante a ser cadastrado.</param>
        /// <response code="201">Fabricante cadastrado com sucesso.</response>
        /// <response code="400">Dados inválidos fornecidos no corpo da requisição.</response>
        /// <response code="500">Erro interno do servidor ao salvar no banco de dados.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Fabricante), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Atualiza os dados de um fabricante existente.
        /// </summary>
        /// <param name="id">Identificador único (ID) do fabricante a ser atualizado.</param>
        /// <param name="fabricante">Objeto contendo os novos dados do fabricante.</param>
        /// <response code="204">Fabricante atualizado com sucesso.</response>
        /// <response code="400">ID da rota não coincide com o corpo ou dados inválidos.</response>
        /// <response code="404">Fabricante não encontrado para atualização.</response>
        /// <response code="500">Erro interno do servidor ao atualizar.</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Exclui um fabricante do sistema pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do fabricante a ser excluído.</param>
        /// <response code="204">Fabricante excluído com sucesso.</response>
        /// <response code="400">Não é possível excluir fabricante vinculado a veículos existentes.</response>
        /// <response code="404">Fabricante não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao excluir.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
