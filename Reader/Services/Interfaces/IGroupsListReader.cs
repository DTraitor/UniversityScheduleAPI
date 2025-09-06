using DataAccess.Models;
using HtmlAgilityPack;

namespace Reader.Services.Interfaces;

public interface IGroupsListReader
{
    IEnumerable<(Group, string)> ReadGroupsList(HtmlDocument document);
}