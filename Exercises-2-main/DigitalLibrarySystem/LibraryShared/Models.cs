namespace LibraryShared;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Rank { get; set; } = string.Empty;
}

public class Book
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int Stock { get; set; }
}

public class BorrowRecord
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int BookId { get; set; }
    public DateTime BorrowDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}

public class LogEntry
{
    public int Id { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime SentDate { get; set; }
}

// DTOs
public class BorrowRequest
{
    public int UserId { get; set; }
    public int BookId { get; set; }
}

public class CanBorrowResponse
{
    public int Id { get; set; }
    public string Rank { get; set; } = string.Empty;
    public int MaxBooks { get; set; }
}