using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Entities;
using SistemaAereo.Models.ViewModels;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly AirportsContext _context;
        private readonly ILogger<CustomersController> _logger;

        public CustomersController(
            ICustomerRepository customerRepository,
            AirportsContext context,
            ILogger<CustomersController> logger)
        {
            _customerRepository = customerRepository;
            _context = context;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        // Lista todos os clientes ativos
        public async Task<IActionResult> Index()
        {
            try
            {
                // Busca clientes ativos
                var activeCustomers = await _customerRepository.GetActiveCustomersAsync();

                // Busca todos os clientes para estatísticas
                var allCustomers = await _customerRepository.GetAllCustomersAsync();

                var activeCount = activeCustomers.Count();
                var inactiveCount = allCustomers.Count() - activeCount;

                var model = new CustomerDashboardViewModel
                {
                    Customers = activeCustomers,
                    TotalCustomers = allCustomers.Count(),
                    ActiveCustomers = activeCount,
                    InactiveCustomers = inactiveCount
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clientes");
                TempData["Erro"] = "Erro ao carregar lista de clientes";
                return View(new CustomerDashboardViewModel
                {
                    Customers = new List<Customer>()
                });
            }
        }

        // Retorna todos os clientes em formato JSON para o front-end (filtros e paginação)
        [HttpGet]
        public async Task<IActionResult> GetAllCustomersJson()
        {
            try
            {
                var customers = await _customerRepository.GetAllCustomersAsync();
                var result = customers.Select(c => new
                {
                    customerId = c.CustomerId,
                    name = c.Name,
                    email = c.Email,
                    phone = c.Phone,
                    city = c.City,
                    state = c.State,
                    registrationDate = c.RegistrationDate.ToString("dd/MM/yyyy"),
                    isActive = c.IsActive
                });

                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar clientes para JSON");
                return StatusCode(500, new { error = "Erro ao carregar clientes" });
            }
        }

        // Lista todos os clientes inativos
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

        // Formulário de criação de cliente
        public IActionResult Create()
        {
            return View();
        }

        // Cria um novo cliente
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Valida se o email já está cadastrado
                    if (await _customerRepository.EmailExistsAsync(customer.Email))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(customer);
                    }

                    // Valida se o CPF já está cadastrado
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

        // Formulário de edição de cliente
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

        // Atualiza os dados de um cliente
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
                    // Valida se o email já está cadastrado (excluindo o próprio cliente)
                    if (await _customerRepository.EmailExistsAsync(customer.Email, id))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(customer);
                    }

                    // Valida se o CPF já está cadastrado (excluindo o próprio cliente)
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

        // Remove (desativa) um cliente - exclusão lógica
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

        // Reativa um cliente que estava inativo
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

        // Lista de clientes para mala direta (exportação de emails)
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