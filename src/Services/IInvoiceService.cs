using System;
using InvoiceGenerator.Models;

namespace InvoiceGenerator.Services;

public interface IInvoiceService
{
    Task<List<Invoice>> GetAllInvoices();
    Task<Invoice> GetInvoiceById(int invoiceId);
    Task AddInvoice(Invoice invoice);
    Task UpdateInvoice(Invoice invoice);
    Task DeleteInvoice(int invoiceId);
    Task<byte[]> GenerateInvoicePdfAsync(int invoiceId);
    Task<UserPdfData?> GetUserForPdfAsync(string userId);
}
