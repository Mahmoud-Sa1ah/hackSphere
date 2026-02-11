export interface Tool {
    toolId: number;
    toolName: string;
    description: string;
    category: string;
    binaryName?: string;
    downloadUrl?: string;
    version?: string;
    lastUpdated?: string;
    author?: string;
    packageExtension?: string;
}
