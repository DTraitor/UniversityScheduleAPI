using BusinessLogic.Services.Interfaces;
using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.GroupLessons;

public class GroupScheduleReader : IScheduleReader<GroupLesson, GroupLessonModified>
{
    public Task<(IEnumerable<GroupLessonModified>, IEnumerable<GroupLesson>)> ReadSchedule(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<GroupLesson> ReadGroupsList(HtmlDocument document)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<(Group, string)> ReadGroupsList123(HtmlDocument document)
    {
        var accordion = document.DocumentNode.SelectSingleNode("//div[@class='accordion-item']");

        while (accordion != null)
        {
            string facultyName = accordion
                .SelectSingleNode(".//button[contains(@class, 'accordion-button')]")
                ?.InnerText;

            var groupsAccordion = accordion.SelectSingleNode(".//div[contains(@class, 'accordion-collapse')]");
            var groups = groupsAccordion?.SelectNodes(".//div[@class='groups-list']/div/a");

            if (groups != null)
            {
                foreach (var groupNode in groups)
                {
                    var newGroup = new Group
                    {
                        GroupName = groupNode.InnerText.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                        FacultyName = facultyName.Replace("\r", string.Empty).Replace("\n", string.Empty).Trim(),
                    };
                    yield return (newGroup, groupNode.GetAttributeValue("href", ""));
                }
            }

            accordion = accordion.SelectSingleNode(".//div[@class='accordion-item']");
        }
    }
}