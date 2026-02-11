export interface ToolConfig {
    iconName: string;
    color: string;
    bg: string;
    placeholder: string;
    payloads: string[];
    kbWhatIs: string;
    kbPrevention: string[];
}

export const TOOL_CONFIGS: Record<string, ToolConfig> = {
    "Web Security": {
        iconName: "lucideGlobe",
        color: "text-blue-500",
        bg: "bg-blue-50",
        placeholder: "https://example.com/vulnerable-page.php?id=1",
        payloads: ["' OR '1'='1", "<script>alert(1)</script>", "../../../etc/passwd", "admin' --"],
        kbWhatIs: "Web Security tools analyze web applications for vulnerabilities like SQL Injection, XSS, and more.",
        kbPrevention: [
            "Validate and sanitize all user inputs",
            "Use Content Security Policy (CSP) headers",
            "Regularly scan with automated tools",
            "Keep dependencies updated"
        ]
    },
    "Network": {
        iconName: "lucideActivity",
        color: "text-green-500",
        bg: "bg-green-50",
        placeholder: "192.168.1.1 or 10.0.0.0/24",
        payloads: ["-sV -p-", "-A -T4", "--script vuln", "-sn"],
        kbWhatIs: "Network tools scan for open ports, active hosts, and service versions.",
        kbPrevention: [
            "Close unnecessary ports and services",
            "Implement strict firewall rules",
            "Segment critical network zones",
            "Use IDS/IPS for monitoring"
        ]
    },
    "Exploitation": {
        iconName: "lucideZap",
        color: "text-red-500",
        bg: "bg-red-50",
        placeholder: "Target IP: 192.168.1.10 or RHOSTS",
        payloads: ["windows/meterpreter/reverse_tcp", "linux/x64/shell_reverse_tcp", "use exploit/multi/handler", "check"],
        kbWhatIs: "Exploitation tools are used to verify if a vulnerability is actually exploitable.",
        kbPrevention: [
            "Patch systems immediately upon release",
            "Run services with least privilege",
            "Enable Data Execution Prevention (DEP)",
            "Use Endpoint Detection and Response (EDR)"
        ]
    },
    "Password": {
        iconName: "lucideHash",
        color: "text-yellow-500",
        bg: "bg-yellow-50",
        placeholder: "/path/to/hash.txt or target_user",
        payloads: ["--wordlist=rockyou.txt", "--format=md5", "--incremental", "hydra -l admin -P pass.txt"],
        kbWhatIs: "Password and Brute Force tools test the strength of credentials.",
        kbPrevention: [
            "Enforce strong, complex password policies",
            "Implement Multi-Factor Authentication (MFA)",
            "Use account lockout policies",
            "Monitor for repeated login failures"
        ]
    },
    "Wireless": {
        iconName: "lucideWifi",
        color: "text-purple-500",
        bg: "bg-purple-50",
        placeholder: "Interface: wlan0mon or BSSID",
        payloads: ["--deauth 10", "--channel 6", "--bssid 00:11:22:33:44:55", "capture.cap"],
        kbWhatIs: "Wireless tools audit WiFi security. They can capture handshakes, inject packets, and test WPA/WPA2 encryption.",
        kbPrevention: [
            "Use WPA3 encryption where possible",
            "Disable WPS (WiFi Protected Setup)",
            "Isolate Guest networks",
            "Monitor for Rogue Access Points"
        ]
    },
    "Cryptography": {
        iconName: "lucideKey",
        color: "text-indigo-500",
        bg: "bg-indigo-50",
        placeholder: "Hash or Encrypted String",
        payloads: ["base64 decode", "rot13", "detect-hash", "verify-sig"],
        kbWhatIs: "Cryptography tools analyze encryption, hashes, and signatures.",
        kbPrevention: [
            "Use strong, modern algorithms (AES-256, RSA-2048+)",
            "Never roll your own crypto",
            "Securely manage encryption keys",
            "Salt hashes properly"
        ]
    }
};

export const DEFAULT_CONFIG: ToolConfig = {
    iconName: "lucideShield",
    color: "text-gray-500",
    bg: "bg-gray-50",
    placeholder: "Target arguments or parameters...",
    payloads: ["--help", "--version", "--verbose", "--scan"],
    kbWhatIs: "This security tool allows for specialized analysis and testing. Consult the documentation for specific usage instructions.",
    kbPrevention: [
        "Follow the Principle of Least Privilege",
        "Maintain comprehensive audit logs",
        "Keep systems patched and updated",
        "Conduct regular security assessments"
    ]
};
