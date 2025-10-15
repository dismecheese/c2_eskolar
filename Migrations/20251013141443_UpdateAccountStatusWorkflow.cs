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
            // Check if VerificationStatus columns exist before trying to use them
            
            // Update StudentProfiles
            migrationBuilder.Sql(@"
                IF COL_LENGTH('StudentProfiles', 'VerificationStatus') IS NOT NULL
                BEGIN
                    UPDATE StudentProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN VerificationStatus = 'Verified' THEN 'Verified'
                            WHEN VerificationStatus = 'Pending' THEN 'Pending'
                            WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                            ELSE 'Unverified'
                        END
                END
                ELSE
                BEGIN
                    UPDATE StudentProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN AccountStatus = 'Deleted' THEN 'Archived'
                            WHEN AccountStatus = 'Active' AND IsVerified = 1 THEN 'Verified'
                            WHEN AccountStatus = 'Active' AND IsVerified = 0 THEN 'Unverified'
                            ELSE AccountStatus
                        END
                END
            ");
            
            // Update InstitutionProfiles
            migrationBuilder.Sql(@"
                IF COL_LENGTH('InstitutionProfiles', 'VerificationStatus') IS NOT NULL
                BEGIN
                    UPDATE InstitutionProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN VerificationStatus = 'Verified' THEN 'Verified'
                            WHEN VerificationStatus = 'Pending' THEN 'Pending'
                            WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                            ELSE 'Unverified'
                        END
                END
                ELSE
                BEGIN
                    UPDATE InstitutionProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN AccountStatus = 'Deleted' THEN 'Archived'
                            WHEN AccountStatus = 'Active' AND IsVerified = 1 THEN 'Verified'
                            WHEN AccountStatus = 'Active' AND IsVerified = 0 THEN 'Unverified'
                            ELSE AccountStatus
                        END
                END
            ");
            
            // Update BenefactorProfiles
            migrationBuilder.Sql(@"
                IF COL_LENGTH('BenefactorProfiles', 'VerificationStatus') IS NOT NULL
                BEGIN
                    UPDATE BenefactorProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN VerificationStatus = 'Verified' THEN 'Verified'
                            WHEN VerificationStatus = 'Pending' THEN 'Pending'
                            WHEN VerificationStatus = 'Rejected' OR AccountStatus = 'Deleted' THEN 'Archived'
                            ELSE 'Unverified'
                        END
                END
                ELSE
                BEGIN
                    UPDATE BenefactorProfiles 
                    SET AccountStatus = 
                        CASE 
                            WHEN AccountStatus = 'Deleted' THEN 'Archived'
                            WHEN AccountStatus = 'Active' AND IsVerified = 1 THEN 'Verified'
                            WHEN AccountStatus = 'Active' AND IsVerified = 0 THEN 'Unverified'
                            ELSE AccountStatus
                        END
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
