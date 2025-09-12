namespace DataAccess.Enums;

// Should be a power of 2
[Flags]
public enum ProcessedByEnum
{
    None = 0,
    GroupLessons = 1,
    ElectiveLessons = 2,
}