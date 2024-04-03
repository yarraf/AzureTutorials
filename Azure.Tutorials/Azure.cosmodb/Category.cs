using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azure.cosmodb
{
    public record Category(
        string Id,
        string CategoryId
    ) : Item(
        Id,
        CategoryId,
        nameof(Category)
    );
}
