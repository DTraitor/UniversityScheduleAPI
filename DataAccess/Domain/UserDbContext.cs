using Microsoft.EntityFrameworkCore;
using DataAccess.Models;

namespace DataAccess.Domain;

public class UserDbContext(DbContextOptions<UserDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
}