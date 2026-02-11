using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace PentestHub.API.Migrations
{
    /// <inheritdoc />
    public partial class AddToolPackageData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "KnowledgeBases",
                columns: table => new
                {
                    KnowledgeBaseId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Content = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Tags = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsPublished = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    CreatedBy = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeBases", x => x.KnowledgeBaseId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Labs",
                columns: table => new
                {
                    LabId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Difficulty = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LabType = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Labs", x => x.LabId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    RoleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    RoleName = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.RoleId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Tools",
                columns: table => new
                {
                    ToolId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ToolName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Category = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    BinaryName = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RequiredExtensions = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DownloadUrl = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PackageData = table.Column<byte[]>(type: "LONGBLOB", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tools", x => x.ToolId);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    UserId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    PasswordHash = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TwoFactorSecret = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsTwoFactorEnabled = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    ResetToken = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResetTokenExpires = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    RoleId = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastLogin = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.UserId);
                    table.ForeignKey(
                        name: "FK_Users_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "RoleId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "AIConversations",
                columns: table => new
                {
                    MsgId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    MessageText = table.Column<string>(type: "TEXT", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ResponseText = table.Column<string>(type: "TEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIConversations", x => x.MsgId);
                    table.ForeignKey(
                        name: "FK_AIConversations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "LabResults",
                columns: table => new
                {
                    ResultId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LabId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    CompletionTime = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Details = table.Column<string>(type: "LONGTEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AIFeedback = table.Column<string>(type: "LONGTEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LabResults", x => x.ResultId);
                    table.ForeignKey(
                        name: "FK_LabResults_Labs_LabId",
                        column: x => x.LabId,
                        principalTable: "Labs",
                        principalColumn: "LabId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LabResults_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Message = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    IsRead = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ScanHistory",
                columns: table => new
                {
                    ScanId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    Target = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Arguments = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    RawOutput = table.Column<string>(type: "LONGTEXT", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AISummary = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    AINextSteps = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScanHistory", x => x.ScanId);
                    table.ForeignKey(
                        name: "FK_ScanHistory_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScanHistory_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "ToolInstallations",
                columns: table => new
                {
                    InstallationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ToolId = table.Column<int>(type: "int", nullable: false),
                    InstallationPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    InstalledAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    LastVerifiedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    IsVerified = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ToolInstallations", x => x.InstallationId);
                    table.ForeignKey(
                        name: "FK_ToolInstallations_Tools_ToolId",
                        column: x => x.ToolId,
                        principalTable: "Tools",
                        principalColumn: "ToolId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ToolInstallations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    ReportId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    ScanId = table.Column<int>(type: "int", nullable: false),
                    PdfPath = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    CreatedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_Reports_ScanHistory_ScanId",
                        column: x => x.ScanId,
                        principalTable: "ScanHistory",
                        principalColumn: "ScanId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reports_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "Labs",
                columns: new[] { "LabId", "Category", "Description", "Difficulty", "LabType", "Title" },
                values: new object[,]
                {
                    { 1, "Web Security", "Practice identifying and exploiting SQL injection vulnerabilities in a vulnerable web application. Learn to use tools like SQLMap and manual techniques.", "Beginner", "Web Application", "SQL Injection Challenge" },
                    { 2, "Network Security", "Perform a complete network penetration test on a simulated corporate network. Practice reconnaissance, scanning, exploitation, and post-exploitation.", "Intermediate", "Network", "Network Penetration Testing" },
                    { 3, "Web Security", "Learn to identify and exploit various types of XSS vulnerabilities including reflected, stored, and DOM-based XSS attacks.", "Beginner", "Web Application", "XSS (Cross-Site Scripting) Exploitation" },
                    { 4, "Reconnaissance", "Conduct an open-source intelligence gathering exercise. Practice using tools like theHarvester, Shodan, and social media reconnaissance.", "Beginner", "OSINT", "OSINT Investigation" },
                    { 5, "Post-Exploitation", "Practice advanced Active Directory attack techniques including Kerberoasting, AS-REP Roasting, and lateral movement strategies.", "Advanced", "Windows", "Active Directory Exploitation" },
                    { 6, "Exploitation", "Learn buffer overflow exploitation techniques on Linux systems. Practice stack-based overflows, ROP chains, and shellcode development.", "Advanced", "Binary Exploitation", "Buffer Overflow Exploitation" },
                    { 7, "Wireless", "Practice WiFi security assessment including WPA2/WPA3 cracking, rogue access point detection, and wireless protocol analysis.", "Intermediate", "Wireless", "Wireless Network Security" },
                    { 8, "Web Security", "Learn to test REST and GraphQL APIs for common vulnerabilities including authentication bypass, authorization flaws, and injection attacks.", "Intermediate", "API", "API Security Testing" },
                    { 9, "Cryptography", "Solve cryptographic challenges including cipher decryption, hash analysis, RSA key recovery, and digital signature verification.", "Intermediate", "Cryptography", "Cryptography Challenge" },
                    { 10, "Reverse Engineering", "Practice reverse engineering techniques on malware samples and crackmes. Learn to use tools like Ghidra, IDA Pro, and x64dbg.", "Advanced", "Reverse Engineering", "Reverse Engineering" }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "RoleId", "RoleName" },
                values: new object[,]
                {
                    { 1, "Admin" },
                    { 2, "Learner" },
                    { 3, "Professional" }
                });

            migrationBuilder.InsertData(
                table: "Tools",
                columns: new[] { "ToolId", "BinaryName", "Category", "Description", "DownloadUrl", "PackageData", "RequiredExtensions", "ToolName" },
                values: new object[,]
                {
                    { 1, null, "Network", "Network mapper and port scanner", null, null, null, "Nmap" },
                    { 2, null, "Exploitation", "Penetration testing framework", null, null, null, "Metasploit" },
                    { 3, null, "Web", "Web application security testing", null, null, null, "Burp Suite" },
                    { 4, null, "Network", "Network protocol analyzer", null, null, null, "Wireshark" },
                    { 5, null, "Wireless", "WiFi security auditing", null, null, null, "Aircrack-ng" },
                    { 6, null, "Password", "Password cracking", null, null, null, "John the Ripper" },
                    { 7, null, "Password", "Advanced password recovery", null, null, null, "Hashcat" },
                    { 8, null, "Web", "SQL injection tool", null, null, null, "SQLMap" },
                    { 9, null, "Web", "Web server scanner", null, null, null, "Nikto" },
                    { 10, null, "Web", "Web app security scanner", null, null, null, "OWASP ZAP" },
                    { 11, null, "Password", "Network login cracker", null, null, null, "Hydra" },
                    { 12, null, "Vulnerability", "Vulnerability scanner", null, null, null, "Nessus" },
                    { 13, null, "Vulnerability", "Vulnerability management", null, null, null, "OpenVAS" },
                    { 14, null, "Network", "Fast port scanner", null, null, null, "Masscan" },
                    { 15, null, "Web", "Web content scanner", null, null, null, "Dirb" },
                    { 16, null, "Web", "Directory/file brute forcer", null, null, null, "Gobuster" },
                    { 17, null, "Vulnerability", "Vulnerability scanner", null, null, null, "Nuclei" },
                    { 18, null, "Reconnaissance", "Subdomain discovery", null, null, null, "Subfinder" },
                    { 19, null, "Reconnaissance", "In-depth attack surface mapping", null, null, null, "Amass" },
                    { 20, null, "Reconnaissance", "Shodan search interface", null, null, null, "Shodan CLI" },
                    { 21, null, "Reconnaissance", "Email/subdomain/people harvester", null, null, null, "TheHarvester" },
                    { 22, null, "Reconnaissance", "Reconnaissance framework", null, null, null, "Recon-ng" },
                    { 23, null, "Reconnaissance", "Link analysis tool", null, null, null, "Maltego" },
                    { 24, null, "Exploitation", "Adversary simulation", null, null, null, "Cobalt Strike" },
                    { 25, null, "Exploitation", "Post-exploitation framework", null, null, null, "Empire" },
                    { 26, null, "Post-Exploitation", "Credential extraction", null, null, null, "Mimikatz" },
                    { 27, null, "Post-Exploitation", "Active Directory analysis", null, null, null, "BloodHound" },
                    { 28, null, "Post-Exploitation", "Network protocols library", null, null, null, "Impacket" },
                    { 29, null, "Post-Exploitation", "LLMNR/NBT-NS poisoner", null, null, null, "Responder" },
                    { 30, null, "Network", "Network attack framework", null, null, null, "Bettercap" },
                    { 31, null, "Network", "Network security tool", null, null, null, "Ettercap" },
                    { 32, null, "Network", "Network monitor", null, null, null, "EtherApe" },
                    { 33, null, "Network", "Packet analyzer", null, null, null, "Tcpdump" },
                    { 34, null, "Network", "Network utility", null, null, null, "Netcat" },
                    { 35, null, "Network", "Network relay", null, null, null, "Socat" },
                    { 36, null, "Web", "SSL/TLS scanner", null, null, null, "SSLyze" },
                    { 37, null, "Web", "SSL/TLS tester", null, null, null, "TestSSL" },
                    { 38, null, "Web", "Web technology identifier", null, null, null, "WhatWeb" },
                    { 39, null, "Web", "WordPress vulnerability scanner", null, null, null, "WPScan" },
                    { 40, null, "Web", "XSS detection suite", null, null, null, "XSStrike" },
                    { 41, null, "Web", "Command injection tool", null, null, null, "Commix" },
                    { 42, null, "Web", "XXE injection tool", null, null, null, "XXEinjector" },
                    { 43, null, "Web", "JSON Web Token toolkit", null, null, null, "JWT Tool" },
                    { 44, null, "Code Analysis", "Git secrets scanner", null, null, null, "GitLeaks" },
                    { 45, null, "Code Analysis", "Git credential scanner", null, null, null, "TruffleHog" },
                    { 46, null, "Code Analysis", "Python security linter", null, null, null, "Bandit" },
                    { 47, null, "Code Analysis", "Static analysis tool", null, null, null, "Semgrep" },
                    { 48, null, "Code Analysis", "Code quality and security", null, null, null, "SonarQube" },
                    { 49, null, "Code Analysis", "JavaScript dependency checker", null, null, null, "Retire.js" },
                    { 50, null, "Code Analysis", "Vulnerability scanner", null, null, null, "Snyk" },
                    { 51, null, "Container", "Docker security checker", null, null, null, "Docker Bench" },
                    { 52, null, "Container", "Kubernetes security checker", null, null, null, "Kube-bench" },
                    { 53, null, "Container", "Kubernetes penetration tool", null, null, null, "Kube-hunter" },
                    { 54, null, "Container", "Container scanner", null, null, null, "Trivy" },
                    { 55, null, "Container", "Container vulnerability scanner", null, null, null, "Clair" },
                    { 56, null, "System", "Security auditing tool", null, null, null, "Lynis" },
                    { 57, null, "System", "Rootkit detector", null, null, null, "Chkrootkit" },
                    { 58, null, "System", "Rootkit hunter", null, null, null, "Rkhunter" },
                    { 59, null, "System", "Intrusion prevention", null, null, null, "Fail2ban" },
                    { 60, null, "System", "Intrusion detection", null, null, null, "Snort" },
                    { 61, null, "System", "Network security monitoring", null, null, null, "Suricata" },
                    { 62, null, "System", "Network analysis framework", null, null, null, "Zeek" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIConversations_UserId",
                table: "AIConversations",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_LabId",
                table: "LabResults",
                column: "LabId");

            migrationBuilder.CreateIndex(
                name: "IX_LabResults_UserId",
                table: "LabResults",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_ScanId",
                table: "Reports",
                column: "ScanId");

            migrationBuilder.CreateIndex(
                name: "IX_Reports_UserId",
                table: "Reports",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Roles_RoleName",
                table: "Roles",
                column: "RoleName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistory_ToolId",
                table: "ScanHistory",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ScanHistory_UserId",
                table: "ScanHistory",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolInstallations_ToolId",
                table: "ToolInstallations",
                column: "ToolId");

            migrationBuilder.CreateIndex(
                name: "IX_ToolInstallations_UserId_ToolId",
                table: "ToolInstallations",
                columns: new[] { "UserId", "ToolId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RoleId",
                table: "Users",
                column: "RoleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AIConversations");

            migrationBuilder.DropTable(
                name: "KnowledgeBases");

            migrationBuilder.DropTable(
                name: "LabResults");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "ToolInstallations");

            migrationBuilder.DropTable(
                name: "Labs");

            migrationBuilder.DropTable(
                name: "ScanHistory");

            migrationBuilder.DropTable(
                name: "Tools");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");
        }
    }
}
