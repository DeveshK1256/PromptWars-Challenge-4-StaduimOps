import { render, screen, fireEvent, waitFor } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { NotificationConsole } from "./NotificationConsole";
import { ApiClient } from "../services/api";

// Mock the ApiClient
const mockNotifications = [
  {
    id: "n1",
    title: "Test Alert",
    message: "Test Message details",
    type: "Operations",
    priority: "High",
    isRead: false,
    externalDeliveryStatus: "Delivered",
  },
];

describe("NotificationConsole", () => {
  it("loads and displays notifications from client", async () => {
    const mockClient = {
      notifications: vi.fn().mockResolvedValue(mockNotifications),
      broadcast: vi.fn().mockResolvedValue({}),
    } as unknown as ApiClient;

    render(<NotificationConsole client={mockClient} />);

    // 1. Assert loader runs and requests notifications
    expect(mockClient.notifications).toHaveBeenCalledTimes(1);

    // 2. Wait for notifications to load
    await waitFor(() => {
      expect(screen.getByText("Test Alert")).toBeInTheDocument();
    });

    expect(screen.getByText("Test Message details")).toBeInTheDocument();
    expect(screen.getByText("High")).toBeInTheDocument();
  });
});
