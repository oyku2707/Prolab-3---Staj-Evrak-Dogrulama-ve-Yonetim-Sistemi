using StajSistemi.Domain.Entities;
using StajSistemi.Domain.Enums;

namespace StajSistemi.Models;

public class AdminDocumentRow
{
    public int Id { get; set; }
    public string StudentName { get; set; } = "";
    public string StudentNo { get; set; } = "";
    public string FileType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; }
}

public class AdminListViewModel
{
    public List<AdminDocumentRow> Rows { get; set; } = new();
    public DocumentStatus? Status { get; set; }
    public string? Q { get; set; }
    public string? Rule { get; set; }
    public List<string> RuleOptions { get; set; } = new();
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public bool OnlyCompleted { get; set; }
}

public class ComparisonRow
{
    public string Label { get; set; } = "";
    public string Entered { get; set; } = "";
    public string Extracted { get; set; } = "";
    public string Dataset { get; set; } = "";
    public bool Mismatch { get; set; }
}

public class AdminDetailsViewModel
{
    public int DocumentId { get; set; }
    public string FileType { get; set; } = "";
    public DateTime UploadedAt { get; set; }
    public DocumentStatus Status { get; set; }
    public bool HasProfile { get; set; }
    public List<ComparisonRow> Rows { get; set; } = new();
    public List<ValidationResultRecord> Errors { get; set; } = new();
    public List<ValidationResultRecord> Warnings { get; set; } = new();
    public List<ValidationResultRecord> Passed { get; set; } = new();
    public List<ValidationResultRecord> DatasetResults { get; set; } = new();
}

public class StatusCount
{
    public DocumentStatus Status { get; set; }
    public int Count { get; set; }
}

public class RuleCount
{
    public string RuleName { get; set; } = "";
    public int FailedCount { get; set; }
}

public class DayCount
{
    public DateTime Day { get; set; }
    public int Count { get; set; }
}

public class AdminDashboardViewModel
{
    public int TotalDocuments { get; set; }
    public List<StatusCount> ByStatus { get; set; } = new();
    public List<RuleCount> TopFailedRules { get; set; } = new();
    public List<DayCount> Last7Days { get; set; } = new();
    public int ProfileCount { get; set; }
    public int LinkedProfileCount { get; set; }
}