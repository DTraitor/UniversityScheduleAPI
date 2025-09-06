using DataAccess.Models;
using HtmlAgilityPack;

namespace BusinessLogic.Services.Readers.Interfaces;

public interface IGroupsListReader
{
    IEnumerable<(Group, string)> ReadGroupsList(HtmlDocument document);
}