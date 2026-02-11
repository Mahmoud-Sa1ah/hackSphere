using Microsoft.EntityFrameworkCore;
using PentestHub.API.Data.Models;

namespace PentestHub.API.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<Tool> Tools { get; set; }
    public DbSet<ToolInstallation> ToolInstallations { get; set; }
    public DbSet<ScanHistory> ScanHistory { get; set; }
    public DbSet<Report> Reports { get; set; }
    public DbSet<Lab> Labs { get; set; }
    public DbSet<LabResult> LabResults { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<AIConversation> AIConversations { get; set; }
    public DbSet<KnowledgeBase> KnowledgeBases { get; set; }
    public DbSet<DomainVerification> DomainVerifications { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.HasOne(e => e.Role)
                  .WithMany()
                  .HasForeignKey(e => e.RoleId);
        });

        // Role configuration
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId);
            entity.Property(e => e.RoleName).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.RoleName).IsUnique();
        });

        // Tool configuration
        modelBuilder.Entity<Tool>(entity =>
        {
            entity.HasKey(e => e.ToolId);
            entity.Property(e => e.ToolName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.BinaryName).HasMaxLength(200);
            entity.Property(e => e.RequiredExtensions).HasMaxLength(100);
            entity.Property(e => e.DownloadUrl).HasMaxLength(500);
            entity.Property(e => e.PackageData).HasColumnType("LONGBLOB"); // For MySQL to allow large files
            entity.Property(e => e.PackageExtension).HasMaxLength(10);
        });

        // ToolInstallation configuration
        modelBuilder.Entity<ToolInstallation>(entity =>
        {
            entity.HasKey(e => e.InstallationId);
            entity.Property(e => e.InstallationPath).IsRequired().HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Tool)
                  .WithMany()
                  .HasForeignKey(e => e.ToolId);
            // Ensure one installation per user per tool
            entity.HasIndex(e => new { e.UserId, e.ToolId }).IsUnique();
        });

        // ScanHistory configuration
        modelBuilder.Entity<ScanHistory>(entity =>
        {
            entity.HasKey(e => e.ScanId);
            entity.Property(e => e.Target).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Arguments).HasMaxLength(1000);
            entity.Property(e => e.RawOutput).HasColumnType("LONGTEXT");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Tool)
                  .WithMany()
                  .HasForeignKey(e => e.ToolId);
        });

        // Report configuration
        modelBuilder.Entity<Report>(entity =>
        {
            entity.HasKey(e => e.ReportId);
            entity.Property(e => e.PdfPath).HasMaxLength(500);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.ScanHistory)
                  .WithMany()
                  .HasForeignKey(e => e.ScanId)
                  .OnDelete(DeleteBehavior.SetNull);
        });

        // Lab configuration
        modelBuilder.Entity<Lab>(entity =>
        {
            entity.HasKey(e => e.LabId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(2000);
            entity.Property(e => e.Difficulty).HasMaxLength(20);
            entity.Property(e => e.Category).HasMaxLength(50);
            entity.Property(e => e.LabType).HasMaxLength(50);
        });

        // LabResult configuration
        modelBuilder.Entity<LabResult>(entity =>
        {
            entity.HasKey(e => e.ResultId);
            entity.Property(e => e.Details).HasColumnType("LONGTEXT");
            entity.Property(e => e.AIFeedback).HasColumnType("LONGTEXT");
            entity.HasOne(e => e.Lab)
                  .WithMany()
                  .HasForeignKey(e => e.LabId);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
        });

        // Notification configuration
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.HasKey(e => e.NotificationId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Message).HasMaxLength(1000);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
        });

        // AIConversation configuration
        modelBuilder.Entity<AIConversation>(entity =>
        {
            entity.HasKey(e => e.MsgId);
            entity.Property(e => e.MessageText).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.ResponseText).HasColumnType("TEXT");
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
        });

        // KnowledgeBase configuration
        modelBuilder.Entity<KnowledgeBase>(entity =>
        {
            entity.HasKey(e => e.KnowledgeBaseId);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired().HasColumnType("TEXT");
            entity.Property(e => e.Tags).HasMaxLength(500);
        });

        // DomainVerification configuration
        modelBuilder.Entity<DomainVerification>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DomainName).IsRequired().HasMaxLength(255);
            entity.Property(e => e.ProofPath).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
        });

        // Badge configuration
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.BadgeId);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IconUrl).HasMaxLength(500);
        });

        // UserBadge configuration
        modelBuilder.Entity<UserBadge>(entity =>
        {
            entity.HasKey(e => e.UserBadgeId);
            entity.HasOne(e => e.User)
                  .WithMany()
                  .HasForeignKey(e => e.UserId);
            entity.HasOne(e => e.Badge)
                  .WithMany()
                  .HasForeignKey(e => e.BadgeId);
        });

        // Seed initial data

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { RoleId = 1, RoleName = "Admin" },
            new Role { RoleId = 2, RoleName = "Learner" },
            new Role { RoleId = 3, RoleName = "Professional" }
        );

        // Seed Badges
        modelBuilder.Entity<Badge>().HasData(
            new Badge { BadgeId = 1, Name = "Bronze Badge", Description = "Earned 100 points", PointsRequired = 100, IconUrl = "/badges/bronze.png" },
            new Badge { BadgeId = 2, Name = "Silver Badge", Description = "Earned 500 points", PointsRequired = 500, IconUrl = "/badges/silver.png" },
            new Badge { BadgeId = 3, Name = "Gold Badge", Description = "Earned 1000 points", PointsRequired = 1000, IconUrl = "/badges/gold.png" }
        );

        // Seed Admin User (username: admin, password: admin)
        // Note: Password hash is generated at runtime, not in seed data
        // Admin will be created via migration or initial setup

        // Seed Tools (sample of 60 tools)
        var tools = new List<Tool>();
        var toolNames = new[]
        {
            ("Nmap", "Network mapper and port scanner", "Network"),
            ("Metasploit", "Penetration testing framework", "Exploitation"),
            ("Burp Suite", "Web application security testing", "Web"),
            ("Wireshark", "Network protocol analyzer", "Network"),
            ("Aircrack-ng", "WiFi security auditing", "Wireless"),
            ("John the Ripper", "Password cracking", "Password"),
            ("Hashcat", "Advanced password recovery", "Password"),
            ("SQLMap", "SQL injection tool", "Web"),
            ("Nikto", "Web server scanner", "Web"),
            ("OWASP ZAP", "Web app security scanner", "Web"),
            ("Hydra", "Network login cracker", "Password"),
            ("Nessus", "Vulnerability scanner", "Vulnerability"),
            ("OpenVAS", "Vulnerability management", "Vulnerability"),
            ("Masscan", "Fast port scanner", "Network"),
            ("Dirb", "Web content scanner", "Web"),
            ("Gobuster", "Directory/file brute forcer", "Web"),
            ("Nuclei", "Vulnerability scanner", "Vulnerability"),
            ("Subfinder", "Subdomain discovery", "Reconnaissance"),
            ("Amass", "In-depth attack surface mapping", "Reconnaissance"),
            ("Shodan CLI", "Shodan search interface", "Reconnaissance"),
            ("TheHarvester", "Email/subdomain/people harvester", "Reconnaissance"),
            ("Recon-ng", "Reconnaissance framework", "Reconnaissance"),
            ("Maltego", "Link analysis tool", "Reconnaissance"),
            ("Cobalt Strike", "Adversary simulation", "Exploitation"),
            ("Empire", "Post-exploitation framework", "Exploitation"),
            ("Mimikatz", "Credential extraction", "Post-Exploitation"),
            ("BloodHound", "Active Directory analysis", "Post-Exploitation"),
            ("Impacket", "Network protocols library", "Post-Exploitation"),
            ("Responder", "LLMNR/NBT-NS poisoner", "Post-Exploitation"),
            ("Bettercap", "Network attack framework", "Network"),
            ("Ettercap", "Network security tool", "Network"),
            ("EtherApe", "Network monitor", "Network"),
            ("Tcpdump", "Packet analyzer", "Network"),
            ("Netcat", "Network utility", "Network"),
            ("Socat", "Network relay", "Network"),
            ("SSLyze", "SSL/TLS scanner", "Web"),
            ("TestSSL", "SSL/TLS tester", "Web"),
            ("WhatWeb", "Web technology identifier", "Web"),
            ("WPScan", "WordPress vulnerability scanner", "Web"),
            ("XSStrike", "XSS detection suite", "Web"),
            ("Commix", "Command injection tool", "Web"),
            ("XXEinjector", "XXE injection tool", "Web"),
            ("JWT Tool", "JSON Web Token toolkit", "Web"),
            ("GitLeaks", "Git secrets scanner", "Code Analysis"),
            ("TruffleHog", "Git credential scanner", "Code Analysis"),
            ("Bandit", "Python security linter", "Code Analysis"),
            ("Semgrep", "Static analysis tool", "Code Analysis"),
            ("SonarQube", "Code quality and security", "Code Analysis"),
            ("Retire.js", "JavaScript dependency checker", "Code Analysis"),
            ("Snyk", "Vulnerability scanner", "Code Analysis"),
            ("Docker Bench", "Docker security checker", "Container"),
            ("Kube-bench", "Kubernetes security checker", "Container"),
            ("Kube-hunter", "Kubernetes penetration tool", "Container"),
            ("Trivy", "Container scanner", "Container"),
            ("Clair", "Container vulnerability scanner", "Container"),
            ("Lynis", "Security auditing tool", "System"),
            ("Chkrootkit", "Rootkit detector", "System"),
            ("Rkhunter", "Rootkit hunter", "System"),
            ("Fail2ban", "Intrusion prevention", "System"),
            ("Snort", "Intrusion detection", "System"),
            ("Suricata", "Network security monitoring", "System"),
            ("Zeek", "Network analysis framework", "System")
        };

        for (int i = 0; i < toolNames.Length; i++)
        {
            tools.Add(new Tool
            {
                ToolId = i + 1,
                ToolName = toolNames[i].Item1,
                Description = toolNames[i].Item2,
                Category = toolNames[i].Item3
            });
        }

        modelBuilder.Entity<Tool>().HasData(tools);

        // Seed Labs
        var labs = new List<Lab>
        {
            new Lab
            {
                LabId = 1,
                Title = "SQL Injection Challenge",
                Description = "Practice identifying and exploiting SQL injection vulnerabilities in a vulnerable web application. Learn to use tools like SQLMap and manual techniques.",
                Difficulty = "Beginner",
                Category = "Web Security",
                LabType = "Web Application",
                Points = 10
            },
            new Lab
            {
                LabId = 2,
                Title = "Network Penetration Testing",
                Description = "Perform a complete network penetration test on a simulated corporate network. Practice reconnaissance, scanning, exploitation, and post-exploitation.",
                Difficulty = "Intermediate",
                Category = "Network Security",
                LabType = "Network",
                Points = 25
            },
            new Lab
            {
                LabId = 3,
                Title = "XSS (Cross-Site Scripting) Exploitation",
                Description = "Learn to identify and exploit various types of XSS vulnerabilities including reflected, stored, and DOM-based XSS attacks.",
                Difficulty = "Beginner",
                Category = "Web Security",
                LabType = "Web Application",
                Points = 10
            },
            new Lab
            {
                LabId = 4,
                Title = "OSINT Investigation",
                Description = "Conduct an open-source intelligence gathering exercise. Practice using tools like theHarvester, Shodan, and social media reconnaissance.",
                Difficulty = "Beginner",
                Category = "Reconnaissance",
                LabType = "OSINT",
                Points = 10
            },
            new Lab
            {
                LabId = 5,
                Title = "Active Directory Exploitation",
                Description = "Practice advanced Active Directory attack techniques including Kerberoasting, AS-REP Roasting, and lateral movement strategies.",
                Difficulty = "Advanced",
                Category = "Post-Exploitation",
                LabType = "Windows",
                Points = 50
            },
            new Lab
            {
                LabId = 6,
                Title = "Buffer Overflow Exploitation",
                Description = "Learn buffer overflow exploitation techniques on Linux systems. Practice stack-based overflows, ROP chains, and shellcode development.",
                Difficulty = "Advanced",
                Category = "Exploitation",
                LabType = "Binary Exploitation",
                Points = 50
            },
            new Lab
            {
                LabId = 7,
                Title = "Wireless Network Security",
                Description = "Practice WiFi security assessment including WPA2/WPA3 cracking, rogue access point detection, and wireless protocol analysis.",
                Difficulty = "Intermediate",
                Category = "Wireless",
                LabType = "Wireless",
                Points = 25
            },
            new Lab
            {
                LabId = 8,
                Title = "API Security Testing",
                Description = "Learn to test REST and GraphQL APIs for common vulnerabilities including authentication bypass, authorization flaws, and injection attacks.",
                Difficulty = "Intermediate",
                Category = "Web Security",
                LabType = "API",
                Points = 25
            },
            new Lab
            {
                LabId = 9,
                Title = "Cryptography Challenge",
                Description = "Solve cryptographic challenges including cipher decryption, hash analysis, RSA key recovery, and digital signature verification.",
                Difficulty = "Intermediate",
                Category = "Cryptography",
                LabType = "Cryptography",
                Points = 25
            },
            new Lab
            {
                LabId = 10,
                Title = "Reverse Engineering",
                Description = "Practice reverse engineering techniques on malware samples and crackmes. Learn to use tools like Ghidra, IDA Pro, and x64dbg.",
                Difficulty = "Advanced",
                Category = "Reverse Engineering",
                LabType = "Reverse Engineering",
                Points = 50
            }
        };

        modelBuilder.Entity<Lab>().HasData(labs);
    }
}

