using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class FixDoubleEncodedUrls : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Fix double-encoded URLs in the Photos table
            migrationBuilder.Sql(@"
                UPDATE Photos 
                SET Url = REPLACE(REPLACE(Url, '%2520', '%20'), '%2525', '%25')
                WHERE Url LIKE '%2520%' OR Url LIKE '%2525%'
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This fix cannot be easily reversed, so we'll leave it as-is
            // The original double-encoded URLs were incorrect anyway
        }
    }
}
