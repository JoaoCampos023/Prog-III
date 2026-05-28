using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerRepository customerRepository,
            ILogger<CustomersController> logger)
        {
            _customerRepository = customerRepository;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        /// <summary>
        /// Lista todos os clientes ativos
        /// </summary>
        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerRepository.GetActiveCustomersAsync();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clientes");
                TempData["Erro"] = "Erro ao carregar lista de clientes";
                return View(new List<Customer>());
            }
        }

        /// <summary>
        /// Lista todos os clientes inativos
        /// </summary>
        public async Task<IActionResult> Inactive()
        {
            try
            {
                var inactiveCustomers = await _customerRepository.GetInactiveCustomersAsync();
                ViewBag.TotalInactive = inactiveCustomers.Count();
                return View(inactiveCustomers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clientes inativos");
                TempData["Erro"] = "Erro ao carregar lista de clientes inativos";
                return View(new List<Customer>());
            }
        }

        /// <summary>
        /// Formulário de criação de cliente
        /// </summary>
        public IActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// Cria um novo cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _customerRepository.EmailExistsAsync(customer.Email))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(customer);
                    }

                    if (!string.IsNullOrEmpty(customer.CPF) &&
                        await _customerRepository.CPFExistsAsync(customer.CPF))
                    {
                        ModelState.AddModelError("CPF", "Este CPF já está cadastrado.");
                        return View(customer);
                    }

                    await _customerRepository.AddAsync(customer);
                    TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente");
                TempData["Erro"] = "Erro ao cadastrar cliente";
                return View(customer);
            }
        }

        /// <summary>
        /// Formulário de edição de cliente
        /// </summary>
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer == null)
                {
                    TempData["Erro"] = "Cliente não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar cliente para edição");
                TempData["Erro"] = "Erro ao carregar cliente";
                return RedirectToAction(nameof(Index));
            }
        }

        /// <summary>
        /// Atualiza um cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Customer customer)
        {
            try
            {
                if (id != customer.CustomerId)
                {
                    TempData["Erro"] = "ID do cliente inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    if (await _customerRepository.EmailExistsAsync(customer.Email, id))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(customer);
                    }

                    if (!string.IsNullOrEmpty(customer.CPF) &&
                        await _customerRepository.CPFExistsAsync(customer.CPF, id))
                    {
                        ModelState.AddModelError("CPF", "Este CPF já está cadastrado.");
                        return View(customer);
                    }

                    await _customerRepository.UpdateAsync(customer);
                    TempData["Sucesso"] = "Cliente atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(customer);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _customerRepository.ExistsAsync(c => c.CustomerId == id))
                {
                    TempData["Erro"] = "Cliente não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar cliente");
                TempData["Erro"] = "Erro ao atualizar cliente";
                return View(customer);
            }
        }

        /// <summary>
        /// Remove (desativa) um cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer != null)
                {
                    customer.IsActive = false;
                    await _customerRepository.UpdateAsync(customer);
                    TempData["Sucesso"] = "Cliente excluído com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Cliente não encontrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao excluir cliente");
                TempData["Erro"] = "Erro ao excluir cliente";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Reativa um cliente
        /// </summary>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reactivate(int id)
        {
            try
            {
                var customer = await _customerRepository.GetByIdAsync(id);
                if (customer != null)
                {
                    customer.IsActive = true;
                    await _customerRepository.UpdateAsync(customer);
                    TempData["Sucesso"] = "Cliente reativado com sucesso!";
                }
                else
                {
                    TempData["Erro"] = "Cliente não encontrado";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reativar cliente");
                TempData["Erro"] = "Erro ao reativar cliente";
            }

            return RedirectToAction(nameof(Inactive));
        }

        // =============================================
        // MÉTODOS ADICIONAIS
        // =============================================

        /// <summary>
        /// Lista de clientes para mala direta
        /// </summary>
        public async Task<IActionResult> MailingList()
        {
            try
            {
                var customers = await _customerRepository.GetActiveCustomersAsync();
                ViewBag.TotalCustomers = customers.Count();
                return View(customers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar mala direta");
                TempData["Erro"] = "Erro ao carregar lista de clientes";
                return View(new List<Customer>());
            }
        }
    }
}