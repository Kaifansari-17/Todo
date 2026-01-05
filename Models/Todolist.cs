using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace Todo.Models;

public partial class Todolist
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Des { get; set; }

    public string? Status { get; set; }

    public string? Logo { get; set; }

    [NotMapped]
    public IFormFile lf { get; set; }

}
