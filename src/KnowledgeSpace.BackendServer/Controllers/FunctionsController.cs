using System.Linq;
using System.Threading.Tasks;
using KnowledgeSpace.BackendServer.Authorization;
using KnowledgeSpace.BackendServer.Constants;
using KnowledgeSpace.BackendServer.Data;
using KnowledgeSpace.BackendServer.Data.Entities;
using KnowledgeSpace.ViewModels;
using KnowledgeSpace.ViewModels.Systems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace KnowledgeSpace.BackendServer.Controllers
{
    public class FunctionsController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public FunctionsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.CREATE)]
        public async Task<IActionResult> PostFunction([FromBody] FunctionCreateRequest request)
        {
            var function = new Function()
            {
                Id = request.Id,
                Name = request.Name,
                ParentId = request.ParentId,
                SortOrder = request.SortOrder,
                Url = request.Url
            };
            _context.Functions.Add(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return CreatedAtAction(nameof(GetById), new { id = function.Id }, request);
            }
            else
            {
                return BadRequest();
            }
        }

        [HttpGet]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctions()
        {
            var functions = _context.Functions;

            var functionvms = await functions.Select(u => new FunctionVm()
            {
                Id = u.Id,
                Name = u.Name,
                Url = u.Url,
                SortOrder = u.SortOrder,
                ParentId = u.ParentId
            }).ToListAsync();

            return Ok(functionvms);
        }

        [HttpGet("filter")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetFunctionsPaging(string filter, int pageIndex, int pageSize)
        {
            var query = _context.Functions.AsQueryable();
            if (!string.IsNullOrEmpty(filter))
            {
                query = query.Where(x => x.Name.Contains(filter)
                || x.Id.Contains(filter)
                || x.Url.Contains(filter));
            }
            var totalRecords = await query.CountAsync();
            var items = await query.Skip((pageIndex - 1 * pageSize))
                .Take(pageSize)
                .Select(u => new FunctionVm()
                {
                    Id = u.Id,
                    Name = u.Name,
                    Url = u.Url,
                    SortOrder = u.SortOrder,
                    ParentId = u.ParentId
                })
                .ToListAsync();

            var pagination = new Pagination<FunctionVm>
            {
                Items = items,
                TotalRecords = totalRecords,
            };
            return Ok(pagination);
        }

        [HttpGet("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.VIEW)]
        public async Task<IActionResult> GetById(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound();

            var functionVm = new FunctionVm()
            {
                Id = function.Id,
                Name = function.Name,
                Url = function.Url,
                SortOrder = function.SortOrder,
                ParentId = function.ParentId
            };
            return Ok(functionVm);
        }

        [HttpPut("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.UPDATE)]
        public async Task<IActionResult> PutFunction(string id, [FromBody]FunctionCreateRequest request)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound();

            function.Name = request.Name;
            function.ParentId = request.ParentId;
            function.SortOrder = request.SortOrder;
            function.Url = request.Url;

            _context.Functions.Update(function);
            var result = await _context.SaveChangesAsync();

            if (result > 0)
            {
                return NoContent();
            }
            return BadRequest();
        }

        [HttpDelete("{id}")]
        [ClaimRequirement(FunctionCode.SYSTEM_FUNCTION, CommandCode.DELETE)]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            var function = await _context.Functions.FindAsync(id);
            if (function == null)
                return NotFound();

            _context.Functions.Remove(function);
            var result = await _context.SaveChangesAsync();
            if (result > 0)
            {
                var functionvm = new FunctionVm()
                {
                    Id = function.Id,
                    Name = function.Name,
                    Url = function.Url,
                    SortOrder = function.SortOrder,
                    ParentId = function.ParentId
                };
                return Ok(functionvm);
            }
            return BadRequest();
        }
    }
}