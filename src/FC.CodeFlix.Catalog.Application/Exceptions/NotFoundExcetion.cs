using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FC.CodeFlix.Catalog.Application.Exceptions;

public class NotFoundExcetion : ApplicationException
{
    public NotFoundExcetion(string? message) : base(message)
    {
    }
}
