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
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public ClientesController(ApplicationContext context)
        {
            _context = context;
        }

        // GET: api/Clientes
        [HttpGet]
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

        // GET: api/Clientes/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Cliente>> ObterPorId(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound($"Cliente com ID {id} não encontrado.");

            return Ok(cliente);
        }

        // POST: api/Clientes
        [HttpPost]
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

        // PUT: api/Clientes/5
        [HttpPut("{id}")]
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

        // DELETE: api/Clientes/5
        [HttpDelete("{id}")]
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
