using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stockat.Core;
using Stockat.Core.IServices;

namespace Stockat.Service;

public class ServiceManager: IServiceManager
{
    public ServiceManager(IRepositoryManager repositoryManager, ILoggerManager logger)
    {
        
    }
}
