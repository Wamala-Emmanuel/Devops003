using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Laboremus_AuthorizationService.Services.RequestCsvService
{
    public interface IRequestCsvService
    {
        Task WriteToCsvFileAsync(Guid requestId);
    }
}
