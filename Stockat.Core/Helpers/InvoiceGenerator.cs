using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stockat.Core.Helpers;

public class InvoiceGenerator
{
    public static string CreateInvoice(
        string transactionId,
        string date,
        string paymentMethod,
        string productName,
        int productQty,
        decimal productPricePerPiece,
        string supportEmail)
    {
        decimal totalAmount = productQty * productPricePerPiece;

        return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset=""utf-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
    <title>Payment Receipt</title>
    <style>
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;
            background-color: #f3f4f6;
            min-height: 100vh;
            padding: 1rem;
        }}
        
        .container {{
            max-width: 42rem;
            margin: 0 auto;
        }}
        
        .card {{
            background-color: white;
            border-radius: 0.5rem;
            box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        
        .header {{
            background: linear-gradient(to right, #2563eb, #1d4ed8);
            padding: 1.5rem 2rem;
            text-align: center;
        }}
        
        .header img {{
            width: 8rem;
            height: auto;
            margin: 0 auto 1rem;
            display: block;
        }}
        
        .header h1 {{
            color: white;
            font-size: 1.5rem;
            font-weight: bold;
        }}
        
        .content {{
            padding: 1.5rem 2rem;
        }}
        
        .greeting {{
            color: #374151;
            margin-bottom: 1rem;
        }}
        
        .description {{
            color: #6b7280;
            margin-bottom: 1.5rem;
        }}
        
        .transaction-details {{
            background-color: #f9fafb;
            border-radius: 0.5rem;
            padding: 1.5rem;
            margin-bottom: 1.5rem;
        }}
        
        .transaction-details h2 {{
            font-size: 1.125rem;
            font-weight: 600;
            color: #1f2937;
            margin-bottom: 1rem;
        }}
        
        .detail-row {{
            display: flex;
            justify-content: space-between;
            margin-bottom: 0.75rem;
        }}
        
        .detail-row:last-child {{
            margin-bottom: 0;
        }}
        
        .detail-label {{
            color: #6b7280;
            font-weight: 500;
        }}
        
        .detail-value {{
            color: #1f2937;
        }}
        
        .detail-value.mono {{
            font-family: 'Courier New', monospace;
        }}
        
        .items-section {{
            background-color: white;
            border: 1px solid #e5e7eb;
            border-radius: 0.5rem;
            overflow: hidden;
        }}
        
        .items-header {{
            background-color: #f9fafb;
            padding: 1rem 1.5rem;
            border-bottom: 1px solid #e5e7eb;
        }}
        
        .items-header h2 {{
            font-size: 1.125rem;
            font-weight: 600;
            color: #1f2937;
        }}
        
        .item-row {{
            padding: 1rem 1.5rem;
            border-bottom: 1px solid #e5e7eb;
        }}
        
        .item-row:last-child {{
            border-bottom: none;
        }}
        
        .item-main {{
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 0.5rem;
        }}
        
        .item-name {{
            color: #1f2937;
            font-weight: 500;
        }}
        
        .item-price {{
            color: #1f2937;
            font-weight: 600;
        }}
        
        .item-details {{
            display: flex;
            justify-content: space-between;
            font-size: 0.875rem;
            color: #6b7280;
        }}
        
        .total-row {{
            padding: 1rem 1.5rem;
            background-color: #eff6ff;
            border-top: 2px solid #bfdbfe;
        }}
        
        .total-content {{
            display: flex;
            justify-content: space-between;
            align-items: center;
        }}
        
        .total-label {{
            font-size: 1.125rem;
            font-weight: bold;
            color: #1f2937;
        }}
        
        .total-amount {{
            font-size: 1.25rem;
            font-weight: bold;
            color: #2563eb;
        }}
        
        .support-section {{
            margin-top: 1.5rem;
            padding: 1rem;
            background-color: #eff6ff;
            border-radius: 0.5rem;
        }}
        
        .support-text {{
            color: #374151;
            text-align: center;
        }}
        
        .support-link {{
            color: #2563eb;
            text-decoration: underline;
        }}
        
        .support-link:hover {{
            color: #1d4ed8;
        }}
        
        .footer {{
            background-color: #f9fafb;
            padding: 1.5rem 2rem;
            text-align: center;
        }}
        
        .footer p {{
            color: #6b7280;
            font-size: 0.875rem;
            margin-bottom: 0.5rem;
        }}
        
        .footer p:last-child {{
            margin-bottom: 0;
        }}
        
        .footer a {{
            color: #2563eb;
            text-decoration: underline;
        }}
        
        .footer a:hover {{
            color: #1d4ed8;
        }}
        
        @media (max-width: 640px) {{
            .container {{
                padding: 0.5rem;
            }}
            
            .content {{
                padding: 1rem;
            }}
            
            .header {{
                padding: 1rem;
            }}
            
            .footer {{
                padding: 1rem;
            }}
        }}
    </style>
</head>
<body>
    <div class=""container"">
        <div class=""card"">
            <!-- Header -->
            <div class=""header"">
                <img src=""https://ik.imagekit.io/bgd2lywhi/Email-Assets/Logo2.png?updatedAt=1752390334452"" alt=""Company Logo"">
                <h1>Payment Receipt</h1>
            </div>

            <!-- Content -->
            <div class=""content"">
                <p class=""greeting"">Hello Customer,</p>
                <p class=""description"">Thank you for your purchase! Here's your transaction details:</p>

                <!-- Transaction Details -->
                <div class=""transaction-details"">
                    <h2>Transaction Details</h2>
                    <div class=""detail-row"">
                        <span class=""detail-label"">Transaction ID:</span>
                        <span class=""detail-value mono"">{transactionId}</span>
                    </div>
                    <div class=""detail-row"">
                        <span class=""detail-label"">Date:</span>
                        <span class=""detail-value"">{date}</span>
                    </div>
                    <div class=""detail-row"">
                        <span class=""detail-label"">Payment Method:</span>
                        <span class=""detail-value"">{paymentMethod}</span>
                    </div>
                </div>

                <!-- Items Table -->
                <div class=""items-section"">
                    <div class=""items-header"">
                        <h2>Order Items</h2>
                    </div>
                    <div class=""item-row"">
                        <div class=""item-main"">
                            <span class=""item-name"">{productName}</span>
                            <span class=""item-price"">${totalAmount:F2}</span>
                        </div>
                        <div class=""item-details"">
                            <span>Quantity: {productQty} × ${productPricePerPiece:F2} per piece</span>
                        </div>
                    </div>
                    <!-- Total Row -->
                    <div class=""total-row"">
                        <div class=""total-content"">
                            <span class=""total-label"">Total Paid:</span>
                            <span class=""total-amount"">${totalAmount:F2}</span>
                        </div>
                    </div>
                </div>

                <div class=""support-section"">
                    <p class=""support-text"">
                        Need help? Contact our support team at 
                        <a href=""mailto:{supportEmail}"" class=""support-link"">{supportEmail}</a>
                    </p>
                </div>
            </div>

            <!-- Footer -->
            <div class=""footer"">
                <p>© 2024 Your Company Name. All rights reserved.</p>
                <p>Business Address: 123 Street Name, City, Country</p>
                <p>
                    <a href=""#"">View our terms and conditions</a>
                </p>
            </div>
        </div>
    </div>
</body>
</html>";
    }

}

// Example usage:
// string invoiceHTML = InvoiceGenerator.CreateInvoice(
//     transactionId: "TRX-123456",
//     date: "January 1, 2024 10:00 AM",
//     paymentMethod: "Credit Card **** 1234",
//     productName: "Premium Widget",
//     productQty: 3,
//     productPricePerPiece: 25.00m,
//     supportEmail: "support@example.com"
// );
// Console.WriteLine(invoiceHTML); 

