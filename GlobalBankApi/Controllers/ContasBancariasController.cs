using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

using GlobalBankApi.Models;
using GlobalBankApi.Data;

namespace EleicaoBrasilApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContasController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContasController(AppDbContext context)
        {
            _context = context;
        }

        // [POST] Abertura de Conta
        [HttpPost]
        public IActionResult Post(ContaBancaria conta)
        {
            if (conta.Saldo < 0)
            {
                return BadRequest("O saldo inicial não pode ser negativo para contas internacionais.");
            }

            _context.ContasBancarias.Add(conta);
            _context.SaveChanges();

            return CreatedAtAction(nameof(GetContas), new { id = conta.Id }, conta);
        }

        // [GET] Listagem de Contas
        [HttpGet("contas")]
        public IActionResult GetContas()
        {
            var contas = _context.ContasBancarias.ToList();
            return Ok(contas);
        }

        // [POST] Registrar Transação
        [HttpPost("transacoes")]
        public IActionResult PostTransacao(Transacao transacao)
        {
            var conta = _context.ContasBancarias.Find(transacao.ContaId);

            if (conta == null)
            {
                return NotFound();
            }

            if (transacao.Tipo == "Saque")
            {
                if (transacao.Valor > conta.Saldo)
                {
                    return Conflict("Saldo Insuficiente");
                }

                conta.Saldo -= transacao.Valor;
            }
            else
            {
                conta.Saldo += transacao.Valor;
            }

            // Alerta de segurança
            if (transacao.Valor > 10000)
            {
                Console.WriteLine($"🚩 ALERTA DE SEGURANÇA: Transação de alto valor detectada para a conta {conta.Id}!");
            }

            _context.Transacoes.Add(transacao);
            _context.SaveChanges();

            return Ok(transacao);
        }

        // [GET] Extrato
        [HttpGet("transacoes/extrato/{contaId}")]
        public IActionResult GetExtrato(int contaId)
        {
            var transacoes = _context.Transacoes
                .Where(t => t.ContaId == contaId)
                .ToList();

            return Ok(transacoes);
        }
    }
}