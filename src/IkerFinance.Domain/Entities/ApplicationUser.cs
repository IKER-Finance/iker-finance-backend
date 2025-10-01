using Microsoft.AspNetCore.Identity;

namespace IkerFinance.Domain.Entities;

public class ApplicationUser : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    
    public int? HomeCurrencyId { get; set; }
    public virtual Currency? HomeCurrency { get; set; }
    
    public int? DefaultTransactionCurrencyId { get; set; }
    public virtual Currency? DefaultTransactionCurrency { get; set; }
    
    public string TimeZone { get; set; } = "Europe/Stockholm";
    public string? PreferredLanguage { get; set; }
    public bool IsActive { get; set; } = true;
    public bool CurrencySetupComplete { get; set; } = false;
    
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginDate { get; set; }
    
    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
    public virtual ICollection<Budget> Budgets { get; set; } = new List<Budget>();
    public virtual ICollection<Category> Categories { get; set; } = new List<Category>();
    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
} 