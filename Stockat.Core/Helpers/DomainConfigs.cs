using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Helpers;

public class DomainConfigs
{
    public DomainConfigs(IConfiguration config)
    {
        FrontURL = config["DomainUrl:FrontURL"];
        BackURL = config["DomainUrl:BackURL"];

    }
    public string FrontURL { get; set; }
    public string BackURL { get; set; }
}
