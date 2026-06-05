using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SistemaAereo.Data.Context;
using SistemaAereo.Models.Enums;
using SistemaAereo.Models.ViewModels;

namespace SistemaAereo.Controllers
{
    [Authorize(Roles = "Admin,Funcionario")]
    public class ReportsController : Controller
    {
        private readonly AirportsContext _context;
        private readonly ILogger<ReportsController> _logger;

        public ReportsController(AirportsContext context, ILogger<ReportsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Página principal de relatórios
        public IActionResult Index()
        {
            return View();
        }

        // Relatório de faturamento por período
        public async Task<IActionResult> Revenue(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddMonths(-1);

            var model = new RevenueReportViewModel
            {
                StartDate = start,
                EndDate = end,
                DailyRevenue = new List<DailyRevenueDto>(),
                MonthlyRevenue = new List<MonthlyRevenueDto>()
            };

            // Busca faturamento agrupado por dia
            var dailyRevenue = await _context.Tickets
                .Where(t => t.IssueDate >= start && t.IssueDate <= end && t.Status != TicketStatus.Cancelled)
                .GroupBy(t => t.IssueDate.Date)
                .Select(g => new DailyRevenueDto
                {
                    Date = g.Key,
                    Amount = g.Sum(t => t.Price),
                    Quantity = g.Count()
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            model.DailyRevenue = dailyRevenue;
            model.TotalRevenue = dailyRevenue.Sum(r => r.Amount);
            model.TotalTickets = dailyRevenue.Sum(r => r.Quantity);
            model.AverageTicketPrice = model.TotalTickets > 0 ? model.TotalRevenue / model.TotalTickets : 0;

            // Busca faturamento dos últimos 12 meses
            var last12Months = DateTime.Now.AddMonths(-11);
            var startOfMonth = new DateTime(last12Months.Year, last12Months.Month, 1);

            var monthlyRevenueRaw = await _context.Tickets
                .Where(t => t.IssueDate >= startOfMonth && t.Status != TicketStatus.Cancelled)
                .GroupBy(t => new { t.IssueDate.Year, t.IssueDate.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Amount = g.Sum(t => t.Price),
                    Quantity = g.Count()
                })
                .OrderBy(m => m.Year)
                .ThenBy(m => m.Month)
                .ToListAsync();

            model.MonthlyRevenue = monthlyRevenueRaw.Select(m => new MonthlyRevenueDto
            {
                Month = $"{m.Year}-{m.Month:D2}",
                Amount = m.Amount,
                Quantity = m.Quantity
            }).ToList();

            return View(model);
        }

        // Relatório de ocupação de voos
        public async Task<IActionResult> FlightOccupancy(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddMonths(-1);

            ViewBag.StartDate = start.ToString("yyyy-MM-dd");
            ViewBag.EndDate = end.ToString("yyyy-MM-dd");

            // Busca voos no período com suas poltronas
            var flights = await _context.Flights
                .Include(f => f.DepartureAirport)
                .Include(f => f.ArrivalAirport)
                .Include(f => f.Seats)
                .Where(f => f.DepartureTime >= start && f.DepartureTime <= end)
                .ToListAsync();

            var model = new FlightOccupancyReportViewModel
            {
                Flights = new List<FlightOccupancyDto>(),
                TotalFlights = flights.Count
            };

            // Calcula a ocupação de cada voo
            foreach (var flight in flights)
            {
                var totalSeats = flight.Seats.Count;
                var occupiedSeats = flight.Seats.Count(s => !s.IsAvailable);
                var occupancy = totalSeats > 0 ? (double)occupiedSeats / totalSeats * 100 : 0;

                model.Flights.Add(new FlightOccupancyDto
                {
                    FlightId = flight.FlightId,
                    FlightNumber = flight.FlightNumber,
                    Origin = flight.DepartureAirport?.IATACode ?? "N/A",
                    Destination = flight.ArrivalAirport?.IATACode ?? "N/A",
                    DepartureTime = flight.DepartureTime,
                    TotalSeats = totalSeats,
                    OccupiedSeats = occupiedSeats,
                    OccupancyPercentage = Math.Round(occupancy, 2)
                });
            }

            model.AverageOccupancy = model.Flights.Any() ? Math.Round(model.Flights.Average(f => f.OccupancyPercentage), 2) : 0;
            model.TotalPassengers = model.Flights.Sum(f => f.OccupiedSeats);

            return View(model);
        }

        // Relatório de clientes mais frequentes (ranking)
        public async Task<IActionResult> TopCustomers(int quantity = 10)
        {
            // Agrupa passagens por cliente e calcula totais
            var customers = await _context.Tickets
                .Where(t => t.Status != TicketStatus.Cancelled)
                .GroupBy(t => new { t.CustomerId, t.Customer.Name, t.Customer.Email })
                .Select(g => new TopCustomerDto
                {
                    CustomerId = g.Key.CustomerId,
                    Name = g.Key.Name,
                    Email = g.Key.Email,
                    TotalTickets = g.Count(),
                    TotalAmount = g.Sum(t => t.Price)
                })
                .OrderByDescending(c => c.TotalTickets)
                .Take(quantity)
                .ToListAsync();

            // Calcula o ticket médio de cada cliente
            foreach (var customer in customers)
            {
                customer.AverageTicketPrice = customer.TotalTickets > 0 ? customer.TotalAmount / customer.TotalTickets : 0;
            }

            var model = new TopCustomersReportViewModel
            {
                TopCustomers = customers,
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalActiveCustomers = await _context.Customers.CountAsync(c => c.IsActive)
            };

            return View(model);
        }

        // Exporta o relatório de faturamento para CSV
        public async Task<IActionResult> ExportRevenueToCsv(DateTime? startDate, DateTime? endDate)
        {
            var end = endDate ?? DateTime.Now;
            var start = startDate ?? end.AddMonths(-1);

            var revenue = await _context.Tickets
                .Where(t => t.IssueDate >= start && t.IssueDate <= end && t.Status != TicketStatus.Cancelled)
                .GroupBy(t => t.IssueDate.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Quantity = g.Count(),
                    Amount = g.Sum(t => t.Price)
                })
                .OrderBy(d => d.Date)
                .ToListAsync();

            // Gera arquivo CSV
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Data;Quantidade;Valor");
            foreach (var item in revenue)
            {
                csv.AppendLine($"{item.Date:dd/MM/yyyy};{item.Quantity};{item.Amount.ToString("C")}");
            }

            var bytes = System.Text.Encoding.UTF8.GetBytes(csv.ToString());
            return File(bytes, "text/csv", $"faturamento_{start:yyyyMMdd}_{end:yyyyMMdd}.csv");
        }
    }
}