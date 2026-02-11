export interface Lab {
    labId: number;
    title: string;
    description: string;
    difficulty: string;
    category: string;
    labType: string;
    points: number;
}

export interface LabResult {
    resultId: number;
    labId: number;
    score: number | null;
    completionTime: string | null;
    details: string;
    aiFeedback: string | null;
    lab: Lab;
}

export interface SolvedUser {
    userId: number;
    name: string;
    profilePhoto: string | null;
    dateSolved: string;
}
