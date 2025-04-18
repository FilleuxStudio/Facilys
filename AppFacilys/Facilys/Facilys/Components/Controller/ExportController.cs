using Facilys.Components.Data;
using Facilys.Components.Models;
using Facilys.Components.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Facilys.Components.Controller
{
    [ApiController]
    [Route("exports")]
    public class ExportController : ControllerBase
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;
        UserConnectionService _userConnection;

        public ExportController(IDbContextFactory<ApplicationDbContext> contextFactory, UserConnectionService userConnection)
        {
            _contextFactory = contextFactory;
            _userConnection = userConnection;
        }


        [HttpGet("csv")]
        public async Task<IActionResult> ExportCsvClients()
        {
           await _userConnection.LoadCredentialsAsync();
            using var _context = await _contextFactory.CreateDbContextAsync();
            var bytes = await ExportService.GenerateCsvAsync(_context.Clients);
            return File(bytes, "text/csv; charset=utf-8", "clients.csv");
        }

        [HttpGet("excel")]
        public async Task<IActionResult> ExportExcelClients()
        {
            await _userConnection.LoadCredentialsAsync();
            using var _context = await _contextFactory.CreateDbContextAsync();
            var bytes = await ExportService.GenerateExcelAsync(_context.Clients);
            return File(bytes,
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        "clients.xlsx");
        }
    }
}
