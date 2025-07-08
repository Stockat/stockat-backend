using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.DTOs.OrderDTOs.OrderAnalysisDto;

public class MonthlyRevenueDto
{
    public string[] Labels { get; set; } =
       new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun",
                "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };

    public decimal[] Values { get; set; } = new decimal[12];
}

public class ReportDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public decimal[] Values { get; set; } = Array.Empty<decimal>();
}

public class TopProductReportDto
{
    public string[] Labels { get; set; } = Array.Empty<string>();
    public string[] TopProductNames { get; set; } = Array.Empty<string>();
    public decimal[] TopProductValues { get; set; } = Array.Empty<decimal>();
}

public enum ReportMetricType
{
    Revenue = 0,
    Count = 1
}