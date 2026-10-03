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
    /// Controlador responsável pela gestão do cadastro de Clientes da locadora.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a listagem completa de todos os clientes cadastrados.
        /// </summary>
        /// <response code="200">Lista de clientes obtida com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao buscar clientes.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Cliente>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<Cliente>>> ObterTodos()
        {
            try
            {
                var clientes = await _context.Clientes.ToListAsync();
                return Ok(clientes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno ao buscar clientes: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtém os dados detalhados de um cliente específico pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do cliente.</param>
        /// <response code="200">Cliente encontrado com sucesso.</response>
        /// <response code="404">Cliente não encontrado com o ID especificado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Cliente), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Cliente>> ObterPorId(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound($"Cliente com ID {id} não encontrado.");

            return Ok(cliente);
        }

        /// <summary>
        /// Cadastra um novo cliente no sistema com validação de unicidade de CPF.
        /// </summary>
        /// <param name="cliente">Objeto contendo dados do cliente (Nome, CPF, Email, Telefone).</param>
        /// <response code="201">Cliente cadastrado com sucesso.</response>
        /// <response code="400">Dados inválidos ou CPF já existente no sistema.</response>
        /// <response code="500">Erro interno do servidor ao cadastrar cliente.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Cliente), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<Cliente>> Criar([FromBody] Cliente cliente)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var cpfJaExiste = await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF);
            if (cpfJaExiste)
                return BadRequest($"Já existe um cliente cadastrado com o CPF '{cliente.CPF}'.");

            try
            {
                _context.Clientes.Add(cliente);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(ObterPorId), new { id = cliente.Id }, cliente);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao cadastrar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza os dados de um cliente existente.
        /// </summary>
        /// <param name="id">Identificador único (ID) do cliente a ser atualizado.</param>
        /// <param name="cliente">Objeto contendo os dados atualizados do cliente.</param>
        /// <response code="204">Cliente atualizado com sucesso.</response>
        /// <response code="400">ID da rota não coincide com o corpo, dados inválidos ou CPF em uso por outro cliente.</response>
        /// <response code="404">Cliente não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao atualizar cliente.</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Atualizar(int id, [FromBody] Cliente cliente)
        {
            if (id != cliente.Id)
                return BadRequest("O ID da rota difere do ID do corpo da requisição.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var clienteBanco = await _context.Clientes.FindAsync(id);
            if (clienteBanco == null)
                return NotFound($"Cliente com ID {id} não encontrado.");

            var cpfEmUso = await _context.Clientes.AnyAsync(c => c.CPF == cliente.CPF && c.Id != id);
            if (cpfEmUso)
                return BadRequest($"O CPF '{cliente.CPF}' já está em uso por outro cliente.");

            try
            {
                clienteBanco.Nome = cliente.Nome;
                clienteBanco.CPF = cliente.CPF;
                clienteBanco.Email = cliente.Email;
                clienteBanco.Telefone = cliente.Telefone;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao atualizar cliente: {ex.Message}");
            }
        }

        /// <summary>
        /// Exclui um cliente do sistema pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do cliente a ser excluído.</param>
        /// <response code="204">Cliente excluído com sucesso.</response>
        /// <response code="400">Não é possível excluir um cliente que possui histórico de aluguéis registrados.</response>
        /// <response code="404">Cliente não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao excluir cliente.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Deletar(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound($"Cliente com ID {id} não encontrado.");

            var temAlugueis = await _context.Alugueis.AnyAsync(a => a.ClienteId == id);
            if (temAlugueis)
                return BadRequest("Não é possível excluir um cliente que possui aluguéis registrados.");

            try
            {
                _context.Clientes.Remove(cliente);
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao excluir cliente: {ex.Message}");
            }
        }
    }
}
