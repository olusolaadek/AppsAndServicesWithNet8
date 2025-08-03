using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Northwind.EntityModels;

public interface IHasLastRefreshed
{
    DateTimeOffset LastRefreshed { get; set; }
}
