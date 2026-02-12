using InvoiceGenerator.Data;
using InvoiceGenerator.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using QuestPDF.Fluent;

namespace InvoiceGenerator.Services;

public class InvoiceService : IInvoiceService
{
    private readonly ApplicationDbContext _context;
    private readonly IUserIdentityService _userIdentity;

    public InvoiceService(ApplicationDbContext context, IUserIdentityService userIdentity)
    {
        _context = context;
        _userIdentity = userIdentity;
    }

    public async Task<List<Invoice>> GetAllInvoices()
    {
        var userId = await _userIdentity.GetCurrentUserId();

        if (string.IsNullOrEmpty(userId)) return new List<Invoice>();

        var invoices = await _context.Invoices
                .Where(i => i.UserId == userId)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();

        return invoices;
    }

    public async Task<Invoice> GetInvoiceById(int invoiceId)
    {
        var userId = await _userIdentity.GetCurrentUserId();
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId);

        if (invoice == null)
        {
            throw new Exception("Invoice not found");
        }

        return invoice;
    }


    public async Task AddInvoice(Invoice invoice)
    {
        var userId = await _userIdentity.GetCurrentUserId();

        invoice.UserId = userId;
        _context.Invoices.Add(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateInvoice(Invoice invoice)
    {
        var userId = await _userIdentity.GetCurrentUserId();

        if (invoice.UserId != userId)
        {
            throw new UnauthorizedAccessException("You do not have permission to edit this invoice.");
        }
        _context.Invoices.Update(invoice);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteInvoice(int invoiceId)
    {
        var userId = await _userIdentity.GetCurrentUserId();
        var invoice = await _context.Invoices
            .FirstOrDefaultAsync(i => i.Id == invoiceId && i.UserId == userId);

        if (invoice != null)
        {
            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<byte[]> GenerateInvoicePdfAsync(int invoiceId)
    {
        var invoice = await GetInvoiceById(invoiceId);
        var userId = await _userIdentity.GetCurrentUserId();

        var userData = await GetUserForPdfAsync(userId!);


        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(50);
                page.Header().Column(header =>
                {
                    header.Item().AlignCenter().Text($"{userData!.CompanyName}")
                    .FontSize(28).SemiBold();

                    header.Item().PaddingTop(5).LineHorizontal(1).LineColor("#e6e9ef"); // Subtle divider
                });

                page.Content().PaddingVertical(20).Column(col =>
                {
                    col.Item().AlignLeft().Column(innerCol =>
                    {
                        innerCol.Item().Text($"Owner: {userData!.FirstName} {userData.LastName}")
                            .FontSize(14).Medium();

                        innerCol.Item().Text($"Email: {userData.Email}")
                            .FontSize(14);
                    });
                    col.Item().PaddingTop(10);
                    col.Item().AlignLeft().Text("Partner").FontSize(14).SemiBold();
                    col.Item().AlignLeft().Column(summary =>
                    {
                        summary.Item().Text(invoice.PartnerName).FontSize(12);
                        summary.Item().Text($"Location: {invoice.PartnerLocation}").FontSize(12);
                        summary.Item().Text($"Street: {invoice.PartnerStreet}").FontSize(12);
                    });

                    col.Item().PaddingVertical(20).LineHorizontal(0.5f).LineColor("#ccd0da");


                    col.Item().PaddingBottom(10).Text("Service Details").FontSize(14).SemiBold();

                    col.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(3); // Description
                            columns.RelativeColumn(1); // Hours
                            columns.RelativeColumn(1); // Price
                            columns.RelativeColumn(2); // SubTotal
                            columns.RelativeColumn(2); // Payment Method
                        });


                        table.Header(header =>
                        {
                            header.Cell().Text("Description");
                            header.Cell().AlignRight().Text("Rate");
                            header.Cell().AlignRight().Text("Hours");
                            header.Cell().AlignRight().Text("SubTotal");
                            header.Cell().AlignRight().Text("Payment Method");
                        });

                        table.Cell().Text(invoice.ServiceDescription).FontSize(10);
                        table.Cell().AlignRight().Text($"{invoice.ServiceSinglePrice:C}").FontSize(10);
                        table.Cell().AlignRight().Text($"{invoice.ServiceHours}").FontSize(10);
                        table.Cell().AlignRight().Text($"{invoice.TotalAmount:C}").FontSize(10);
                        table.Cell().AlignRight().Text(invoice.PaymentMethod).FontSize(10);
                    });

                    col.Item().PaddingTop(10).Column(summaryCol =>
                    {
                        summaryCol.Item().LineHorizontal(0.5f).LineColor("#ccd0da");
                        summaryCol.Item().PaddingTop(20).Row(row =>
                        {
                            row.RelativeItem(2);
                            row.Spacing(10);

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Invoice No.");
                                c.Item().Text(invoice.InvoiceNumber);
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Payment Date");
                                c.Item().Text(invoice.CreatedAt.AddDays(30).ToString("MMM dd, yyyy"));
                            });

                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Total");
                                c.Item().Text($"{invoice.TotalAmount:C}")
                                    .FontColor("#ff5555").SemiBold().FontSize(14);
                            });
                        });
                    });

                });
            });
        });
        return document.GeneratePdf();
    }

    // Helper method
    public async Task<UserPdfData?> GetUserForPdfAsync(string userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .Select(u => new UserPdfData
            {
                FirstName = u.FirstName,
                LastName = u.LastName,
                CompanyName = u.CompanyName!,
                Email = u.Email!,
            })
            .FirstOrDefaultAsync();
    }
}
