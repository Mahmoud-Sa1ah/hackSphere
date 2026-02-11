export interface User {
    userId: number;
    name: string;
    email: string;
    role: string | 'Admin' | 'User';
    isTwoFactorEnabled: boolean;
    profilePhoto?: string;
    bio?: string;
    completedRooms?: number;
    points?: number;
    streak?: number;
}

export interface AuthState {
    token: string | null;
    user: User | null;
}
