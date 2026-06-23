using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChandorProject.Shared.Models;

public class DataResponse<T>
{
    public bool Success { get; set; } = false;
    public T? Data { get; set; }
    public string? Message { get; set; }
    public string?[]? Error { get; set; }
}