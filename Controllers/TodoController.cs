using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Todo.Models;

namespace TodoControllers
{
    public class TodoController : Controller
    {
        private readonly TodoDbContext todoDbContext;

        public TodoController(TodoDbContext todoDbContext)
        {
            this.todoDbContext = todoDbContext;
        }

        public IActionResult Index(int? id)
        {
            Todolist model = new Todolist();


            if (id != null)
            {
                model = todoDbContext.Todos.Find(id);
            }

            ViewBag.todo = todoDbContext.Todos.ToList();
            return View(model);

        }
        [HttpPost]
        public async Task<IActionResult> Index(Todolist todos)
        {
            int i = 0;
            if (todos.Id == 0)
            {

                string fp = Path.Combine("logos/", todos.lf.FileName);
                using(var stream=new FileStream(fp,FileMode.Create))
                {
                    await todos.lf.CopyToAsync(stream);
                }

                // ✅ Base URL
                string baseUrl = $"{Request.Scheme}://{Request.Host}";

                // ✅ Set logo URL
                todos.Logo = $"{baseUrl}/{fp}";
                // INSERT
                todoDbContext.Todos.Add(todos);

                var ex = todos;
                i = 1;
                SendUpdateMail(ex, i);
            }
            else
            {
                // UPDATE
                var ex = todoDbContext.Todos.Find(todos.Id);
                if (ex != null)
                {
                    ex.Name = todos.Name;
                    ex.Status = todos.Status;

                    // 🔔 Send Mail After Update
                    
                    i = 2;
                    SendUpdateMail(ex,i);
                }
            }
            

            todoDbContext.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult delete(int id) {
            int i = 0;
            var ex = todoDbContext.Todos.Find(id);
                if (ex != null) 
            {
                todoDbContext.Todos.Remove(ex);
                SendUpdateMail(ex,3);
                todoDbContext.SaveChanges();
                return RedirectToAction("Index");
            }
            else
            {
                return BadRequest("id not found");
            }
        }
        private void SendUpdateMail(Todolist todo,int i)
        {
            var fromMail = "prasadgurav2612@gmail.com";
            var toMail = "ansarikaif0306@gmail.com"; // user ka email DB se bhi la sakte ho

            MailMessage mail = new MailMessage();
            mail.From = new MailAddress(fromMail);
            mail.To.Add(toMail);
            if (i == 1)
            {
                mail.Subject = "Todo Added Successfully";
                mail.Body = $@"
        Hello,

        Your Todo has been Added.

        🔹 Name   : {todo.Name}
        🔹 Status : {todo.Status}

        Thanks,
        Todo App Team
    ";
            }
            else if (i == 2)
            {
                mail.Subject = "Todo Updated Successfully";
                mail.Body = $@"
        Hello,

        Your Todo has been Updated.

        🔹 Name   : {todo.Name}
        🔹 Status : {todo.Status}

        Thanks,
        Todo App Team
    ";
            }
            else if (i == 3)
            {
                mail.Subject = "Todo Deleted Successfully";
                mail.Body = $@"
        Hello,

        Your Todo has been Deleted.

        🔹 Name   : {todo.Name}
        🔹 Status : {todo.Status}

        Thanks,
        Todo App Team
    ";
            }
            mail.IsBodyHtml = false;

            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
            smtp.Credentials = new NetworkCredential(fromMail, "goce smcl rkhw zjvg");
            smtp.EnableSsl = true;

            smtp.Send(mail);
        }
       

    }
}