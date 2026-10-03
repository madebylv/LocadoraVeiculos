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
    /// Controlador responsável pelo gerenciamento da frota de Veículos da locadora.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class VeiculosController : ControllerBase
    {
        private readonly ApplicationContext _context;

        public VeiculosController(ApplicationContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Obtém a listagem completa de todos os veículos cadastrados, incluindo dados do Fabricante e da Categoria.
        /// </summary>
        /// <response code="200">Lista de veículos cadastrados retornada com sucesso.</response>
        /// <response code="500">Erro interno do servidor ao buscar veículos.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Veiculo>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Obtém os dados detalhados de um veículo específico pelo seu ID, com Fabricante e Categoria.
        /// </summary>
        /// <param name="id">Identificador único (ID) do veículo.</param>
        /// <response code="200">Veículo encontrado com sucesso.</response>
        /// <response code="404">Veículo não encontrado com o ID especificado.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(Veiculo), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
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

        /// <summary>
        /// Cadastra um novo veículo no estoque da locadora.
        /// </summary>
        /// <param name="veiculo">Objeto contendo modelo, ano de fabricação, quilometragem, placa, cor, fabricante e categoria.</param>
        /// <response code="201">Veículo cadastrado com sucesso.</response>
        /// <response code="400">Dados inválidos, placa duplicada ou Fabricante/Categoria inexistentes.</response>
        /// <response code="500">Erro interno do servidor ao cadastrar o veículo.</response>
        [HttpPost]
        [Consumes("application/json")]
        [ProducesResponseType(typeof(Veiculo), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Atualiza os dados de um veículo cadastrado.
        /// </summary>
        /// <param name="id">Identificador único (ID) do veículo a ser atualizado.</param>
        /// <param name="veiculo">Objeto contendo as novas informações do veículo.</param>
        /// <response code="204">Veículo atualizado com sucesso.</response>
        /// <response code="400">ID da rota não coincide com o corpo, dados inválidos ou placa já em uso por outro veículo.</response>
        /// <response code="404">Veículo não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao atualizar o veículo.</response>
        [HttpPut("{id}")]
        [Consumes("application/json")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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

        /// <summary>
        /// Exclui um veículo do sistema pelo seu ID.
        /// </summary>
        /// <param name="id">Identificador único (ID) do veículo a ser excluído.</param>
        /// <response code="204">Veículo excluído com sucesso.</response>
        /// <response code="400">Não é possível excluir um veículo que possui registros de aluguéis.</response>
        /// <response code="404">Veículo não encontrado com o ID especificado.</response>
        /// <response code="500">Erro interno do servidor ao excluir veículo.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
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
