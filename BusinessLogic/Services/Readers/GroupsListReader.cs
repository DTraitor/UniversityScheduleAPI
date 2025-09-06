using DataAccess.Models;
using HtmlAgilityPack;
using BusinessLogic.Services.Readers.Interfaces;

namespace BusinessLogic.Services.Readers;

public class GroupsListReader : IGroupsListReader
{
    public IEnumerable<(Group, string)> ReadGroupsList(HtmlDocument document)
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
                        GroupName = groupNode.InnerText,
                        FacultyName = facultyName,
                    };
                    yield return (newGroup, groupNode.GetAttributeValue("href", ""));
                }
            }

            accordion = accordion.SelectSingleNode(".//div[@class='accordion-item']");
        }
    }
}