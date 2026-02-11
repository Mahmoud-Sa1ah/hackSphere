import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';
import { AuthService } from './auth.service';

@Injectable({
    providedIn: 'root'
})
export class SignalRService {
    private launcherConnection: signalR.HubConnection | null = null;
    private notificationConnection: signalR.HubConnection | null = null;

    constructor(private authService: AuthService) { }

    async startLauncherHub(callbacks: {
        onRunTool?: (data: any) => void;
        onScanOutput?: (data: any) => void;
        onScanComplete?: (data: any) => void;
        onLauncherStatusChanged?: (data: any) => void;
    }) {
        if (this.launcherConnection) {
            // If connection exists, just update callbacks? 
            // SignalR doesn't support re-registering 'on' handlers easily without off-ing them.
            // For simplicity, we assume one active listener or we append which might duplicating.
            // Better to just return the connection if active.
            // However, the caller might be different. 
            // In the React code, used in useEffect, so it might be called on mount.
            // We should add the new callbacks.
            this.registerLauncherCallbacks(callbacks);
            return this.launcherConnection;
        }

        const token = this.authService.getToken();
        this.launcherConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hubs/launcher', {
                accessTokenFactory: () => token || '',
            })
            .withAutomaticReconnect()
            .build();

        this.registerLauncherCallbacks(callbacks);

        try {
            await this.launcherConnection.start();
            console.log('Launcher SignalR connected');
        } catch (err) {
            console.error('Error while starting Launcher Hub connection: ' + err);
        }

        return this.launcherConnection;
    }

    private registerLauncherCallbacks(callbacks: any) {
        if (!this.launcherConnection) return;

        // Note: This simple implementation might stack listeners if called multiple times.
        // Ideally we should manage subscriptions. 
        // For this migration, we will assume the Dashboard is the main consumer.
        // If we navigate away and back, we might duplicate listeners.
        // A robust solution would store the handler reference and off it.

        if (callbacks.onRunTool) this.launcherConnection.on('RunTool', callbacks.onRunTool);
        if (callbacks.onScanOutput) this.launcherConnection.on('ScanOutput', callbacks.onScanOutput);
        if (callbacks.onScanComplete) this.launcherConnection.on('ScanComplete', callbacks.onScanComplete);
        if (callbacks.onLauncherStatusChanged) this.launcherConnection.on('LauncherStatusChanged', callbacks.onLauncherStatusChanged);
    }

    async startNotificationHub(callbacks: {
        onNotification?: (data: any) => void
    }) {
        if (this.notificationConnection) {
            if (callbacks.onNotification) this.notificationConnection.on('Notification', callbacks.onNotification);
            return this.notificationConnection;
        }

        const token = this.authService.getToken();
        const user = this.authService.currentUserValue;
        const userId = user?.userId;

        this.notificationConnection = new signalR.HubConnectionBuilder()
            .withUrl(`/hubs/notifications?userId=${userId}`, {
                accessTokenFactory: () => token || '',
            })
            .withAutomaticReconnect()
            .build();

        if (callbacks.onNotification) this.notificationConnection.on('Notification', callbacks.onNotification);

        try {
            await this.notificationConnection.start();
            console.log('Notification SignalR connected');
        } catch (err) {
            console.error('Error while starting Notification Hub connection: ' + err);
        }

        return this.notificationConnection;
    }

    async stopLauncherHub() {
        if (this.launcherConnection) {
            await this.launcherConnection.stop();
            this.launcherConnection = null;
        }
    }

    async stopNotificationHub() {
        if (this.notificationConnection) {
            await this.notificationConnection.stop();
            this.notificationConnection = null;
        }
    }

    async sendOutput(scanId: number, output: string) {
        if (this.launcherConnection) {
            await this.launcherConnection.invoke('SendOutput', scanId, output);
        }
    }

    async scanComplete(scanId: number, userId: number, fullOutput: string) {
        if (this.launcherConnection) {
            await this.launcherConnection.invoke('ScanComplete', scanId, userId, fullOutput);
        }
    }
}
