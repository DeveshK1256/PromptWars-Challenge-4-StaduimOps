using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace StadiumOps.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAiOrchestrationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AgentKey",
                table: "AiConversations",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "AiConversations",
                type: "nvarchar(160)",
                maxLength: 160,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "ConfidenceScore",
                table: "AiConversations",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "EscalationRecommended",
                table: "AiConversations",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GroundingSummary",
                table: "AiConversations",
                type: "nvarchar(1200)",
                maxLength: 1200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "GuardrailsJson",
                table: "AiConversations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PromptVersion",
                table: "AiConversations",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourcesJson",
                table: "AiConversations",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "AiAnalyticsEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EventName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Intent = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AgentKey = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    AgentName = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    ConfidenceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EscalationRecommended = table.Column<bool>(type: "bit", nullable: false),
                    LatencyMs = table.Column<int>(type: "int", nullable: false),
                    TokensUsed = table.Column<int>(type: "int", nullable: false),
                    MetadataJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiAnalyticsEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AiKnowledgeDocuments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    SourceType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    SourceUri = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    ContentSummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    Language = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    IsApproved = table.Column<bool>(type: "bit", nullable: false),
                    EffectiveFrom = table.Column<DateOnly>(type: "date", nullable: true),
                    EffectiveTo = table.Column<DateOnly>(type: "date", nullable: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AiKnowledgeDocuments", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_AgentKey",
                table: "AiConversations",
                column: "AgentKey");

            migrationBuilder.CreateIndex(
                name: "IX_AiConversations_Intent",
                table: "AiConversations",
                column: "Intent");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyticsEvents_AgentKey",
                table: "AiAnalyticsEvents",
                column: "AgentKey");

            migrationBuilder.CreateIndex(
                name: "IX_AiAnalyticsEvents_Intent",
                table: "AiAnalyticsEvents",
                column: "Intent");

            migrationBuilder.CreateIndex(
                name: "IX_AiKnowledgeDocuments_Category_Language_IsApproved",
                table: "AiKnowledgeDocuments",
                columns: new[] { "Category", "Language", "IsApproved" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AiAnalyticsEvents");

            migrationBuilder.DropTable(
                name: "AiKnowledgeDocuments");

            migrationBuilder.DropIndex(
                name: "IX_AiConversations_AgentKey",
                table: "AiConversations");

            migrationBuilder.DropIndex(
                name: "IX_AiConversations_Intent",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "AgentKey",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "ConfidenceScore",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "EscalationRecommended",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "GroundingSummary",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "GuardrailsJson",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "PromptVersion",
                table: "AiConversations");

            migrationBuilder.DropColumn(
                name: "SourcesJson",
                table: "AiConversations");
        }
    }
}
