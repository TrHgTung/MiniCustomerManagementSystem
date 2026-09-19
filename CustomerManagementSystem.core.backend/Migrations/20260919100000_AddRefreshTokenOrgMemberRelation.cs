using CustomerManagementSystem.core.backend.Entities.AppDataContext;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CustomerManagementSystem.core.backend.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260919100000_AddRefreshTokenOrgMemberRelation")]
    public partial class AddRefreshTokenOrgMemberRelation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrgId",
                table: "RefreshTokens",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            // Existing tokens cannot be safely associated with an account, so revoke them.
            migrationBuilder.Sql("UPDATE RefreshTokens SET IsActive = 0 WHERE OrgId IS NULL;");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_OrgId",
                table: "RefreshTokens",
                column: "OrgId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_OrgMembers_OrgId",
                table: "RefreshTokens",
                column: "OrgId",
                principalTable: "OrgMembers",
                principalColumn: "OrgId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_OrgMembers_OrgId",
                table: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_OrgId",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "OrgId",
                table: "RefreshTokens");
        }
    }
}
