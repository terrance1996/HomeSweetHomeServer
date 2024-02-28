using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeSweetHomeServer.Models;

namespace HomeSweetHomeServer.Contexts
{
    //All database operations
    public class DatabaseContext : DbContext , IDisposable
    {
        public DatabaseContext()
        {
        }

        public DatabaseContext(DbContextOptions<DatabaseContext> options) : base(options){ }

        DbSet<UserModel> UserModels { get; set; } //User table
        DbSet<InformationModel> InformationModels { get; set; } //Information table
        DbSet<UserInformationModel> UserInformationModels { get; set; } //UserInformation table
        DbSet<FriendshipModel> FriendshipModels { get; set; } //Friendship table
        DbSet<HomeModel> HomeModels { get; set; } //Home table
        DbSet<NotepadModel> NotepadModels { get; set; } //Notepad table
        DbSet<ShoppingListModel> ShoppingModels { get; set; } //Shopping table
        DbSet<ExpenseModel> ExpenseModels { get; set; } //Expense table
        DbSet<UserExpenseModel> UserExpenseModels { get; set; } //UserExpense table
        DbSet<MealModel> MealModels { get; set; } //Meal table
        DbSet<MenuModel> MenuModels { get; set; } //Menu table
        DbSet<MenuMealModel> MenuMeals { get; set; } //MenuMeal table
        DbSet<HouseworkModel> HouseworkModels { get; set; } //Housework table
        
        protected override void OnModelCreating(ModelBuilder builder)
        {
            //Table names 
            builder.Entity<UserModel>().ToTable("User").HasOne(u => u.Home).WithMany(h => h.Users).OnDelete(DeleteBehavior.SetNull);
            builder.Entity<InformationModel>().ToTable("Information");
            builder.Entity<UserInformationModel>().ToTable("UserInformation");
            builder.Entity<FriendshipModel>().ToTable("Friendship");
            builder.Entity<HomeModel>().ToTable("Home").HasMany(h => h.Users).WithOne(u => u.Home).IsRequired().OnDelete(DeleteBehavior.SetNull);
            builder.Entity<NotepadModel>().ToTable("Notepad");
            builder.Entity<ShoppingListModel>().ToTable("ShoppingList");
            builder.Entity<ExpenseModel>().ToTable("Expense");
            builder.Entity<UserExpenseModel>().ToTable("UserExpense");
            builder.Entity<MealModel>().ToTable("Meal");
            builder.Entity<MenuModel>().ToTable("Menu");
            builder.Entity<MenuMealModel>().ToTable("MenuMeal");
            builder.Entity<HouseworkModel>().ToTable("Housework");
        }
    }
}
