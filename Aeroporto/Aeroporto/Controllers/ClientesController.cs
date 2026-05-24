using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Models.Entities;
using SistemaAereo.Repositories.Interfaces;

namespace SistemaAereo.Controllers
{
    public class ClientesController : Controller
    {
        private readonly IClientePreferencialRepository _clienteRepository;
        private readonly ILogger<ClientesController> _logger;

        public ClientesController(
            IClientePreferencialRepository clienteRepository,
            ILogger<ClientesController> logger)
        {
            _clienteRepository = clienteRepository;
            _logger = logger;
        }

        // =============================================
        // MÉTODOS PRINCIPAIS - CRUD
        // =============================================

        // GET: Clientes
        public async Task<IActionResult> Index()
        {
            try
            {
                var clientes = await _clienteRepository.GetClientesAtivosAsync();
                return View(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clientes");
                TempData["Erro"] = "Erro ao carregar lista de clientes";
                return View(new List<ClientePreferencial>());
            }
        }

        // GET: Clientes/Inativos
        public async Task<IActionResult> Inativos()
        {
            try
            {
                var clientesInativos = await _clienteRepository.GetClientesInativosAsync();
                ViewBag.TotalInativos = clientesInativos.Count();
                return View(clientesInativos);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar clientes inativos");
                TempData["Erro"] = "Erro ao carregar lista de clientes inativos";
                return View(new List<ClientePreferencial>());
            }
        }

        // GET: Clientes/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Clientes/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ClientePreferencial cliente)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (await _clienteRepository.EmailExistsAsync(cliente.Email))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(cliente);
                    }

                    if (!string.IsNullOrEmpty(cliente.CPF) &&
                        await _clienteRepository.CPFExistsAsync(cliente.CPF))
                    {
                        ModelState.AddModelError("CPF", "Este CPF já está cadastrado.");
                        return View(cliente);
                    }

                    await _clienteRepository.AddAsync(cliente);
                    TempData["Sucesso"] = "Cliente cadastrado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar cliente");
                TempData["Erro"] = "Erro ao cadastrar cliente";
                return View(cliente);
            }
        }

        // GET: Clientes/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente == null)
                {
                    TempData["Erro"] = "Cliente não encontrado";
                    return RedirectToAction(nameof(Index));
                }
                return View(cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar cliente para edição");
                TempData["Erro"] = "Erro ao carregar cliente";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Clientes/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ClientePreferencial cliente)
        {
            try
            {
                if (id != cliente.ClienteId)
                {
                    TempData["Erro"] = "ID do cliente inválido";
                    return RedirectToAction(nameof(Index));
                }

                if (ModelState.IsValid)
                {
                    if (await _clienteRepository.EmailExistsAsync(cliente.Email, id))
                    {
                        ModelState.AddModelError("Email", "Este email já está cadastrado.");
                        return View(cliente);
                    }

                    if (!string.IsNullOrEmpty(cliente.CPF) &&
                        await _clienteRepository.CPFExistsAsync(cliente.CPF, id))
                    {
                        ModelState.AddModelError("CPF", "Este CPF já está cadastrado.");
                        return View(cliente);
                    }

                    await _clienteRepository.UpdateAsync(cliente);
                    TempData["Sucesso"] = "Cliente atualizado com sucesso!";
                    return RedirectToAction(nameof(Index));
                }
                return View(cliente);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _clienteRepository.ExistsAsync(c => c.ClienteId == id))
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
                return View(cliente);
            }
        }

        // POST: Clientes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente != null)
                {
                    cliente.Ativo = false;
                    await _clienteRepository.UpdateAsync(cliente);
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

        // POST: Clientes/Reativar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reativar(int id)
        {
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(id);
                if (cliente != null)
                {
                    cliente.Ativo = true;
                    await _clienteRepository.UpdateAsync(cliente);
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

            return RedirectToAction(nameof(Inativos));
        }

        // =============================================
        // MÉTODOS ADICIONAIS
        // =============================================

        // GET: Clientes/MalaDireta
        public async Task<IActionResult> MalaDireta()
        {
            try
            {
                var clientes = await _clienteRepository.GetClientesAtivosAsync();
                ViewBag.TotalClientes = clientes.Count();
                return View(clientes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao carregar mala direta");
                TempData["Erro"] = "Erro ao carregar lista de clientes";
                return View(new List<ClientePreferencial>());
            }
        }
    }
}