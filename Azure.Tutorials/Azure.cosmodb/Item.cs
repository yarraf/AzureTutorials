using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.cosmodb
{
    public record Item(
             string Id,
             string CategoryId,
             string Type
     );
}
