using IkerFinance.Domain.Common;
using IkerFinance.Domain.Enums;

namespace IkerFinance.Domain.Entities;

public class Export : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public virtual ApplicationUser User { get; set; } = null!;
    
    public ExportType Type { get; set; } = ExportType.CSV;
    public ExportContent Content { get; set; } = ExportContent.Transactions;
    public string? Filters { get; set; }
    public int RecordCount { get; set; }
    
    public string FileName { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? SelectedCategories { get; set; }
    public bool IncludeBudgetSummary { get; set; } = false;
}