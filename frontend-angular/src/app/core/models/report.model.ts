export interface Report {
    reportId: number;
    userId: number;
    scanId?: number;
    pdfPath?: string;
    createdAt: Date;
    user?: any; // Avoiding circular dependency for now
    scanHistory?: any;
    title?: string; // Optional if we want to display a title
    type?: string;
}
