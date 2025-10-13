using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace c2_eskolar.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAccountStatusWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing records to use the new AccountStatus workflow
            // Users with VerificationStatus "Verified" -> AccountStatus "Verified"
            // Users with VerificationStatus "Pending" -> AccountStatus "Pending"  
            // Users with VerificationStatus "Rejected" -> AccountStatus "Archived"
            // Users with AccountStatus "Deleted" -> AccountStatus "Archived"
            
            // Update StudentProfiles
            migrationBuilder.Sql(@"
                UPDATE StudentProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN VerificationStatus = 'Verified' THEN 'Verified'
                        WHEN VerificationStatus = 'Pending' THEN 'Pending'
                        WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                        ELSE 'Unverified'
                    END
            ");
            
            // Update InstitutionProfiles
            migrationBuilder.Sql(@"
                UPDATE InstitutionProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN VerificationStatus = 'Verified' THEN 'Verified'
                        WHEN VerificationStatus = 'Pending' THEN 'Pending'
                        WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                        ELSE 'Unverified'
                    END
            ");
            
            // Update BenefactorProfiles
            migrationBuilder.Sql(@"
                UPDATE BenefactorProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN VerificationStatus = 'Verified' THEN 'Verified'
                        WHEN VerificationStatus = 'Pending' THEN 'Pending'
                        WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                        ELSE 'Unverified'
                    END
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert AccountStatus changes
            migrationBuilder.Sql(@"
                UPDATE StudentProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN AccountStatus = 'Archived' THEN 'Deleted'
                        ELSE 'Active'
                    END
            ");
            
            migrationBuilder.Sql(@"
                UPDATE InstitutionProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN AccountStatus = 'Archived' THEN 'Deleted'
                        ELSE 'Active'
                    END
            ");
            
            migrationBuilder.Sql(@"
                UPDATE BenefactorProfiles 
                SET AccountStatus = 
                    CASE 
                        WHEN AccountStatus = 'Archived' THEN 'Deleted'
                        ELSE 'Active'
                    END
            ");
        }
    }
}
