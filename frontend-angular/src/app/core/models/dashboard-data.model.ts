export interface DashboardData {
    recentScans: Array<{
        scanId: number;
        toolName: string;
        target: string;
        createdAt: string;
        hasAISummary: boolean;
    }>;
    activityFeed: Array<{
        type: string;
        message: string;
        time: string;
        createdAt: string;
    }>;
    unreadNotifications: number;
    launcherStatus: {
        isOnline: boolean;
        status: string;
    };
    statistics: {
        totalScans: number;
        totalLabs: number;
        scansChange: number;
        vulnerabilitiesFound: number;
        vulnerabilitiesChange: number;
        toolsUsed: number;
        toolsUsedChange: number;
        activeUsers: number;
    };
    toolCategoryUsage: Array<{
        category: string;
        usage: number;
        tools: number;
    }>;
    systemHealthData: Array<{
        time: string;
        value: number;
    }>;
}
