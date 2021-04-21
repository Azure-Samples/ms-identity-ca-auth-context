// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using TodoListService.Models;

namespace TodoListService.Controllers
{
    [Authorize(AuthenticationSchemes =JwtBearerDefaults.AuthenticationScheme)]
    [Route("api/[controller]")]
    public class TodoListController : Controller
    {
        CommonDBContext _context;

        private readonly IHttpContextAccessor _contextAccessor;

        public TodoListController(IHttpContextAccessor contextAccessor, CommonDBContext context)
        {
            _contextAccessor = contextAccessor;

            _context = context;
        }

        // GET: api/values
        [HttpGet]
        public IEnumerable<Todo> Get()
        {
            string owner = User.Identity.Name;
            return _context.Todo.ToList();
        }

        // GET: api/values
        [HttpGet("{id}", Name = "Get")]
        public Todo Get(int id)
        {
            return _context.Todo.FirstOrDefault(t => t.Id == id);
        }

        [HttpDelete("{id}")]
        public void Delete(int id)
        {
            var todo = _context.Todo.Find(id);
            if (todo == null)
            {
                _context.Todo.Remove(todo);
                _context.SaveChanges();
            }
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody] Todo todo)
        {
            Todo todonew = new Todo() {  Owner = HttpContext.User.Identity.Name, Title = todo.Title };
            _context.Todo.Add(todonew);
            _context.SaveChanges();
            return Ok(todo);
        }

        // PATCH api/values
        [HttpPatch("{id}")]
        public IActionResult Patch(int id, [FromBody] Todo todo)
        {
            if (id != todo.Id)
            {
                return NotFound();
            }

            _context.Todo.Update(todo);
            _context.SaveChanges();

            return Ok(todo);
        }
    }
}