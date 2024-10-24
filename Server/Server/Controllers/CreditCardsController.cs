﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server;
using Server.Models;
using Server.Models.DTO;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class CreditCardsController : ControllerBase
    {
        private readonly Context _context;

        public CreditCardsController(Context context)
        {
            _context = context;
        }

        // GET: api/CreditCards
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CreditCard>>> GetCreditCard()
        {
            var creditCards = await _context.CreditCard
                                            .Where(c => c.Estatus == "Activo")
                                            .ToListAsync();

            return Ok(creditCards);
        }

        // GET: api/CreditCards/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CreditCard>> GetCreditCard(int id)
        {
            var creditCard = await _context.CreditCard
                                           .Where(c => c.Id == id && c.Estatus == "Activo")
                                           .FirstOrDefaultAsync();

            if (creditCard == null)
            {
                return NotFound();
            }

            return Ok(creditCard);
        }

        private string OcultaNumero(string numero)
        {
            string oculto = "";
            for (int i = 0; i < numero.Length; i++)
            {
                if (i < numero.Length - 4)
                {
                    oculto += "*";
                }
                else
                {
                    oculto += numero[i];
                }
            }
            return oculto;
        }

        private bool IsExpired(string expiryDate)
        {
            string[] date = expiryDate.Split("/");
            if (date.Length != 2)
            {
                // Formato de fecha inválido
                return true;
            }

            if (!int.TryParse(date[0], out int month) || !int.TryParse(date[1], out int year))
            {
                // Valores de fecha no válidos
                return true;
            }

            // Validar el mes
            if (month < 1 || month > 12)
            {
                return true;
            }

            // Validar el año
            if (year < DateTime.Now.Year % 100)
            {
                return true;
            }
            else if (year == DateTime.Now.Year % 100)
            {
                if (month < DateTime.Now.Month)
                {
                    return true;
                }
            }

            return false;
        }

        // PUT: api/CreditCards/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCreditCard(int id, CreditCard creditCard)
        {
            if (id != creditCard.Id)
            {
                return BadRequest();
            }

            _context.Entry(creditCard).State = EntityState.Modified;

            if (!IsValidCreditCardNumber(creditCard.CardNumber)) { 
            return BadRequest("El número de tarjeta no es válido");
            }

            if (IsExpired(creditCard.ExpiryDate))
            {
                return BadRequest("La tarjeta está vencida");
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CreditCardExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(new CreditCardDTO
            {
                Id = creditCard.Id,
                CardHolderName = creditCard.CardHolderName,
                CardNumber = creditCard.CardNumber,
                ExpiryDate = creditCard.ExpiryDate,
                UserId = creditCard.UserId
            });
        }

        // POST: api/CreditCards
        [HttpPost]
        public async Task<ActionResult<CreditCardDTO>> PostCreditCard(CreditCard creditCard)
        {
            if(!IsValidCreditCardNumber(creditCard.CardNumber))
            {
                return BadRequest("El número de tarjeta no es válido");
            }

            if (IsExpired(creditCard.ExpiryDate))
            {
                return BadRequest("La tarjeta está vencida");
            }

            creditCard.Estatus = "Activo";

            _context.CreditCard.Add(creditCard);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCreditCard", new { id = creditCard.Id }, new CreditCardDTO
            {
                Id = creditCard.Id,
                CardHolderName = creditCard.CardHolderName,
                CardNumber = creditCard.CardNumber,
                ExpiryDate = creditCard.ExpiryDate,
                UserId = creditCard.UserId
            });
        }

        [HttpPost]
        [Route("bulk")]
        public async Task<ActionResult<CreditCardDTO[]>> PostCreditCards(CreditCard[] creditCards)
        {
            var creditCardsDTO = new List<CreditCardDTO>();
            foreach (var creditCard in creditCards)
            {
                if (!IsValidCreditCardNumber(creditCard.CardNumber))
                {
                    return BadRequest("El número de tarjeta no es válido");
                }
                if (IsExpired(creditCard.ExpiryDate))
                {
                    return BadRequest("La tarjeta está vencida");
                }

                creditCard.Estatus = "Activo";
            }

            _context.CreditCard.AddRange(creditCards);
            await _context.SaveChangesAsync();

            foreach (var creditCard in creditCards)
            {
                creditCardsDTO.Add(new CreditCardDTO
                {
                    Id = creditCard.Id,
                    CardHolderName = creditCard.CardHolderName,
                    CardNumber = OcultaNumero(creditCard.CardNumber),
                    ExpiryDate = creditCard.ExpiryDate,
                    UserId = creditCard.UserId
                });
            }

            return CreatedAtAction("GetCreditCard", creditCardsDTO);
        }

        // DELETE: api/CreditCards/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCreditCard(int id)
        {
            var creditCard = await _context.CreditCard.FindAsync(id);
            if (creditCard == null)
            {
                return NotFound();
            }

            creditCard.Estatus = "Inactivo";
            _context.Entry(creditCard).State = EntityState.Modified;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CreditCardExists(int id)
        {
            return _context.CreditCard.Any(e => e.Id == id);
        }

        private bool IsValidCreditCardNumber(string cardNumber)
        {
            // Eliminamos todos los espacios o caracteres que no sean dígitos
            cardNumber = cardNumber.Replace(" ", "");

            // Verificamos si el string contiene solo dígitos
            if (!long.TryParse(cardNumber, out _))
            {
                return false;
            }

            int sum = 0;
            bool alternate = false;

            // Recorremos el número de la tarjeta de derecha a izquierda
            for (int i = cardNumber.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(cardNumber[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9)
                    {
                        n -= 9;
                    }
                }

                sum += n;
                alternate = !alternate;
            }

            // Si la suma es múltiplo de 10, el número es válido
            return (sum % 10 == 0);
        }
    }
}
