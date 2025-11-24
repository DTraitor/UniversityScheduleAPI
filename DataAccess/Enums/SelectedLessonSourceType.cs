namespace DataAccess.Enums;

[Flags]
public enum SelectedLessonSourceType
{
    None = 0,
    Source = 1,
    Entry = 2,
    OneTimeOccurence = 4,
}