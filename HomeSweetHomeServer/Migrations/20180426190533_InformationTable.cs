using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace HomeSweetHomeServer.Migrations
{
    public partial class InformationTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "FirstName", "string", 1 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "LastName", "string", 2 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "Email", "string", 3 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "PhoneNumber", "string", 4 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "RegistrationDate", "DateTime", 5 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "EmailVerificationCode", "string", 6 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "EmailVerificationCodeGenerateDate", "DateTime", 7 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ForgotPasswordVerificationCode", "string", 8 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ForgotPasswordVerificationCodeGenerateDate", "DateTime", 9 });

            migrationBuilder.InsertData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ReportCount", "int", 10 });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "FirstName", "string", 1 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "LastName", "string", 2 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "Email", "string", 3 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "PhoneNumber", "string", 4 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "RegistrationDate", "DateTime", 5 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "EmailVerificationCode", "string", 6 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "EmailVerificationCodeGenerateDate", "DateTime", 7 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ForgotPasswordVerificationCode", "string", 8 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ForgotPasswordVerificationCodeGenerateDate", "DateTime", 9 });

            migrationBuilder.DeleteData("Information",
                new string[] { "InformationName", "InformationType", "Id" },
                new object[] { "ReportCount", "int", 10 });
        }
    }
}
