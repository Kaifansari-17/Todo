using Azure.Storage.Blobs;
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
        private readonly IWebHostEnvironment _env;
        private readonly IConfiguration _config;
        public TodoController(TodoDbContext todoDbContext, IWebHostEnvironment _env,IConfiguration _config)
        {
            this.todoDbContext = todoDbContext;
            this._env = _env;
            this._config = _config;
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
                // ✅ Validate file
                if (todos.lf != null && todos.lf.Length > 0)
                {
                    // ✅ Folder path: wwwroot/logos
                    string uploadDir = Path.Combine(_env.WebRootPath, "logos");

                    // ✅ Create directory if not exists
                    if (!Directory.Exists(uploadDir))
                    {
                        Directory.CreateDirectory(uploadDir);
                    }

                    // ✅ Safe filename
                    string fileName = Path.GetFileName(todos.lf.FileName);

                    // ✅ Full physical path
                    string filePath = Path.Combine(uploadDir, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await todos.lf.CopyToAsync(stream);
                    }

                    // ✅ Base URL
                    string baseUrl = $"{Request.Scheme}://{Request.Host}";

                    // ✅ Save URL in DB
                    todos.Logo = $"{baseUrl}/logos/{fileName}";
                }

                // INSERT
                todoDbContext.Todos.Add(todos);
                i = 1;
                SendUpdateMail(todos, i);
            }
            else
            {
                // UPDATE
                var ex = todoDbContext.Todos.Find(todos.Id);
                if (ex != null)
                {
                    ex.Name = todos.Name;
                    ex.Status = todos.Status;

                    i = 2;
                    SendUpdateMail(ex, i);
                }
            }



            //await todoDbContext.SaveChangesAsync();


            string s=await UploadImageAsyncToAzureBlobStorage(todos.lf);
            todos.Logo = s;
            await todoDbContext.SaveChangesAsync();
            return RedirectToAction("Index");
        }



        public async Task<string> UploadImageAsyncToAzureBlobStorage(IFormFile file)
        {
            string connectionString = _config["AzureBlob:ConnectionString"];
            string containerName = _config["AzureBlob:ContainerName"];

            var blobServiceClient = new BlobServiceClient(connectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

            await containerClient.CreateIfNotExistsAsync();

            string fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);

            BlobClient blobClient = containerClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blobClient.UploadAsync(stream, overwrite: true);
            }

            return blobClient.Uri.ToString();
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