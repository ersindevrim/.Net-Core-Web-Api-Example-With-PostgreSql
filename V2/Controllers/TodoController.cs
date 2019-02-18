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

namespace Todo2Api.V2.Controllers {
    [ApiVersion ("2.0")]
    [Route ("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class TodoController : ControllerBase {

        [HttpGet]
        public async Task<IActionResult> Get () {
            List<TodoItem> lTodoItem = new List<TodoItem> ();

            TodoItem rTodoItem = new TodoItem () {
                Id = 1,
                Name = "Versiyon 2",
                Description = "Versiyon 2 açıklaması",
                IsComplete = true
            };

            lTodoItem.Add(rTodoItem);

            if (lTodoItem != null && lTodoItem.Count > 0) {
                return Ok (lTodoItem);
            }

            return NotFound ();
        }

    }
}