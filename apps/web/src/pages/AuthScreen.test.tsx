import { render, screen, fireEvent } from "@testing-library/react";
import { describe, expect, it, vi } from "vitest";
import { AuthScreen } from "./AuthScreen";

describe("AuthScreen", () => {
  it("renders login form by default and toggles to registration form", () => {
    const handleAuthenticated = vi.fn();
    render(<AuthScreen onAuthenticated={handleAuthenticated} />);

    // 1. Assert default title and fields
    expect(screen.getByRole("heading", { name: /sign in to operations/i })).toBeInTheDocument();
    expect(screen.queryByLabelText(/name/i)).not.toBeInTheDocument();
    expect(screen.getByLabelText(/email/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/password/i)).toBeInTheDocument();

    // 2. Click Register tab to switch forms
    const registerTab = screen.getByRole("tab", { name: /register/i });
    fireEvent.click(registerTab);

    // 3. Assert registration fields appear
    expect(screen.getByRole("heading", { name: /create a tournament account/i })).toBeInTheDocument();
    expect(screen.getByLabelText(/name/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/role/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/preferred language/i)).toBeInTheDocument();
    expect(screen.getByLabelText(/accessibility preference/i)).toBeInTheDocument();
  });
});
