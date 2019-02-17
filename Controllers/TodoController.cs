using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Todo2Api.Entities;
using Todo2Api.Models;

namespace Todo2Api.Controllers {
    [Route ("api/Todo")]
    [ApiVersion ("1.0")]
    [ApiController]
    [Authorize]
    public class TodoV1Controller : ControllerBase {
        private readonly TodoContext _context;

        public TodoV1Controller (TodoContext context) {
            _context = context;
            if (_context.TodoItems.Count () == 0) {
                _context.TodoItems.Add (new TodoItem { Name = "Item1" });
                _context.SaveChanges ();
            }
        }

        [HttpGet]
        public async Task<IActionResult> Get () {
            List<TodoItem> lTodoItem = _context.TodoItems.ToList ();

            if (lTodoItem != null && lTodoItem.Count > 0) {
                return Ok (lTodoItem);
            }

            return NotFound ();
        }

        // GET api/values/5
        [HttpGet ("{q}")]
        public async Task<IActionResult> Get (String q) {
            List<TodoItem> lTodoItem = _context.TodoItems.Where (x => x.Name == q).ToList ();

            if (lTodoItem != null && lTodoItem.Count > 0) {
                return Ok (lTodoItem);
            }

            return NotFound (new { Message = q + " Aranılan öğre bulunamadı.", Title = "Öğe bulunamadı." });
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post (String work) {
            if (!String.IsNullOrEmpty (work)) {
                TodoItem item = new TodoItem () {
                    Name = work
                };

                _context.TodoItems.Add (item);
                _context.SaveChanges ();

                return Ok (item);
            } else {
                return BadRequest (new { Message = "asd", Title = "test" });
            }
        }

        // PUT api/values/5
        [HttpPut ("{id}")]
        public void Put (int id, [FromBody] string value) { }

        // DELETE api/values/5
        [HttpDelete ("{id}")]
        public void Delete (int id) { }
    }

    [Route ("api/Todo")]
    [ApiVersion ("0.0")]
    [ApiController]
    public class TodoV2Controller : ControllerBase {
        [HttpGet]
        public async Task<IActionResult> Get () {
            List<TodoItem> lTodoItem = new List<TodoItem>(){
                new TodoItem(){Name="test1",Description="test1"},
                new TodoItem(){Name="test2",Description="test2"}
            };

            if (lTodoItem != null && lTodoItem.Count > 0) {
                return Ok (lTodoItem);
            }

            return NotFound ();
        }
    }
}