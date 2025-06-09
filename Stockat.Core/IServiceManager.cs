using Stockat.Core.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core;

public interface IServiceManager
{
    IAuthenticationService AuthenticationService { get; }
    IImageService ImageService { get; }

    IEmailService EmailService { get; } 
}
